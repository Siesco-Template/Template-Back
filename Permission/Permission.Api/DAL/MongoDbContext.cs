using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Permission.Api.Entities;
using SharedLibrary.Settings;

namespace Permission.Api.DAL
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            if (options?.Value?.ConnectionString == null || options.Value.DatabaseName == null)
            {
                throw new ArgumentNullException("MongoDbSettings are not properly configured.");
            }

            var client = new MongoClient(options.Value.ConnectionString);
            _database = client.GetDatabase(options.Value.DatabaseName);
        }

        public IMongoCollection<UserPermission> UserPermissions =>
            _database.GetCollection<UserPermission>("user_permissions");
        public IMongoCollection<Page> PagePermissions =>
          _database.GetCollection<Page>("page_permissions");
    }
}