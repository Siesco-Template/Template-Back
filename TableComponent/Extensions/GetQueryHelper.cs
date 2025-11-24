using FilterComponent.Dtos;
using FilterComponent.Enums;
using FilterComponent.Extensions;
using FilterComponent.Services;
using QueryGenerator.Core;
using QueryGenerator.Entities;
using QueryGenerator.Extensions;
using TableComponent.Entities;

namespace TableComponent.Extensions
{
    public class GetQueryHelper(EntitySetProvider setProvider, FilterService filterService)
    {
        private readonly EntitySetProvider _setProvider = setProvider;
        private readonly FilterService _filterService = filterService;

        /// <summary>
        /// Table datasının gətirilməsi Pagination ilə
        /// </summary>
        /// <param name="tableRequest"></param>
        /// <returns></returns>
        public async Task<PaginationResult> GetQuery(TableQueryRequest tableRequest)
        {
            var finalQuery = await GenerateQuery(tableRequest);
            var totalCount = finalQuery.Count();
            var result = finalQuery.ApplyPagination(tableRequest.Pagination, totalCount);
            
            // Transform flat navigation properties to nested objects
            result.Items = TransformNavigationProperties(result.Items, tableRequest.Columns);
            
            return result;
        }

        /// <summary>
        /// View as catalog-dan seçilmiş dəyərə əsasən filterlənmiş datanın gətirilməsi
        /// </summary>
        /// <param name="catalogFilterRequest"></param>
        /// <returns></returns>
        public async Task<IQueryable<dynamic>> GetQuery(CatalogFilterRequest catalogFilterRequest)
        {
            var finalQuery = await GenerateQuery(catalogFilterRequest);
            return finalQuery;
        }

        /// <summary>
        /// Excel export üçün pagination olmadan bütün datanın gətirilməsi
        /// </summary>
        /// <param name="tableRequest"></param>
        /// <param name="isExcel"></param>
        /// <returns></returns>
        public async Task<IQueryable<dynamic>> GetQuery(TableQueryRequest tableRequest, bool isExcel)
        {
            return await GenerateQuery(tableRequest);
        }

        /// <summary>
        /// Filterdə Catalog üçün axtarış nəticələrinin gətirilməsi Infinite Scroll ilə
        /// </summary>
        /// <param name="catalogRequest"></param>
        /// <returns></returns>
        public async Task<PaginationResult> GetQuery(CatalogQueryRequest catalogRequest)
        {
            var finalQuery = await GenerateQuery(catalogRequest);
            var result = finalQuery.ApplyInfiniteScroll(catalogRequest.Page);
            
            // Transform flat navigation properties to nested objects
            result.Items = TransformNavigationProperties(result.Items, catalogRequest.Columns);
            
            return result;
        }

