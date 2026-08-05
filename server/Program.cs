using DotNetEnv;
using Pbl3.Extensions;

namespace Pbl3
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Load Environment Variables
            Env.TraversePath().Load();

            var builder = WebApplication.CreateBuilder(args);

            // 2. Configure Controllers & Web Services
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new Pbl3.Utils.LocalDateTimeJsonConverter());
            });

            // 3. Register Modular Services via Extension Methods
            builder.Services.AddCorsPolicy();
            builder.Services.AddPostgresDatabase(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddPaymentServices();
            builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);
            builder.Services.AddSwaggerDocumentation();

            // 4. Build Application Pipeline
            var app = builder.Build();

            // if (app.Environment.IsDevelopment())
            // {
            //     app.UseSwagger();
            //     app.UseSwaggerUI();
            // }
            app.UseSwagger();
            app.UseSwaggerUI();
            
            app.UseHttpsRedirection();
            app.UseCors("FrontendDev");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // 5. Database Execution Commands (--migrate, --seed, initialize)
            if (args.Contains("--migrate"))
            {
                await app.MigrateDatabaseAsync();
                return;
            }

            await app.InitializeDatabaseAsync();

            if (args.Contains("--seed"))
            {
                await app.SeedDatabaseAsync();
                return;
            }

            app.Run();
        }
    }
}
