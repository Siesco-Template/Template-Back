using Auth.Business.Dtos;
using Auth.Business.Helpers;
using Auth.Business.Models;
using Auth.Business.Services;
using Auth.Core.Dtos.FolderFiles;
using Auth.DAL.Contexts;
using ConfigComponent.Services;
using FilterComponent.Services;
using FilterComponent.Utilities;
using FluentValidation;
using Folder.Abstractions;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Dtos;
using ImportExportComponent.HelperServices;
using ImportExportComponent.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.ServiceRegistration;
using System.Reflection;
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

            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
            });

            builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("FolderDb"));

            builder.Services.AddSingleton<IFolderMongoContext, FolderMongoContext>();

            // user modeli folder strukturuna uygunlasdirmaq
            builder.Services.AddScoped<IFolderService<UserFile>>(sp =>
                new FolderService<UserFile>(sp.GetRequiredService<IFolderMongoContext>(), RootFolders.Users));


            builder.Services.AddScoped<IFolderFileService<UserFile>>(sp =>
            {
                var context = sp.GetRequiredService<IFolderMongoContext>();
                var folderService = sp.GetRequiredService<IFolderService<UserFile>>();
                return new FolderFileService<UserFile>(context, folderService, RootFolders.Users);
            });

            // Bu s?tir Guid problemi üçün laz?md?r
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // ---------------- Folder inteqrasiya ----------------end

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserFileService, UserFileService>();

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