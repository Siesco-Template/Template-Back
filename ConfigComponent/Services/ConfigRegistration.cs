using ConfigComponent.HelperServices;
using ConfigComponent.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigComponent.Services
{
    public static class ConfigRegistration
    {
        public static void RegisterConfigComponent(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<ConfigService>();
        }
    }
}