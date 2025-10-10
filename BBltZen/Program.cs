
using Database;
using Microsoft.EntityFrameworkCore;
using Repository;

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
            builder.Services.AddSwaggerGen();

            // 🔍 DEBUG: Verifica la configurazione
            Console.WriteLine("=== DEBUG CONFIGURAZIONE ===");

            // Database Context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"ConnectionString: {(string.IsNullOrEmpty(connectionString) ? "NULL o VUOTA" : "TROVATA")}");

            // Mostra tutte le chiavi di configurazione disponibili
            Console.WriteLine("Tutte le chiavi di configurazione:");
            foreach (var key in builder.Configuration.AsEnumerable())
            {
                if (key.Key.Contains("Connection", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  {key.Key} = {key.Value}");
                }
            }
            Console.WriteLine("=== FINE DEBUG ===");

            // Database Context - SOLO se la connection string esiste
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Services.AddDbContext<BubbleTeaContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                // Fallback: usa una stringa di connessione di default per development
                Console.WriteLine("⚠️  Usando connection string di fallback per InMemory");
                builder.Services.AddDbContext<BubbleTeaContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Registra TUTTI i repository in un colpo solo
            builder.Services.AddServiceDb(); // Metodo da StartUpConfigurator

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}