using MongoDB.Driver;
using Permission.Api;
using Permission.Api.DAL;
using SharedLibrary;
using SharedLibrary.ServiceRegistration;
using SharedLibrary.Settings;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwagger();

        builder.Services.AddPermissionServices();

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton(new PermissionServiceConfig
        {
#if DEBUG
            BaseUrl = "http://localhost:5003/api"
#else
            BaseUrl = "https://template-api.microsol.az/permission"
#endif
        });

        var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
        builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoDbSettings.ConnectionString));
        builder.Services.AddSingleton(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoDbSettings.DatabaseName);
        });
        builder.Services.AddSingleton<MongoDbContext>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder =>
                {
                    builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                });
        });

        builder.Services.AddMassTransitPermission(configuration["RabbitMQ:Username"]!, configuration["RabbitMQ:Password"]!, configuration["RabbitMQ:Hostname"]!, configuration["RabbitMQ:Port"]!);

        builder.Services.AddAuth(configuration["Jwt:Issuer"]!, configuration["Jwt:Audience"]!, configuration["Jwt:SigningKey"]!);
        // shared-deki servisler
        builder.Services.AddSharedServices(configuration);
        var app = builder.Build();

        //if (app.Environment.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
        });
        //}

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCustomExceptionHandler();

        app.UseCors("AllowSpecificOrigin");
        app.UseAuthorization();

        app.MapControllers();

        ////attributlardaki page ve action-lari scan edir
        //using (var scope = app.Services.CreateScope())
        //{
        //    var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        //    // user-lerin permissi-ona elave olunmasi 
        //    var userPermissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        //    await userPermissionService.SyncUsersFromMainServiceAsync();
        //    //page ve action-lerin scan olunmasi
        //    var permissionScanner = scope.ServiceProvider.GetRequiredService<PermissionScannerForServices>();
        //    await permissionScanner.ScanAndSendPagesAndActionsAsync(Assembly.GetExecutingAssembly());
        //}

        app.Run();
    }
}