using ConfigComponent.Dtos;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SharedLibrary.HelperServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ConfigComponent.Services
{
    public class ConfigService
    {
        private readonly IMongoCollection<BsonDocument> _configsCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _userId;
        public ConfigService(MongoDbService _mongoDbService, IHttpContextAccessor httpContextAccessor)
        {
            var database = _mongoDbService.GetDatabase();
            _configsCollection = database.GetCollection<BsonDocument>("Configs");
            _httpContextAccessor = httpContextAccessor;
            _userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }

        public async Task<ConfigResponseDto> GetDefaultAndUserConfigAsync()
        {
            var objectId = new ObjectId("68305836433f0432cba78449");

            var defaultConfigDoc = await _configsCollection
                .Find(x => x["_id"] == objectId)
                .FirstOrDefaultAsync();

            var userConfigDoc = await _configsCollection
                .Find(x => x["userId"] == _userId)
                .FirstOrDefaultAsync();

            var defaultConfig = JObject.Parse(defaultConfigDoc.ToJson());

            JObject userOverrides = [];

            if (userConfigDoc != null && userConfigDoc.TryGetValue("overrides", out var overridesBson))
            {
                userOverrides = JObject.Parse(overridesBson.ToJson());
            }

            ApplyOrderingToAllTables(defaultConfig["tables"] as JObject, userOverrides);

            return new ConfigResponseDto
            {
                DefaultConfig = defaultConfig,
                UserConfig = userOverrides
            };
        }

        public async Task CreateOrUpdateUserConfigAsync(Dictionary<string, object?> overrides)
        {
            if (overrides == null) return;

            var filter = Builders<BsonDocument>.Filter.Eq("userId", _userId);
            var existing = await _configsCollection.Find(filter).FirstOrDefaultAsync();

            var doc = existing ?? new BsonDocument { ["userId"] = _userId };
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
                .SetOnInsert("userId", _userId);

            await _configsCollection.UpdateOneAsync(
                filter,
                update,
                new UpdateOptions { IsUpsert = true }
            );
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

        public async Task DeleteUserTableConfigAsync(string tableKey)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("userId", _userId);
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

            await _configsCollection.UpdateOneAsync(filter, pipeline);
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
    }
}