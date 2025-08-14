using Auth.Business.Dtos;
using Auth.Business.Helpers;
using Auth.Business.Models;
using Auth.DAL.Contexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.HelperServices;
using SharedLibrary.ServiceRegistration;
using FilterComponent.Services;

namespace Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;

            builder.Services.AddControllers();

            builder.Services.AddDbContext<AuthDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("MSSQL"));
            });

            builder.Services.ConfigureAuthServices();

            builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

            builder.Services.AddAuthMassTransit(configuration["RabbitMQ:Username"]!, configuration["RabbitMQ:Password"]!, configuration["RabbitMQ:Hostname"]!, configuration["RabbitMQ:Port"]!);

            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            builder.RegisterFilterComponent("MongoDb");
            builder.Services.AddScoped<CurrentUser>();
            builder.Services.AddSwagger();

            builder.Services.AddSharedServices(configuration);

            builder.Services.AddSingleton(new PermissionServiceConfig
            {
#if DEBUG
                BaseUrl = "http://localhost:5003/api"
#else
            BaseUrl = "https://template-api.microsol.az/permission"
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