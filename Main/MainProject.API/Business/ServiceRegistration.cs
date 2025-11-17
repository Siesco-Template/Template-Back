using ImportExportComponent.Configuration;
using ImportExportComponent.Converters;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using ImportExportComponent.Validation.Strategies;
using MainProject.API.Business.Services;
using MainProject.API.DAL.Contexts;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.HelperServices;
using System.Text;
using TableComponent.Extensions;

namespace MainProject.API.Business
{
    public static class ServiceRegistration
    {
        public static void AddMainServices(this IServiceCollection services, IConfiguration? configuration = null)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<CurrentUser>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<DbContext, MainDbContext>();
            services.AddScoped<EntitySetProvider>();
            services.AddScoped<ReportService>();
            services.AddScoped<GetQueryHelper>();

            // ImportExportComponent configuration
            if (configuration != null)
            {
                services.Configure<ImportExportOptions>(configuration.GetSection(ImportExportOptions.SectionName));
            }
            services.AddSingleton<ImportExportOptions>(provider =>
            {
                if (configuration != null)
                {
                    var options = new ImportExportOptions();
                    configuration.GetSection(ImportExportOptions.SectionName).Bind(options);
                    return options;
                }
                return new ImportExportOptions();
            });

            // ImportExportComponent services
            services.AddScoped<TypeConverterFactory>();
            services.AddScoped<ValidationStrategyFactory>();
            services.AddScoped<ExcelHeaderDetector>();
            services.AddScoped<MetadataRuleGenerator>(provider =>
            {
                var entityProvider = provider.GetRequiredService<EntitySetProvider>();
                var dbContext = provider.GetRequiredService<DbContext>();
                var options = provider.GetRequiredService<ImportExportOptions>();
                return new MetadataRuleGenerator(entityProvider, dbContext, options);
            });
            services.AddScoped<RuleProvider>();
            services.AddScoped<ExportQueryHelper>();
            services.AddScoped<ExportService>();
            services.AddScoped<ImportService>();
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

                // Configure SignalR to use JWT token from query string or header
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;
                        
                        // If the request is for SignalR hub
                        if (path.StartsWithSegments("/hubs"))
                        {
                            // Try to get token from query string (for WebSocket connections)
                            var accessToken = context.Request.Query["access_token"];
                            
                            // If token is not in query string, it should be in Authorization header
                            // which is handled automatically by JWT Bearer middleware
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization();
        }
    }
}