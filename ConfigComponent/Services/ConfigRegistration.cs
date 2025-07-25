using ConfigComponent.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace ConfigComponent.Services
{
    public static class ConfigRegistration
    {
        public static void RegisterConfigComponent(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<ConfigService>();
        }
    }
}