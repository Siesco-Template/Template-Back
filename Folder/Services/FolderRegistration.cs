using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.HelperServices;
using SharedLibrary.Settings;

namespace Folder.Services
{
    public static class FolderRegistration
    {
        public static void RegisterFolderComponent(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<IFolderService, FolderService>();
            builder.Services.AddScoped<IFolderFileService, FolderFileService>();
        }
    }
}