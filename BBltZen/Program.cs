using Database;
using Microsoft.EntityFrameworkCore;
using Repository;
using Keycloak.AuthServices.Authentication;
using Microsoft.OpenApi.Models;

namespace BBltZen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ✅ SWAGGER CONFIGURATO PER KEYCLOAK
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BBltZen API", Version = "v1" });

                // Configurazione Bearer token per Keycloak
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Inserisci il token JWT di Keycloak: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ✅ KEYCLOAK AUTHENTICATION
            builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);

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
                    options.UseInMemoryDatabase("BubbleTeaInMemory"));
            }

            // Registra tutti i repository
            builder.Services.AddServiceDb();

            // ✅ AUTHORIZATION
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // ✅ MIDDLEWARE IN ORDINE CORRETTO
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}