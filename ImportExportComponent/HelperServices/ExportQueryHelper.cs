using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Dtos;
using ImportExportComponent.Services;
using TableComponent.Extensions;
using System;
using System.Threading;

namespace ImportExportComponent.HelperServices
{
    public class ExportQueryHelper(ExportService exportService, GetQueryHelper getQueryHelper)
    {
        private readonly ExportService _exportService = exportService;
        private readonly GetQueryHelper _getQueryHelper = getQueryHelper;
        public async Task ExportDataAsync(ExportQueueItem queueItem, CancellationToken cancellationToken = default)
        {
            if (queueItem.TableRequest is null)
            {
                throw new ArgumentException("Table request cannot be null for export operations.", nameof(queueItem));
            }

            // Background service'te HTTP context olmadığı için userId'yi parametre olarak geçiyoruz
            var query = await _getQueryHelper.GetQuery(queueItem.TableRequest, true, queueItem.UserId);
            await _exportService.ExportAndNotifyAsync(
                query,
                queueItem.Columns,
                queueItem.TableRequest.TableId,
                queueItem.UserId,
                cancellationToken);
        }
    }
}