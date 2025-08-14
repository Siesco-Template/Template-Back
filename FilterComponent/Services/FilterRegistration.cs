using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.HelperServices;
using SharedLibrary.Settings;

namespace FilterComponent.Services
{
    public static class FilterRegistration
    {
        /// <summary>
        /// "MongoDB": {
        ///"ConnectionString": "mongodb://Siesco:S!%40sc0.%40z@65.108.38.170:27017/",
        ///"DatabaseName": "FilterTestDb"
        ///},  burada mutleq ConnectionString ve DatabaseName yazilmalidir
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="MongoDbName"></param>
        public static void RegisterFilterComponent(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<FilterService>();
        }
    }
}