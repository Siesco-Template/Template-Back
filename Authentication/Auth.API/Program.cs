using Auth.Business.Dtos;
using Auth.Business.Helpers;
using Auth.Business.Models;
using Auth.Business.Services;
using Auth.DAL.Contexts;
using ConfigComponent.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FilterComponent.Services;
using FluentValidation;
using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Dtos;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.ServiceRegistration;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using TableComponent.Entities;
using TableComponent.Extensions;

namespace Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            builder.Services.AddControllers().AddNewtonsoftJson().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.RegisterConfigComponent("MongoDb");

            builder.Services.AddDbContext<AuthDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("MSSQL"));
            });

            builder.Services.ConfigureAuthServices();

            builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

            builder.Services.AddAuthMassTransit(configuration["RabbitMQ:Username"]!, configuration["RabbitMQ:Password"]!, configuration["RabbitMQ:Hostname"]!, configuration["RabbitMQ:Port"]!);

            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            builder.Services.AddScoped<DbContext, AuthDbContext>();
            builder.Services.AddScoped<EntitySetProvider>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<ConfigService>();
            builder.Services.AddScoped<FilterService>();
            builder.Services.AddScoped<CurrentUser>();
            builder.Services.AddScoped<GetQueryHelper>();
            builder.Services.AddScoped<RuleProvider>();
            builder.Services.AddScoped<DownloadItemService>();
            builder.Services.AddScoped<ExportQueryHelper>();
            builder.Services.AddScoped<ExportService>();
            builder.Services.AddScoped<ImportService>();

            builder.Services.AddSingleton(provider =>
            {
                return Channel.CreateUnbounded<(List<ExportColumnDto>, TableQueryRequest, string)>();
            });
            builder.Services.AddHostedService<ExportWorkerService>();

            builder.Services.AddSwagger();

            builder.Services.AddSharedServices(configuration);

            builder.Services.AddSingleton(new PermissionServiceConfig
            {
#if DEBUG
                BaseUrl = "http://localhost:5003/api"
#else
            BaseUrl = ""
#endif
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                    });
            });

            builder.Services.AddAuth(configuration["Jwt:Issuer"]!, configuration["Jwt:Audience"]!, configuration["Jwt:SigningKey"]!);

            builder.Services.AddAuthorization();
            var app = builder.Build();

            app.UseCustomExceptionHandler();

            app.UseSwagger();
            app.UseSwaggerUI(x => x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}