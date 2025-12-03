using ConfigComponent.Services;
using FilterComponent.HelperServices;
using FilterComponent.Services;
using Folder.Services;
using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Hubs;
using MainProject.API.Business;
using MainProject.API.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.ServiceRegistration;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace MainProject.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddDbContext<MainDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL"));
            });

            builder.Services.AddMainServices(builder.Configuration);
            builder.Services.AddMainMassTransit(builder.Configuration["RabbitMQ:Username"]!, builder.Configuration["RabbitMQ:Password"]!, builder.Configuration["RabbitMQ:Hostname"]!, builder.Configuration["RabbitMQ:Port"]!);
            builder.Services.AddAuth(builder.Configuration["Jwt:Issuer"]!, builder.Configuration["Jwt:Audience"]!, builder.Configuration["Jwt:SigningKey"]!);
            builder.Services.AddSharedServices(builder.Configuration);

            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
            });

            builder.RegisterFilterComponent("MongoDb");
            builder.Services.AddScoped<FilterComponent.HelperServices.CurrentUser>();
            builder.RegisterConfigComponent("MongoDb");
            builder.RegisterDownloadItems("MongoDb");
            builder.RegisterFolderComponent("MongoDb");

            builder.Services.AddSingleton(provider =>
                Channel.CreateUnbounded<ExportQueueItem>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false
                }));

            builder.Services.AddHostedService<ExportWorkerService>();

            builder.Services.AddSwagger();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Configure SignalR with CORS
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.AddSingleton(new PermissionServiceConfig
            {
#if DEBUG
                BaseUrl = "http://localhost:5003/api"
#else
                BaseUrl = "https://template-api.microsol.az/permission"
#endif
            });

            var app = builder.Build();

            app.UseCustomExceptionHandler();

            app.UseSwagger();

            app.UseSwaggerUI(x => x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true"));

            // Skip HTTPS redirection in development for HTTP SignalR connections
            // app.UseHttpsRedirection();

            // Enable static files serving from wwwroot
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            // SignalR hub - CORS için authorization'ı hub seviyesinde değil endpoint seviyesinde kontrol ediyoruz
            app.MapHub<ExportHub>("/hubs/export");

            app.Run();
        }
    }
}