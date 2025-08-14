using ConfigComponent.Dtos;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SharedLibrary.HelperServices;
using System.Security.Claims;
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

        public async Task CreateOrUpdateUserConfigAsync(Dictionary<string, object> overrides)
        {
            var bsonOverrides = new BsonDocument();
            foreach (var kvp in overrides)
            {
                bsonOverrides.Add(kvp.Key, BsonValue.Create(kvp.Value));
            }

            var filter = Builders<BsonDocument>.Filter.Eq("userId", _userId);
            var updateDoc = new BsonDocument
            {
                { "userId", _userId },
                { "overrides", bsonOverrides }
            };

            await _configsCollection.ReplaceOneAsync(filter, updateDoc, new ReplaceOptions { IsUpsert = true });
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