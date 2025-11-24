using Microsoft.EntityFrameworkCore.Metadata;
using QueryGenerator.Core;
using System.Reflection;

namespace TableComponent.Extensions
{
    public static class DynamicProjectionHelper
    {
        public static IQueryable<dynamic> GetSelectedColumns(this IQueryable<dynamic> query, string columns, Type entityType, IEntityType? entityMetadata)
        {
            if (string.IsNullOrWhiteSpace(columns))
                throw new ArgumentException("Columns string cannot be null or empty.", nameof(columns));

            try
            {
                var columnList = columns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                if (!columnList.Any())
                    throw new ArgumentException("At least one column must be specified.", nameof(columns));

                var projectionParts = new List<string>();
                var navigationPropertiesMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                // First pass: Identify navigation properties and their requested properties
                foreach (var column in columnList)
                {
                    var columnParts = column.Split('.');
                    var columnName = columnParts[0].Trim();
                    
                    if (entityMetadata != null)
                    {
                        var navigation = entityMetadata.FindNavigation(columnName);
                        if (navigation != null && !navigation.IsCollection)
                        {
                            // This is a navigation property
                            if (!navigationPropertiesMap.ContainsKey(columnName))
                            {
                                navigationPropertiesMap[columnName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            }
                            
                            // If there's a nested property access (e.g., "ReportType.Description" or "AddendumHeader.AdministrativeClassification.Code")
                            if (columnParts.Length > 1)
                            {
                                var nestedProp = string.Join(".", columnParts.Skip(1)).Trim();
                                if (!string.IsNullOrWhiteSpace(nestedProp))
                                {
                                    navigationPropertiesMap[columnName].Add(nestedProp);
                                }
                            }
                        }
                    }
                }

                // Second pass: Build projection - only include requested properties
                foreach (var column in columnList)
                {
                    var columnParts = column.Split('.');
                    var columnName = columnParts[0].Trim();
                    
                    // Check if this is a navigation property
                    if (entityMetadata != null && navigationPropertiesMap.ContainsKey(columnName))
                    {
                        var navigation = entityMetadata.FindNavigation(columnName);
                        if (navigation != null && !navigation.IsCollection)
                        {
                            // This is a navigation property - project only requested properties
                            var requestedProps = navigationPropertiesMap[columnName];
                            
                            if (requestedProps.Any())
                            {
                                // Project only the requested properties of navigation property
                                // Handle multi-level nested properties (e.g., "AddendumHeader.AdministrativeClassification.Code")
                                foreach (var prop in requestedProps)
                                {
                                    // Create alias with underscores for all levels (e.g., AddendumHeader_AdministrativeClassification_Code)
                                    var alias = $"{columnName}_{prop.Replace(".", "_")}";
                                    projectionParts.Add($"{columnName}.{prop} as {alias}");
                                }
                            }
                            else
                            {
                                // If no specific properties requested, use the navigation property name
                                projectionParts.Add(columnName);
                            }
                            continue;
                        }
                    }

                    // Regular property (not a navigation property)
                    if (columnParts.Length == 1)
                    {
                        projectionParts.Add(column);
                    }
                }

                if (!projectionParts.Any())
                    throw new ArgumentException("No valid columns found for projection.", nameof(columns));

                var projectionString = string.Join(", ", projectionParts);
                return (IQueryable<dynamic>)query.Select($"new({projectionString})");
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                // Fallback: if projection fails, try simple projection without navigation handling
                try
                {
                    return (IQueryable<dynamic>)query.Select($"new({columns})");
                }
                catch
                {
                    throw new InvalidOperationException($"Failed to create projection for columns: {columns}. Original error: {ex.Message}", ex);
                }
            }
        }

        private static List<string> GetScalarProperties(Type entityType, Microsoft.EntityFrameworkCore.Metadata.IModel model)
        {
            var scalarProps = new List<string>();
            
            if (entityType == null || model == null)
                return scalarProps;

            try
            {
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var prop in properties)
                {
                    // Skip navigation properties
                    if (IsNavigationProperty(prop, model))
                        continue;

                    // Include scalar properties
                    scalarProps.Add(prop.Name);
                }
            }
            catch
            {
                // If error occurs, return empty list
            }

            return scalarProps;
        }

        private static bool IsNavigationProperty(PropertyInfo property, Microsoft.EntityFrameworkCore.Metadata.IModel model)
        {
            if (property == null || model == null)
                return false;

            try
            {
                // Check if property type is a class (not string, not primitive, not value types)
                if (property.PropertyType == typeof(string) || 
                    property.PropertyType.IsPrimitive || 
                    property.PropertyType.IsValueType ||
                    property.PropertyType == typeof(byte[]) ||
                    property.PropertyType == typeof(Guid) ||
                    property.PropertyType == typeof(DateTime) ||
                    property.PropertyType == typeof(DateOnly) ||
                    property.PropertyType == typeof(TimeOnly) ||
                    property.PropertyType == typeof(DateTimeOffset) ||
                    Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    return false;
                }

                // Check if it's a collection (ICollection, IEnumerable, List, etc.)
                if (property.PropertyType.IsGenericType)
                {
                    var genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(ICollection<>) ||
                        genericTypeDefinition == typeof(IEnumerable<>) ||
                        genericTypeDefinition == typeof(List<>) ||
                        genericTypeDefinition == typeof(IList<>))
                    {
                        return true;
                    }
                }

                // Check if it's a navigation property by checking if EF Core recognizes it
                if (property.DeclaringType != null)
                {
                    var entityType = model.FindEntityType(property.DeclaringType);
                    if (entityType != null)
                    {
                        var navigation = entityType.FindNavigation(property.Name);
                        if (navigation != null)
                        {
                            return true;
                        }
                    }
                }

                // If it's a class type and not a known value type, assume it might be a navigation property
                return property.PropertyType.IsClass;
            }
            catch
            {
                return false;
            }
        }
    }
}