using Auth.Business.Dtos;
using Auth.Business.Helpers;
using Auth.Business.Models;
using Auth.DAL.Contexts;
using ConfigComponent.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.ServiceRegistration;
using System.Text.Json.Serialization;

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

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    x.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
                });
            //}

            app.UseCustomExceptionHandler();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowSpecificOrigin");
            app.MapControllers();

            app.Run();
        }
    }
}