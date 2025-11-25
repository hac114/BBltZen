using BBltZen.Services;
using BBltZen.Services.Background;
using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
//using Keycloak.AuthServices.Authentication; // ✅ COMMENTATO
using Microsoft.OpenApi.Models;
using Repository;

namespace BBltZen
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Per statistiche carrello scheduling
            builder.Services.AddHostedService<CacheBackgroundService>();

            // ✅ SWAGGER SEMPLICE (senza autenticazione Keycloak)
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BBltZen API", Version = "v1" });

                // ✅ COMMENTATO: Configurazione Bearer token per Keycloak
                // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                // {
                //     Description = "Inserisci il token JWT di Keycloak: Bearer {token}",
                //     Name = "Authorization",
                //     In = ParameterLocation.Header,
                //     Type = SecuritySchemeType.ApiKey,
                //     Scheme = "Bearer"
                // });
                //
                // c.AddSecurityRequirement(new OpenApiSecurityRequirement
                // {
                //     {
                //         new OpenApiSecurityScheme
                //         {
                //             Reference = new OpenApiReference
                //             {
                //                 Type = ReferenceType.SecurityScheme,
                //                 Id = "Bearer"
                //             }
                //         },
                //         Array.Empty<string>()
                //     }
                // });
            });

            // ✅ COMMENTATO: KEYCLOAK AUTHENTICATION
            // builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);

            // 🔍 DEBUG: Verifica la configurazione
            Console.WriteLine("=== DEBUG CONFIGURAZIONE ===");
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"ConnectionString: {(string.IsNullOrEmpty(connectionString) ? "NULL o VUOTA" : "TROVATA")}");
            Console.WriteLine("=== FINE DEBUG ===");

            // Database Context
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Services.AddDbContext<BubbleTeaContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                Console.WriteLine("⚠️  Usando database InMemory");
                builder.Services.AddDbContext<BubbleTeaContext>(options =>
                {
                    options.UseInMemoryDatabase("BubbleTeaInMemory");
                    // ✅ DISABILITA I WARNING SULLE TRANSAZIONI
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });
            }

            // Registra tutti i repository
            builder.Services.AddServiceDb();
            builder.Services.Configure<StripeSettingsDTO>(
                builder.Configuration.GetSection("StripeSettings"));

            // ✅ REGISTRA IL DATABASE SEEDER
            builder.Services.AddScoped<DatabaseSeeder>();

            // ✅ COMMENTATO: AUTHORIZATION
            // builder.Services.AddAuthorization();

            var app = builder.Build();

            // ✅ SEEDING AUTOMATICO IN DEVELOPMENT
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync(); // Esegue il seeding all'avvio
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            // ✅ COMMENTATO: MIDDLEWARE DI AUTENTICAZIONE
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers();
            await app.RunAsync();
        }
    }
}