using MassTransit;
using Permission.Api.Consumers;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.Settings;

namespace Permission.Api.Services
{
    public static class PermissionRegistration
    {
        public static void RegisterPermissionComponent(this WebApplicationBuilder builder, string MongoDbName)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbName));
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<CurrentUser>();
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton(new PermissionServiceConfig
            {
#if DEBUG
                BaseUrl = "http://localhost:5003/api"
#else
            BaseUrl = "https://template-api.microsol.az/permission"
#endif
            });
        }

        public static IServiceCollection AddMassTransitPermission(this IServiceCollection services, string username, string password, string hostname, string port)
        {
            password = Uri.EscapeDataString(password);
            string cString = $"amqp://{username}:{password}@{hostname}:{port}/";

            services.AddMassTransit(x =>
            {
                x.AddConsumer<PermissionRegisteredConsumer>();
                x.AddConsumer<UserRegisteredPermissionConsumer>();
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((con, cfg) =>
                {
                    cfg.Host(cString);
                    cfg.ConfigureEndpoints(con);
                });

            });
            return services;
        }
    }
}