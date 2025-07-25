using MongoDB.Driver;
using SharedLibrary.Entities;

namespace SharedLibrary.HelperServices
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

        public async Task AddItem(DownloadItem item)
        {
            await _downloadItems.InsertOneAsync(item);
        }

        public async Task<List<DownloadItem>> GetUserDownloads()
        {
            return await _downloadItems
                .Find(x => x.UserId == _currentUser.UserId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}