using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.Exceptions.Common;
using SharedLibrary.HelperServices.Permission;
using System.Reflection;
using System.Text;

namespace SharedLibrary.ServiceRegistration
{
    public static class MainServiceRegistration
    {
        public static void AddAuth(this IServiceCollection services, string issuer, string audience, string key)
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
            });
            services.AddAuthorization();
        }

        public static void AddSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<PageAndActionScannerForServices>();
            services.AddHttpClient<ICheckPermissionService, CheckPermissionService>(client =>
            {
                var permissionServiceUrl = configuration["PermissionService:BaseUrl"];
                client.BaseAddress = new Uri(permissionServiceUrl);
            });
            services.AddFluentValidation(opt =>
            {
                opt.RegisterValidatorsFromAssemblyContaining<UserDto>();
            });
        }

        public static void UseCustomExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(handlerApp =>
            {
                handlerApp.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();
                    var error = feature?.Error;

                    // unwrap TargetInvocationException
                    if (error is TargetInvocationException tie && tie.InnerException != null)
                    {
                        error = tie.InnerException;
                    }

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    if (error is IBaseException ex)
                    {
                        context.Response.StatusCode = ex.StatusCode;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            ex.StatusCode,
                            Message = ex.ErrorMessage
                        });
                    }
                    else
                    {
                        await context.Response.WriteAsJsonAsync(new
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                            Message = "Gözlənilməz xəta baş verdi."
                        });
                    }
                });
            });
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }
    }
}