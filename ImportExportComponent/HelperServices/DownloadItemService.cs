using MongoDB.Driver;
using ImportExportComponent.Entities;
using System.Threading;

namespace ImportExportComponent.HelperServices
{
    public class DownloadItemService
    {
        private readonly IMongoCollection<DownloadItem> _downloadItems;
        private readonly CurrentUser _currentUser;
        public DownloadItemService(MongoDbService _mongoDbService, CurrentUser currentUser)
        {
            var database = _mongoDbService.GetDatabase();
            _downloadItems = database.GetCollection<DownloadItem>("DownloadItems");
            _currentUser = currentUser;
        }

        public async Task AddItem(DownloadItem item, CancellationToken cancellationToken = default)
        {
            await _downloadItems.InsertOneAsync(item, cancellationToken: cancellationToken);
        }

        public async Task<List<DownloadItem>> GetUserDownloads(CancellationToken cancellationToken = default)
        {
            return await _downloadItems
                .Find(x => x.UserId == _currentUser.UserId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

