using ImportExportComponent.Dtos;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using TableComponent.Extensions;

namespace ImportExportComponent.HelperServices
{
    public class RuleProvider
    {
        private readonly EntitySetProvider _entityProvider;
        private readonly Dictionary<string, List<FieldValidationRule>> _rules;
        public RuleProvider(EntitySetProvider entityProvider)
        {
            _entityProvider = entityProvider;

            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Resources", "rules.json");
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("rules.json faylı tapılmadı.", jsonPath);

            var json = File.ReadAllText(jsonPath);

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            _rules = JsonSerializer.Deserialize<Dictionary<string, List<FieldValidationRule>>>(json, options)!;
        }

        public Task<List<FieldValidationRule>> GetRulesAsync(string tableName)
        {
            if (_rules.TryGetValue(tableName, out var rules))
                return Task.FromResult(rules);

            return Task.FromResult(new List<FieldValidationRule>());
        }

        public async Task<bool> ForeignValueExistsAsync(string tableName, string columnName, string value)
        {
            var clrType = _entityProvider.GetEntityType(tableName)
                      ?? throw new Exception($"'{tableName}' cədvəli tapılmadı.");

            var queryable = _entityProvider.GetQueryable(clrType);

            var param = Expression.Parameter(clrType, "x");
            var property = Expression.PropertyOrField(param, columnName);
            var constant = Expression.Constant(Convert.ChangeType(value, property.Type));
            var body = Expression.Equal(property, constant);
            var lambda = Expression.Lambda(body, param);

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .MakeGenericMethod(clrType);

            var result = (bool)method.Invoke(null, [queryable, lambda])!;
            return await Task.FromResult(result);
        }
    }
}