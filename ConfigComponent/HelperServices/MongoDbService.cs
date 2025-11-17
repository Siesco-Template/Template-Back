using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ConfigComponent.Settings;

namespace ConfigComponent.HelperServices
{
    public class MongoDbService
    {
        private readonly MongoDbSettings _dbSettings;

        public MongoDbService(IOptions<MongoDbSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public IMongoDatabase GetDatabase()
        {
            var client = new MongoClient(_dbSettings.ConnectionString);
            return client.GetDatabase(_dbSettings.DatabaseName);
        }
    }
}


