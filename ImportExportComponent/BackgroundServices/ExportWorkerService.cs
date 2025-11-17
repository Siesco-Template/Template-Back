using ImportExportComponent.HelperServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace ImportExportComponent.BackgroundServices
{
    public class ExportWorkerService(
        Channel<ExportQueueItem> queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ExportWorkerService> logger) : BackgroundService
    {
        private readonly Channel<ExportQueueItem> _queue = queue;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<ExportWorkerService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var item in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                try
                {
                    var exportQueryHelper = scope.ServiceProvider.GetRequiredService<ExportQueryHelper>();
                    await exportQueryHelper.ExportDataAsync(item, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Export job failed for table {TableId} and user {UserId}.",
                        item.TableRequest.TableId,
                        item.UserId);
                }
            }
        }
    }
}