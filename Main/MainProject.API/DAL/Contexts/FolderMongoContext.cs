using Folder.Abstractions;
using Folder.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SharedLibrary.Settings;

namespace MainProject.API.DAL.Contexts
{
    public class FolderMongoContext : IFolderMongoContext
    {
        private readonly IMongoDatabase _database;

        public FolderMongoContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<FolderEntity> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<FolderEntity>(collectionName);
        }
    }
}