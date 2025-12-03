using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ImportExportComponent.Settings;

namespace ImportExportComponent.HelperServices
{
    public class MongoDbService(IOptions<MongoDbSettings> _dbSettings)
    {
        private readonly MongoDbSettings _mongoDbSettings = _dbSettings.Value;
        public IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_mongoDbSettings.ConnectionString);
            return client.GetDatabase(_mongoDbSettings.DatabaseName);
        }
    }
}