        /// <summary>
        /// Dinamik sorğuların yaradılması, filterin tətbiqi, sort və seçilmiş sütunların tətbiqi
        /// </summary>
        /// <param name="tableRequest"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<IQueryable<dynamic>> GenerateQuery(TableQueryRequest tableRequest)
        {
            var entityType = _setProvider.GetEntityType(tableRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var entityMetadata = _setProvider.GetEntityTypeMetadata(entityType);
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);
            
            // Include navigation properties before filtering
            query = IncludeNavigationProperties(query, entityType, entityMetadata, tableRequest.Columns);
            
            var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = tableRequest.Filters, TableId = !tableRequest.InitialFilter ? tableRequest.TableId : null });
            filteredQuery = filteredQuery.ApplySorting(entityType, tableRequest.SortBy, tableRequest.SortDirection);

            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, tableRequest.Columns, entityType, entityMetadata);
        }

        private Task<IQueryable<dynamic>> GenerateQuery(CatalogQueryRequest catalogRequest)
        {
            var entityType = _setProvider.GetEntityType(catalogRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var entityMetadata = _setProvider.GetEntityTypeMetadata(entityType);
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);

            // Include navigation properties before filtering
            query = IncludeNavigationProperties(query, entityType, entityMetadata, catalogRequest.Columns);

            var stringProps = entityType.GetProperties().Where(p => p.PropertyType == typeof(string)).Select(p => p.Name);

            var searchQuery = IQueryableExtensions.GenerateCatalogQuery(catalogRequest.Columns, catalogRequest.Filter);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(searchQuery);
            }

            return Task.FromResult(DynamicProjectionHelper.GetSelectedColumns(query, catalogRequest.Columns, entityType, entityMetadata));
        }

        private async Task<IQueryable<dynamic>> GenerateQuery(CatalogFilterRequest catalogFilterRequest)
        {
            var entityType = _setProvider.GetEntityType(catalogFilterRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var entityMetadata = _setProvider.GetEntityTypeMetadata(entityType);
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);

            // Include navigation properties before filtering
            query = IncludeNavigationProperties(query, entityType, entityMetadata, catalogFilterRequest.Columns);

            var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = [new() { Column = catalogFilterRequest.CatalogColumn, Value = catalogFilterRequest.CatalogValue, FilterKey = FilterKey.Select, FilterOperation = FilterOperationType.Equal }] });

            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, catalogFilterRequest.Columns, entityType, entityMetadata);
        }

        /// <summary>
        /// Navigation property'leri Include edir (nested navigation property'ler dahil)
        /// </summary>
        private IQueryable<dynamic> IncludeNavigationProperties(IQueryable<dynamic> query, Type entityType, Microsoft.EntityFrameworkCore.Metadata.IEntityType? entityMetadata, string columns)
        {
            if (entityMetadata == null || string.IsNullOrWhiteSpace(columns))
                return query;

            try
            {
                var columnList = columns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                var processedNavigations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var column in columnList)
                {
                    var columnParts = column.Split('.');
                    if (columnParts.Length == 0)
                        continue;

                    var firstNav = columnParts[0].Trim();
                    
                    // Include first level navigation property
                    if (!processedNavigations.Contains(firstNav))
                    {
                        try
                        {
                            var navigation = entityMetadata.FindNavigation(firstNav);
                            if (navigation != null && !navigation.IsCollection)
                            {
                                query = (IQueryable<dynamic>)_setProvider.IncludeNavigationProperty(query, entityType, firstNav);
                                processedNavigations.Add(firstNav);
                            }
                        }
                        catch
                        {
                            // If Include fails for one navigation, continue with others
                            continue;
                        }
                    }

                    // Include nested navigation properties (e.g., "AddendumHeader.AdministrativeClassification")
                    if (columnParts.Length > 2)
                    {
                        // Build nested include path (e.g., "AddendumHeader.AdministrativeClassification")
                        var nestedIncludePath = string.Join(".", columnParts.Take(columnParts.Length - 1));
                        
                        if (!processedNavigations.Contains(nestedIncludePath))
                        {
                            try
                            {
                                // Try to include the nested navigation path
                                query = (IQueryable<dynamic>)_setProvider.IncludeNavigationProperty(query, entityType, nestedIncludePath);
                                processedNavigations.Add(nestedIncludePath);
                            }
                            catch
                            {
                                // If nested Include fails, continue with others
                                continue;
                            }
                        }
                    }
                }

                return query;
            }
            catch
            {
                // If navigation inclusion fails completely, return original query
                return query;
            }
        }

        /// <summary>
        /// Flat navigation property'leri nested object'lere çevirir
        /// </summary>
        private List<dynamic> TransformNavigationProperties(List<dynamic> items, string columns)
        {
            if (items == null || !items.Any() || string.IsNullOrWhiteSpace(columns))
                return items;

            try
            {
                // Parse columns to identify which properties should be included
                var columnList = columns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToList();

                var navigationPropertiesMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                var regularProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Identify navigation properties and regular properties
                foreach (var column in columnList)
                {
                    var columnParts = column.Split('.');
                    var columnName = columnParts[0].Trim();
                    
                    if (columnParts.Length > 1)
                    {
                        // Navigation property with nested property
                        // Handle nested properties like "ReportType.Description" or "AddendumHeader.AdministrativeClassification.Code"
                        var navProp = string.Join(".", columnParts.Skip(1)).Trim();
                        
                        if (!navigationPropertiesMap.ContainsKey(columnName))
                        {
                            navigationPropertiesMap[columnName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        // Store the full nested path (e.g., "AdministrativeClassification.Code")
                        navigationPropertiesMap[columnName].Add(navProp);
                    }
                    else
                    {
                        // Regular property
                        regularProperties.Add(columnName);
                    }
                }

                var transformedItems = new List<dynamic>();

                foreach (var item in items)
                {
                    if (item == null)
                    {
                        transformedItems.Add(item);
                        continue;
                    }

                    var itemDict = new Dictionary<string, object?>();
                    var navigationGroups = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

                    // Get all properties from dynamic object using reflection
                    var itemType = item.GetType();
                    var properties = itemType.GetProperties();

                    foreach (var prop in properties)
                    {
                        var propName = prop.Name;
                        object? propValue = null;
                        
                        try
                        {
                            propValue = prop.GetValue(item);
                        }
                        catch
                        {
                            continue;
                        }

                        // Check if this is a navigation property (contains underscore pattern: NavigationProperty_PropertyName or NavigationProperty_NestedProperty_PropertyName)
                        var underscoreIndex = propName.IndexOf('_');
                        if (underscoreIndex > 0 && underscoreIndex < propName.Length - 1)
                        {
                            var navName = propName.Substring(0, underscoreIndex);
                            var remainingPath = propName.Substring(underscoreIndex + 1);

                            // Check if this navigation property is in the requested columns
                            if (navigationPropertiesMap.ContainsKey(navName))
                            {
                                // Check if the remaining path matches any requested nested property
                                // Handle both simple (e.g., "Code") and multi-level (e.g., "AdministrativeClassification_Code") paths
                                var matchingPath = navigationPropertiesMap[navName]
                                    .FirstOrDefault(path => 
                                        path.Equals(remainingPath, StringComparison.OrdinalIgnoreCase) ||
                                        path.Replace(".", "_").Equals(remainingPath, StringComparison.OrdinalIgnoreCase));

                                if (matchingPath != null)
                                {
                                    // Build nested structure based on the path
                                    BuildNestedStructure(navigationGroups, navName, matchingPath, propValue);
                                }
                            }
                        }
                        else
                        {
                            // Regular property - only include if in requested columns
                            if (regularProperties.Contains(propName))
                            {
                                itemDict[propName] = propValue;
                            }
                        }
                    }

                    // Add navigation properties as nested objects
                    foreach (var navGroup in navigationGroups)
                    {
                        if (navGroup.Value.Any())
                        {
                            itemDict[navGroup.Key] = CreateDynamicObject(navGroup.Value);
                        }
                    }

                    transformedItems.Add(CreateDynamicObject(itemDict));
                }

                return transformedItems;
            }
            catch
            {
                // If transformation fails, return original items
                return items;
            }
        }

        /// <summary>
        /// Nested property path'ini nested object structure'ına çevirir
        /// Örnek: "AdministrativeClassification.Code" -> { AdministrativeClassification: { Code: value } }
        /// </summary>
        private static void BuildNestedStructure(Dictionary<string, Dictionary<string, object?>> navigationGroups, string navName, string path, object? value)
        {
            if (!navigationGroups.ContainsKey(navName))
            {
                navigationGroups[navName] = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }

            var pathParts = path.Split('.');
            if (pathParts.Length == 1)
            {
                // Simple property (e.g., "Code")
                navigationGroups[navName][pathParts[0]] = value;
            }
            else
            {
                // Multi-level nested property (e.g., "AdministrativeClassification.Code")
                // Build nested structure
                var currentDict = navigationGroups[navName];
                
                // Navigate/create nested dictionaries
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    var part = pathParts[i];
                    Dictionary<string, object?>? nextDict = null;
                    
                    if (currentDict.ContainsKey(part))
                    {
                        // Try to get existing dictionary
                        if (currentDict[part] is Dictionary<string, object?> existingDict)
                        {
                            nextDict = existingDict;
                        }
                        else
                        {
                            // If it's not a dictionary, replace it with a new dictionary
                            nextDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                            currentDict[part] = nextDict;
                        }
                    }
                    else
                    {
                        // Create new dictionary
                        nextDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        currentDict[part] = nextDict;
                    }
                    
                    currentDict = nextDict;
                }
                
                // Set the final value
                var finalProperty = pathParts[pathParts.Length - 1];
                currentDict[finalProperty] = value;
            }
        }

        private static dynamic CreateDynamicObject(Dictionary<string, object?> properties)
        {
            var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
            foreach (var prop in properties)
            {
                // If the value is a Dictionary, recursively convert it to ExpandoObject
                if (prop.Value is Dictionary<string, object?> nestedDict)
                {
                    expando[prop.Key] = CreateDynamicObject(nestedDict);
                }
                else
                {
                    expando[prop.Key] = prop.Value;
                }
            }
            return expando;
        }
    }
}