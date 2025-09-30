using Permission.Api;
using Permission.Api.Services;
using SharedLibrary.ServiceRegistration;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwagger();

        builder.RegisterPermissionComponent("MongoDb");

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
        builder.Services.AddSharedServices(configuration);

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
        });

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