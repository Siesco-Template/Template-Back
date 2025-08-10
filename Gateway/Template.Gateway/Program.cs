using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Template.Gateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var environment = builder.Environment.EnvironmentName;
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                                  .AddJsonFile($"ocelot.{environment}.json", optional: true, reloadOnChange: true);

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials();
                });
            });

            builder.Services.AddOcelot();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            await app.UseOcelot();

            app.Run();
        }
    }
}