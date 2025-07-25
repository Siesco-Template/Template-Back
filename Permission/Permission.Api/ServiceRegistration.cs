using MassTransit;
using Permission.Api.Consumers;
using Permission.Api.Services;
using SharedLibrary.HelperServices;
using SharedLibrary.Requests;

namespace Permission.Api
{
    public static class ServiceRegistration
    {
        public static void AddPermissionServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<CurrentUser>();
            services.AddScoped<IPermissionService, PermissionService>();
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
