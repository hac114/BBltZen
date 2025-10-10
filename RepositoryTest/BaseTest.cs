using Database;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service;

namespace RepositoryTest
{
    public class BaseTest : IDisposable
    {
        protected readonly BubbleTeaContext _context;
        protected readonly IIngredienteRepository _ingredienteRepository;

        public BaseTest()
        {
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

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}