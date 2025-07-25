using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Settings;

namespace SharedLibrary.HelperServices
{
    public static class DownloadItemsRegistration
    {
        public static void RegisterDownloadItems(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
        }
    }
}