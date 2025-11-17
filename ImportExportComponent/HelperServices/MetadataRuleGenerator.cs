using ImportExportComponent.Configuration;
using ImportExportComponent.Dtos;
using ImportExportComponent.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using TableComponent.Extensions;

namespace ImportExportComponent.HelperServices
{
    /// <summary>
    /// Generates validation rules from Entity Framework Core metadata
    /// </summary>
    public class MetadataRuleGenerator
    {
        private readonly EntitySetProvider _entityProvider;
        private readonly DbContext _dbContext;
        private readonly ImportExportOptions _options;

        public MetadataRuleGenerator(EntitySetProvider entityProvider, DbContext dbContext, ImportExportOptions options)
        {
            _entityProvider = entityProvider;
            _dbContext = dbContext;
            _options = options;
        }

        /// <summary>
        /// Generates validation rules for a table based on EF Core metadata
        /// </summary>
        public List<FieldValidationRule> GenerateRules(string tableName)
        {
            var entityType = _entityProvider.GetEntityType(tableName);
            if (entityType == null)
            {
                return new List<FieldValidationRule>();
            }

            // Get EF Core metadata
            var entityMetadata = _dbContext.Model.FindEntityType(entityType);
            if (entityMetadata == null)
            {
                return new List<FieldValidationRule>();
            }

            var rules = new List<FieldValidationRule>();

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip navigation properties and complex types
                if (property.PropertyType.IsClass && 
                    property.PropertyType != typeof(string) && 
                    property.PropertyType != typeof(byte[]) &&
                    !property.PropertyType.IsPrimitive &&
                    property.PropertyType != typeof(Guid) &&
                    property.PropertyType != typeof(DateTime) &&
                    property.PropertyType != typeof(DateOnly) &&
                    property.PropertyType != typeof(TimeOnly))
                {
                    continue;
                }

                var propertyMetadata = entityMetadata.FindProperty(property.Name);
                if (propertyMetadata == null)
                {
                    continue;
                }

                var rule = new FieldValidationRule
                {
                    Field = property.Name
                };

                // Check if required (not nullable)
                rule.IsRequired = !propertyMetadata.IsNullable && 
                                  !IsNullableType(property.PropertyType);

                // Get max length from metadata
                var maxLength = propertyMetadata.GetMaxLength();
                if (maxLength.HasValue)
                {
                    rule.MaxLength = maxLength.Value;
                }
                else if (property.PropertyType == typeof(string))
                {
                    // Check for StringLength attribute
                    var stringLengthAttr = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.StringLengthAttribute>();
                    if (stringLengthAttr != null)
                    {
                        rule.MaxLength = stringLengthAttr.MaximumLength;
                    }
                }

                // Determine data type
                rule.DataType = DetermineDataType(property.PropertyType);

                // Check if property is enum type
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (underlyingType.IsEnum)
                {
                    rule.EnumType = underlyingType.AssemblyQualifiedName;
                }

                // Check for Guid type
                if (underlyingType == typeof(Guid))
                {
                    rule.DataType = ValidationDataType.Guid;
                }

                // Check for foreign key relationships
                var foreignKey = entityMetadata.GetForeignKeys()
                    .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == property.Name));

                if (foreignKey != null)
                {
                    var principalEntity = foreignKey.PrincipalEntityType;
                    var principalProperty = foreignKey.PrincipalKey.Properties.FirstOrDefault();
                    
                    if (principalEntity != null && principalProperty != null)
                    {
                        rule.ForeignTable = principalEntity.GetTableName();
                        rule.ForeignColumn = principalProperty.Name;
                    }
                }

                rules.Add(rule);
            }

            return rules;
        }

        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static ValidationDataType? DetermineDataType(Type propertyType)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            return underlyingType switch
            {
                Type t when t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) => ValidationDataType.Int,
                Type t when t == typeof(decimal) || t == typeof(double) || t == typeof(float) => ValidationDataType.Decimal,
                Type t when t == typeof(DateTime) || t == typeof(DateOnly) => ValidationDataType.Date,
                Type t when t == typeof(string) => ValidationDataType.String,
                _ => null
            };
        }
    }
}

