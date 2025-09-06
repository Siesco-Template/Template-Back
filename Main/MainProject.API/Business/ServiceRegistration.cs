using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using MainProject.API.Business.Services;
using MainProject.API.DAL.Contexts;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.HelperServices;
using System.Text;
using TableComponent.Extensions;

namespace MainProject.API.Business
{
    public static class ServiceRegistration
    {
        public static void AddMainServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<CurrentUser>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserFileService, UserFileService>();
            services.AddScoped<DbContext, MainDbContext>();
            services.AddScoped<EntitySetProvider>();
            services.AddScoped<ReportService>();
            services.AddScoped<GetQueryHelper>();
            services.AddScoped<RuleProvider>();
            services.AddScoped<ExportQueryHelper>();
            services.AddScoped<ExportService>();
            services.AddScoped<ImportService>();
            services.AddScoped<CatalogService>();
        }

        public static IServiceCollection AddMainMassTransit(this IServiceCollection services, string username, string password, string hostname, string port)
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

                x.AddInMemoryInboxOutbox();
            });

            return services;
        }

        public static void AddMainAuth(this IServiceCollection services, string issuer, string audience, string key)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    LifetimeValidator = (_, expires, token, _) => token != null ? DateTime.Now < expires : false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });
            services.AddAuthorization();
        }
    }
}