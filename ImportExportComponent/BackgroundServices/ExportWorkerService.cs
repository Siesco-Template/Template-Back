using ImportExportComponent.Dtos;
using ImportExportComponent.HelperServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using TableComponent.Entities;

namespace ImportExportComponent.BackgroundServices
{
    public class ExportWorkerService(
        Channel<(List<ExportColumnDto> columns, TableQueryRequest tableRequest, string userId)> queue,
        IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private readonly Channel<(List<ExportColumnDto> columns, TableQueryRequest tableRequest, string userId)> _queue = queue;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // queue-dan sırayla data oxunur
            await foreach (var (columns, tableRequest, userId) in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    //her bir work ayrı db-e baglana biler
                    using var scope = _scopeFactory.CreateScope();
                    var exportQueryHelper = scope.ServiceProvider.GetRequiredService<ExportQueryHelper>();

                    await exportQueryHelper.ExportData(tableRequest, columns, userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Export error: {ex}");
                }
            }
        }
    }
}