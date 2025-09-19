using ConfigComponent.Services;
using FilterComponent.Services;
using Folder.Abstractions;
using Folder.Roots;
using Folder.Services.FolderFileServices;
using Folder.Services.FolderServices;
using ImportExportComponent.BackgroundServices;
using ImportExportComponent.Dtos;
using MainProject.API.Business;
using MainProject.API.Business.Dtos.FolderFiles;
using MainProject.API.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.ServiceRegistration;
using SharedLibrary.Settings;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using TableComponent.Entities;

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

            builder.Services.AddMainServices();
            builder.Services.AddMainMassTransit(builder.Configuration["RabbitMQ:Username"]!, builder.Configuration["RabbitMQ:Password"]!, builder.Configuration["RabbitMQ:Hostname"]!, builder.Configuration["RabbitMQ:Port"]!);
            builder.Services.AddAuth(builder.Configuration["Jwt:Issuer"]!, builder.Configuration["Jwt:Audience"]!, builder.Configuration["Jwt:SigningKey"]!);
            builder.Services.AddSharedServices(builder.Configuration);

            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                c.CustomSchemaIds(t => t.FullName!.Replace("+", "."));
            });

            builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetSection("FolderDb"));

            builder.RegisterFilterComponent("MongoDb");
            builder.RegisterConfigComponent("MongoDb");
            builder.RegisterDownloadItems("MongoDb");

            builder.Services.AddSingleton(provider =>
            {
                return Channel.CreateUnbounded<(List<ExportColumnDto>, TableQueryRequest, string)>();
            });

            builder.Services.AddHostedService<ExportWorkerService>();

            builder.Services.AddSingleton<IFolderMongoContext, FolderMongoContext>();

            // user modeli folder strukturuna uygunlasdirmaq
            builder.Services.AddScoped<IFolderService<UserFile>>(sp =>
                new FolderService<UserFile>(sp.GetRequiredService<IFolderMongoContext>(), RootFolders.Users));

            builder.Services.AddScoped<IFolderService<ReportFile>>(sp =>
                new FolderService<ReportFile>(sp.GetRequiredService<IFolderMongoContext>(), RootFolders.Reports));

            builder.Services.AddScoped<IFolderService<OrganizationFile>>(sp =>
                new FolderService<OrganizationFile>(sp.GetRequiredService<IFolderMongoContext>(), RootFolders.Organizations));

            builder.Services.AddScoped<IFolderFileService<UserFile>>(sp =>
            {
                var context = sp.GetRequiredService<IFolderMongoContext>();
                var folderService = sp.GetRequiredService<IFolderService<UserFile>>();
                return new FolderFileService<UserFile>(context, folderService, RootFolders.Users);
            });

            builder.Services.AddScoped<IFolderFileService<ReportFile>>(sp =>
            {
                var context = sp.GetRequiredService<IFolderMongoContext>();
                var folderService = sp.GetRequiredService<IFolderService<ReportFile>>();
                return new FolderFileService<ReportFile>(context, folderService, RootFolders.Reports);
            });

            builder.Services.AddScoped<IFolderFileService<OrganizationFile>>(sp =>
            {
                var context = sp.GetRequiredService<IFolderMongoContext>();
                var folderService = sp.GetRequiredService<IFolderService<OrganizationFile>>();
                return new FolderFileService<OrganizationFile>(context, folderService, RootFolders.Organizations);
            });

            // Bu s?tir Guid problemi üçün laz?md?r
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // ---------------- Folder inteqrasiya ----------------end

            builder.Services.AddSwagger();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    });
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

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}