using FilterComponent.Dtos;
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
        public async Task<PaginationResult> GetQuery(TableQueryRequest tableRequest)
        {
            var finalQuery = await GenerateQuery(tableRequest);
            return finalQuery.ApplyPagination(tableRequest.Pagination);
        }

        public async Task<IQueryable<dynamic>> GetQuery(TableQueryRequest tableRequest, bool isExcel)
        {
            return await GenerateQuery(tableRequest);
        }

        public async Task<PaginationResult> GetCatalog(CatalogQueryRequest catalogRequest)
        {
            var finalQuery = await GenerateCatalogQuery(catalogRequest);
            return finalQuery.ApplyInfiniteScroll(catalogRequest.Page);
        }

        private async Task<IQueryable<dynamic>> GenerateQuery(TableQueryRequest tableRequest)
        {
            var entityType = _setProvider.GetEntityType(tableRequest.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);
            var filteredQuery = await _filterService.ApplyFilter(query, new FilterDto { Filters = tableRequest.Filters, TableId = !tableRequest.InitialFilter ? tableRequest.TableId : null });
            filteredQuery = filteredQuery.ApplySorting(entityType, tableRequest.SortBy, tableRequest.SortDirection);

            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, tableRequest.Columns);
        }

        private async Task<IQueryable<dynamic>> GenerateCatalogQuery(CatalogQueryRequest catalogRequest)
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
    }
}