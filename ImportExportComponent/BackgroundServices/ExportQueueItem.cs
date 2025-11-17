using ImportExportComponent.Dtos;
using TableComponent.Entities;

namespace ImportExportComponent.BackgroundServices
{
    public readonly record struct ExportQueueItem(
        IReadOnlyList<ExportColumnDto> Columns,
        TableQueryRequest TableRequest,
        string UserId);
}