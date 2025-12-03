using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Settings;

namespace ImportExportComponent.Services
{
    public static class ImportExportRegistration
    {
        /// <summary>
        /// Registers ImportExportComponent services including DownloadItemService
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        /// <param name="mongoDbName">MongoDB configuration section name (e.g., "MongoDb")</param>
        public static void RegisterImportExportComponent(this WebApplicationBuilder builder, string mongoDbName)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(mongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<CurrentUser>();
            builder.Services.AddScoped<DownloadItemService>();
        }
    }
}

