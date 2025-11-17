using ConfigComponent.Dtos;
using ConfigComponent.HelperServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConfigComponent.Services
{
    public class ConfigService
    {
        private readonly IMongoCollection<BsonDocument> _configsCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        private static readonly FilterDefinition<BsonDocument> DefaultConfigFilter = Builders<BsonDocument>.Filter.Exists("userId", false);
        private const string DefaultConfigCacheKey = "config:default-document";
        private static readonly TimeSpan DefaultConfigCacheDuration = TimeSpan.FromMinutes(5);

        private static bool _indexesEnsured;
        private static readonly object IndexLock = new();

        public ConfigService(
            MongoDbService mongoDbService,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache)
        {
            var database = mongoDbService.GetDatabase();
            _configsCollection = database.GetCollection<BsonDocument>("Configs");
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;

            EnsureIndexes();
        }

        public async Task<ConfigResponseDto> GetDefaultAndUserConfigAsync(CancellationToken cancellationToken = default)
        {
            var defaultConfigTask = GetDefaultConfigSnapshotAsync(cancellationToken);
            var userOverridesTask = GetUserOverridesAsync(cancellationToken);

            await Task.WhenAll(defaultConfigTask, userOverridesTask).ConfigureAwait(false);

            var defaultConfig = defaultConfigTask.Result;
            var userOverrides = userOverridesTask.Result;

            ApplyOrderingToAllTables(defaultConfig["tables"] as JObject, userOverrides);

            return new ConfigResponseDto
            {
                DefaultConfig = defaultConfig,
                UserConfig = userOverrides
            };
        }

        public async Task CreateOrUpdateUserConfigAsync(Dictionary<string, object?> overrides, CancellationToken cancellationToken = default)
        {
            if (overrides == null) return;

            var userId = GetRequiredUserId();
            if (overrides.Count == 0)
            {
                await EnsureUserDocumentExistsAsync(userId, cancellationToken).ConfigureAwait(false);
                return;
            }

            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var projection = Builders<BsonDocument>.Projection.Include("overrides").Exclude("_id");

            var existing = await _configsCollection
                .Find(filter)
                .Project<BsonDocument>(projection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            var doc = existing ?? new BsonDocument();
            var ovDoc = (doc.TryGetValue("overrides", out var ovVal) && ovVal.IsBsonDocument)
                ? ovVal.AsBsonDocument
                : [];

            foreach (var (key, val) in overrides)
            {
                if (val is null || (val is JsonElement je && je.ValueKind == JsonValueKind.Null))
                {
                    if (ovDoc.Contains(key)) ovDoc.Remove(key);
                    continue;
                }

                ovDoc[key] = ToBson(val);
            }

            var update = Builders<BsonDocument>.Update
                .Set("overrides", ovDoc)
                .SetOnInsert("userId", userId);

            await _configsCollection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken
            ).ConfigureAwait(false);

            InvalidateUserCache(userId);
        }

        public async Task DeleteUserTableConfigAsync(string tableKey, CancellationToken cancellationToken = default)
        {
            var userId = GetRequiredUserId();
            if (string.IsNullOrWhiteSpace(tableKey)) return;

            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var regex = $"^tables\\.{Regex.Escape(tableKey)}\\.";

            var setStage = new BsonDocument("$set",
                new BsonDocument("overrides",
                     new BsonDocument("$arrayToObject",
                         new BsonDocument("$filter", new BsonDocument
                         {
                             { "input", new BsonDocument("$objectToArray", "$overrides") },
                             { "as", "kv" },
                             { "cond", new BsonDocument("$not",
                                 new BsonDocument("$regexMatch", new BsonDocument
                                 {
                                     { "input", "$$kv.k" },
                                     { "regex", regex }
                                 })
                             )}
                         })
                     )
                )
            );

            var pipeline = PipelineDefinition<BsonDocument, BsonDocument>.Create([setStage]);

            await _configsCollection.UpdateOneAsync(filter, pipeline, cancellationToken: cancellationToken).ConfigureAwait(false);
            InvalidateUserCache(userId);
        }

        private static BsonValue ToBson(object val)
        {
            if (val is JsonElement el) return FromJsonElement(el);
            return BsonValue.Create(val);
        }

        private static BsonValue FromJsonElement(JsonElement el) =>
            el.ValueKind switch
            {
                JsonValueKind.Null => BsonNull.Value,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => el.GetString(),
                JsonValueKind.Number => el.TryGetInt64(out var l) ? (BsonValue)l :
                                        el.TryGetDouble(out var d) ? d : (BsonValue)el.GetDecimal(),
                JsonValueKind.Array => new BsonArray(el.EnumerateArray().Select(FromJsonElement)),
                JsonValueKind.Object => JsonObjToBson(el),
                _ => BsonNull.Value
            };

        private static BsonDocument JsonObjToBson(JsonElement obj)
        {
            var d = new BsonDocument();
            foreach (var p in obj.EnumerateObject())
                d[p.Name] = FromJsonElement(p.Value);
            return d;
        }

        private void ApplyColumnOrdering(JObject? table)
        {
            if (table == null) return;

            if (table["columns"] is not JObject columns || table["columnsOrder"] is not JObject orderInfo) return;

            var orderedProps = orderInfo.Properties()
                .OrderBy(p => (int)p.Value)
                .Select(p => new JProperty(p.Name, columns[p.Name]))
                .Where(p => p.Value != null)
                .ToList();

            table["columns"] = new JObject(orderedProps);
        }

        private void ApplyColumnOrderingWithOverrides(JObject? table, JObject userOverrides, string tableId)
        {
            if (table == null) return;

            if (table["columns"] is not JObject columns) return;

            var prefix = $"tables.{tableId}.columnsOrder.";
            var columnOrderOverrides = userOverrides.Properties()
                .Where(p => p.Name.StartsWith(prefix))
                .ToDictionary(
                    p => p.Name[prefix.Length..],
                    p => (int)p.Value
                );

            if (columnOrderOverrides.Count == 0) return;

            var orderedProps = columnOrderOverrides
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => new JProperty(kvp.Key, columns[kvp.Key]))
                .Where(p => p.Value != null)
                .ToList();

            table["columns"] = new JObject(orderedProps);
        }

        private void ApplyOrderingToAllTables(JObject? tables, JObject userOverrides)
        {
            if (tables == null) return;

            foreach (var tableProperty in tables.Properties())
            {
                var tableId = tableProperty.Name;

                if (tableProperty.Value is not JObject table) continue;

                ApplyColumnOrdering(table);

                ApplyColumnOrderingWithOverrides(table, userOverrides, tableId);
            }
        }

        private async Task<JObject> GetDefaultConfigSnapshotAsync(CancellationToken cancellationToken)
        {
            var cached = await _memoryCache.GetOrCreateAsync(
                DefaultConfigCacheKey,
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = DefaultConfigCacheDuration;

                    var defaultConfigDoc = await _configsCollection
                        .Find(DefaultConfigFilter)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false)
                        ?? throw new InvalidOperationException("Default configuration document was not found in MongoDB.");

                    return JObject.Parse(defaultConfigDoc.ToJson());
                }).ConfigureAwait(false);

            return (JObject)cached.DeepClone();
        }

        private async Task<JObject> GetUserOverridesAsync(CancellationToken cancellationToken)
        {
            var userId = TryGetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return [];
            }

            if (_memoryCache.TryGetValue(BuildUserCacheKey(userId), out JObject cachedOverrides) && cachedOverrides is not null)
            {
                return (JObject)cachedOverrides.DeepClone();
            }

            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var projection = Builders<BsonDocument>.Projection.Include("overrides").Exclude("_id");

            var userConfigDoc = await _configsCollection
                .Find(filter)
                .Project<BsonDocument>(projection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (userConfigDoc == null || !userConfigDoc.TryGetValue("overrides", out var overridesBson) || !overridesBson.IsBsonDocument)
            {
                _memoryCache.Set(BuildUserCacheKey(userId), (JObject)[], DefaultConfigCacheDuration);
                return [];
            }

            var overrides = JObject.Parse(overridesBson.ToJson());
            _memoryCache.Set(BuildUserCacheKey(userId), overrides.DeepClone(), DefaultConfigCacheDuration);
            return (JObject)overrides.DeepClone();
        }

        private string GetRequiredUserId()
        {
            var userId = TryGetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return userId;
        }

        private string? TryGetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue(ClaimTypes.Sid)
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task EnsureUserDocumentExistsAsync(string userId, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
            var update = Builders<BsonDocument>.Update
                .SetOnInsert("userId", userId)
                .SetOnInsert("overrides", new BsonDocument());

            await _configsCollection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true },
                cancellationToken
            ).ConfigureAwait(false);
        }

        private void EnsureIndexes()
        {
            if (_indexesEnsured) return;

            lock (IndexLock)
            {
                if (_indexesEnsured) return;

                var indexModel = new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("userId"),
                    new CreateIndexOptions
                    {
                        Name = "ux_configs_userId",
                        Unique = true,
                        Sparse = true
                    });

                try
                {
                    _configsCollection.Indexes.CreateOne(indexModel);
                }
                catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict" || ex.CodeName == "IndexKeySpecsConflict")
                {
                    // Index already exists with different options.
                }

                _indexesEnsured = true;
            }
        }

        private static string BuildUserCacheKey(string userId) => $"config:user:{userId}";

        private void InvalidateUserCache(string userId) => _memoryCache.Remove(BuildUserCacheKey(userId));
    }
}