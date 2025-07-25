using Auth.Business.Dtos;
using Auth.Business.Helpers;
using Auth.Business.Models;
using Auth.DAL.Contexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.ServiceRegistration;

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

            builder.Services.AddAuth(configuration["Jwt:Issuer"]!, configuration["Jwt:Audience"]!, configuration["Jwt:SigningKey"]!);

            builder.Services.AddAuthorization();
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
                });
            }

            app.UseCustomExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}