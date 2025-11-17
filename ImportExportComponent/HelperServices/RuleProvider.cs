using ImportExportComponent.Configuration;
using ImportExportComponent.Converters;
using ImportExportComponent.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using TableComponent.Extensions;

namespace ImportExportComponent.HelperServices
{
    public class RuleProvider
    {
        private static readonly MethodInfo AnyAsyncMethodDefinition =
            typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .Single(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) &&
                             m.GetParameters().Length == 3);

        private readonly EntitySetProvider _entityProvider;
        private readonly Dictionary<string, List<FieldValidationRule>> _rules;
        private readonly ConcurrentDictionary<(Type EntityType, string ColumnName), Type> _propertyTypeCache = new();
        private readonly MetadataRuleGenerator _metadataRuleGenerator;
        private readonly TypeConverterFactory _typeConverterFactory;
        private readonly ImportExportOptions _options;

        public RuleProvider(
            EntitySetProvider entityProvider,
            ImportExportOptions options,
            MetadataRuleGenerator? metadataRuleGenerator = null,
            TypeConverterFactory? typeConverterFactory = null)
        {
            _entityProvider = entityProvider;
            _options = options;
            // MetadataRuleGenerator will be injected via DI, so we don't create it here
            // If not provided, it means DI registration is missing
            _metadataRuleGenerator = metadataRuleGenerator 
                ?? throw new InvalidOperationException("MetadataRuleGenerator must be registered in DI container.");
            _typeConverterFactory = typeConverterFactory ?? new TypeConverterFactory();

            // Load rules from JSON file if it exists
            var jsonPath = ResolveRulesFilePath();
            if (File.Exists(jsonPath))
            {
                var json = File.ReadAllText(jsonPath);
                JsonSerializerOptions jsonOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var jsonRules = JsonSerializer.Deserialize<Dictionary<string, List<FieldValidationRule>>>(json, jsonOptions);
                if (jsonRules != null)
                {
                    _rules = jsonRules;
                }
            }

            _rules ??= new Dictionary<string, List<FieldValidationRule>>(StringComparer.OrdinalIgnoreCase);
        }

        private string ResolveRulesFilePath()
        {
            if (Path.IsPathRooted(_options.RulesFilePath))
            {
                return _options.RulesFilePath;
            }

            return Path.Combine(AppContext.BaseDirectory, _options.RulesFilePath);
        }

        public Task<List<FieldValidationRule>> GetRulesAsync(string tableName, CancellationToken cancellationToken = default)
        {
            // First check if rules exist in loaded dictionary
            if (_rules.TryGetValue(tableName, out var rules) && rules.Count > 0)
            {
                return Task.FromResult(rules);
            }

            // If auto-generation is enabled, generate rules from metadata
            if (_options.EnableAutoRuleGeneration)
            {
                try
                {
                    var generatedRules = _metadataRuleGenerator.GenerateRules(tableName);
                    if (generatedRules.Count > 0)
                    {
                        // Cache generated rules
                        _rules[tableName] = generatedRules;
                        return Task.FromResult(generatedRules);
                    }
                }
                catch (Exception)
                {
                    // If generation fails, return empty list
                }
            }

            return Task.FromResult(new List<FieldValidationRule>());
        }

        public async Task<bool> ForeignValueExistsAsync(string tableName, string columnName, string value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var clrType = _entityProvider.GetEntityType(tableName)
                ?? throw new InvalidOperationException($"'{tableName}' cədvəli tapılmadı.");

            var typedQueryable = _entityProvider.GetQueryable(clrType);
            var parameter = Expression.Parameter(clrType, "entity");
            var property = Expression.PropertyOrField(parameter, columnName);
            var targetType = _propertyTypeCache.GetOrAdd((clrType, columnName), _ => Nullable.GetUnderlyingType(property.Type) ?? property.Type);
            
            // Dönüştürme işlemini try-catch ile koru
            object? convertedValue;
            try
            {
                convertedValue = ConvertToPropertyType(value, targetType);
            }
            catch (FormatException)
            {
                // Geçersiz format durumunda false dön (değer veritabanında yok demektir)
                return false;
            }
            catch (Exception)
            {
                // Diğer dönüştürme hatalarında da false dön
                return false;
            }

            var constant = Expression.Constant(convertedValue, property.Type);
            var body = Expression.Equal(property, constant);
            var predicateType = typeof(Func<,>).MakeGenericType(clrType, typeof(bool));
            var lambda = Expression.Lambda(predicateType, body, parameter);

            var anyAsync = AnyAsyncMethodDefinition.MakeGenericMethod(clrType);
            var task = (Task<bool>)anyAsync.Invoke(null, new object[] { typedQueryable, lambda, cancellationToken })!;
            return await task.ConfigureAwait(false);
        }

        private object? ConvertToPropertyType(string value, Type targetType)
        {
            return _typeConverterFactory.Convert(value, targetType);
        }
    }
}
