using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Repository.Service;

namespace RepositoryTest
{
    public class BaseTest : IDisposable
    {
        protected readonly BubbleTeaContext _context;
        protected readonly IIngredienteRepository _ingredienteRepository;
        protected readonly IConfiguration _configuration;

        public BaseTest()
        {
            // ✅ CARICA CONFIGURAZIONE CON ENTRAMBI I FILE
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false)  // Template con placeholder
                .AddJsonFile("appsettings.test.local.json", optional: true)  // Chiavi reali (se esiste)
                .AddEnvironmentVariables()  // Variabili d'ambiente
                .Build();

            // ✅ CREA OPZIONI PER INMEMORY
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            // ✅ CREA IL CONTEXT
            _context = new BubbleTeaContext(options);

            // ✅ CREA IL REPOSITORY DIRETTAMENTE
            _ingredienteRepository = new IngredienteRepository(_context);

            // ✅ INIZIALIZZA IL DATABASE
            InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            try
            {
                // ✅ USA SOLO EnsureCreated
                _context.Database.EnsureCreated();

                // ✅ INIZIALIZZA CATEGORIE SOLO SE NECESSARIO
                if (!_context.CategoriaIngrediente.Any())
                {
                    _context.CategoriaIngrediente.AddRange(
                        new CategoriaIngrediente { CategoriaId = 1, Categoria = "tea" },
                        new CategoriaIngrediente { CategoriaId = 2, Categoria = "latte" },
                        new CategoriaIngrediente { CategoriaId = 3, Categoria = "dolcificante" },
                        new CategoriaIngrediente { CategoriaId = 4, Categoria = "topping" },
                        new CategoriaIngrediente { CategoriaId = 5, Categoria = "aroma" },
                        new CategoriaIngrediente { CategoriaId = 6, Categoria = "speciale" },
                        new CategoriaIngrediente { CategoriaId = 7, Categoria = "ghiaccio" },
                        new CategoriaIngrediente { CategoriaId = 8, Categoria = "caffe" }
                    );
                    _context.SaveChanges();
                }

                // ✅ INIZIALIZZA STATI PAGAMENTO PER STRIPE
                if (!_context.StatoPagamento.Any())
                {
                    _context.StatoPagamento.AddRange(
                        new StatoPagamento { StatoPagamentoId = 1, StatoPagamento1 = "In_Attesa" },
                        new StatoPagamento { StatoPagamentoId = 2, StatoPagamento1 = "Pagato" },
                        new StatoPagamento { StatoPagamentoId = 3, StatoPagamento1 = "Fallito" },
                        new StatoPagamento { StatoPagamentoId = 4, StatoPagamento1 = "Rimborsato" }
                    );
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Errore inizializzazione database test: {ex.Message}", ex);
            }
        }

        // ✅ METODO PER PULIRE TABELLE SPECIFICHE
        protected async Task CleanTableAsync<T>() where T : class
        {
            var entities = _context.Set<T>().ToList();
            if (entities.Any())
            {
                _context.Set<T>().RemoveRange(entities);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ METODO PER OTTENERE CONFIGURAZIONE STRIPE
        protected StripeSettingsDTO GetStripeSettings()  // ✅ CAMBIA IL TIPO DI RITORNO
        {
            var stripeSection = _configuration.GetSection("Stripe");

            return new StripeSettingsDTO  // ✅ USA IL DTO
            {
                SecretKey = stripeSection["SecretKey"] ?? "REPLACE_WITH_STRIPE_SECRET_KEY",
                PublishableKey = stripeSection["PublishableKey"] ?? "REPLACE_WITH_STRIPE_PUBLISHABLE_KEY",
                WebhookSecret = stripeSection["WebhookSecret"] ?? "REPLACE_WITH_STRIPE_WEBHOOK_SECRET"
            };
        }

        // ✅ METODO PER VERIFICARE SE LE CHIAVI STRIPE SONO REALI
        protected bool HasRealStripeKeys()
        {
            var settings = GetStripeSettings();
            return !settings.SecretKey.Contains("REPLACE_WITH_STRIPE") &&
                   !settings.SecretKey.Contains("sk_test_mock");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}