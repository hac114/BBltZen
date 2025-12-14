using DTO;
using Microsoft.EntityFrameworkCore;
//using Keycloak.AuthServices.Authentication; // ✅ COMMENTATO
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Repository;
using System.Threading.RateLimiting;
using BBltZen.BackgroundServices;
using BBltZen.Infrastructure;
using BBltZen;

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

            // ✅ RATE LIMITING CONFIGURATION
            builder.Services.AddRateLimiter(options =>
            {
                // ✅ COMMENTATO: GlobalLimiter (applica a TUTTI gli endpoint automaticamente)
                // options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                //     context => RateLimitPartition.GetFixedWindowLimiter(
                //         partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                //         factory: partition => new FixedWindowRateLimiterOptions
                //         {
                //             AutoReplenishment = true,
                //             PermitLimit = 100, // ✅ 100 richieste al minuto
                //             QueueLimit = 0,    // ✅ Nessuna coda
                //             Window = TimeSpan.FromMinutes(1) // ✅ Finestra di 1 minuto
                //         }));

                // ✅ POLITICA "Default" PER SVILUPPO (limiti alti)
                options.AddPolicy("Default", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 1000, // ✅ Limite alto per sviluppo
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // ✅ POLITICA PER ENDPOINT SENSIBILI: 30 richieste/minuto
                options.AddPolicy("SensitiveEndpoints", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ??
                                     context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 30, // ✅ 30 richieste al minuto
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // ✅ POLITICA PER SWAGGER: NO LIMITI
                options.AddPolicy("SwaggerExempt", context =>
                    RateLimitPartition.GetNoLimiter("swagger"));

                // ✅ POLITICA PRODUZIONE (100 richieste/minuto)
                options.AddPolicy("Production", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100, // ✅ 100 richieste al minuto
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // ✅ RISPOSTA PERSONALIZZATA QUANDO SI SUPERA IL LIMITE
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync(
                        "⚠️ Troppe richieste. Riprova tra qualche minuto.",
                        cancellationToken: token);

                    // ✅ LOGGING (solo in sviluppo)
                    if (builder.Environment.IsDevelopment())
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning($"Rate limit exceeded for IP: {context.HttpContext.Connection.RemoteIpAddress}");
                    }
                };

                // ✅ DISABILITA RATE LIMITING PER CERTI ENDPOINT (opzionale)
                options.AddPolicy("NoLimiting", context =>
                    RateLimitPartition.GetNoLimiter("unlimited"));
            });

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

            // ✅ APPLICA RATE LIMITING (DOPO builder.Build() e PRIMA di UseAuthorization)
            app.UseRateLimiter();

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