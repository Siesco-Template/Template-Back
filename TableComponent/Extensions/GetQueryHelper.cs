using FilterComponent.Dtos;
using FilterComponent.Services;
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

        private async Task<IQueryable<dynamic>> GenerateQuery(TableQueryRequest tableRequest)
        {
            var entityType = _setProvider.GetEntityType(tableRequest.FilterDto.TableId) ?? throw new Exception("Cədvəl tapılmadı.");
            var query = (IQueryable<dynamic>)_setProvider.GetQueryable(entityType);
            var filteredQuery = await _filterService.ApplyFilter(query, tableRequest.FilterDto);
            filteredQuery = filteredQuery.ApplySorting(entityType, tableRequest.SortBy, tableRequest.SortDirection);
            return DynamicProjectionHelper.GetSelectedColumns(filteredQuery, tableRequest.Columns);
        }
    }
}