using ImportExportComponent.Dtos;
using ImportExportComponent.Services;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace ImportExportComponent.HelperServices
{
    public class ExportQueryHelper(ExportService exportService, GetQueryHelper getQueryHelper)
    {
        private readonly ExportService _exportService = exportService;
        private readonly GetQueryHelper _getQueryHelper = getQueryHelper;
        public async Task ExportData(TableQueryRequest tableRequest, List<ExportColumnDto> columns, string userId)
        {
            var query = await _getQueryHelper.GetQuery(tableRequest, true);
            await _exportService.ExportAndNotifyAsync(query, columns, userId);
        }
    }
}