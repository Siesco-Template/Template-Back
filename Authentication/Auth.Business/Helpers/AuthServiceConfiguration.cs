using Auth.Business.Helpers.HelperServices;
using Auth.Business.Helpers.HelperServices.Email;
using Auth.Business.Helpers.HelperServices.Token;
using Auth.Business.Services;
using Auth.Business.Utilies.Templates;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TableComponent.Extensions;



namespace Auth.Business.Helpers
{
    public static class AuthServiceConfiguration
    {
        public static void ConfigureAuthServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<TokenService>();
            services.AddScoped<EmailTemplate>();
            services.AddScoped<EmailService>();
            services.AddScoped<CurrentUser>();
            services.AddScoped<AuthService>();
            services.AddScoped<GetQueryHelper>();
        }

        public static void AddAuthMassTransit(this IServiceCollection services, string username, string password, string hostname, string port)
        {
            password = Uri.EscapeDataString(password);
            string cString = $"amqp://{username}:{password}@{hostname}:{port}/";

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(cString);
                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}