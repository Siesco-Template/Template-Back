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
            return finalQuery.ApplyPagination(tableRequest.Pagination, totalCount);
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
            return finalQuery.ApplyInfiniteScroll(catalogRequest.Page);
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
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);
            var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = tableRequest.Filters, TableId = !tableRequest.InitialFilter ? tableRequest.TableId : null });
            filteredQuery = filteredQuery.ApplySorting(entityType, tableRequest.SortBy, tableRequest.SortDirection);

            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, tableRequest.Columns);
        }

        private async Task<IQueryable<dynamic>> GenerateQuery(CatalogQueryRequest catalogRequest)
        {
            var entityType = _setProvider.GetEntityType(catalogRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);

            var stringProps = entityType.GetProperties().Where(p => p.PropertyType == typeof(string)).Select(p => p.Name);

            var searchQuery = IQueryableExtensions.GenerateCatalogQuery(catalogRequest.Columns, catalogRequest.Filter);

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(searchQuery);
            }

            return DynamicProjectionHelper.GetSelectedColumns(query, catalogRequest.Columns);
        }

        private async Task<IQueryable<dynamic>> GenerateQuery(CatalogFilterRequest catalogFilterRequest)
        {
            var entityType = _setProvider.GetEntityType(catalogFilterRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);

            var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = [new() { Column = catalogFilterRequest.CatalogColumn, Value = catalogFilterRequest.CatalogValue, FilterKey = FilterKey.Select, FilterOperation = FilterOperationType.Equal }] });

            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, catalogFilterRequest.Columns);
        }
    }
}