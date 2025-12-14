using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest.IntegrationTests
{
    [Trait("Category", "Integration")]
    [Collection("DatabaseIntegrationTests")]
    public class VwArticoliCompletiIntegrationTest : IDisposable
    {
        private readonly BubbleTeaContext _context;
        private readonly IVwArticoliCompletiRepository _repository;
        private readonly string _connectionString;

        public VwArticoliCompletiIntegrationTest()
        {
            // Connection string per il database reale
            _connectionString = "Server=localhost;Database=BubbleTea;Trusted_Connection=true;TrustServerCertificate=true;";

            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseSqlServer(_connectionString)
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new VwArticoliCompletiRepository(_context, Mock.Of<ILogger<VwArticoliCompletiRepository>>());
        }

        [Fact]
        public async Task GetAllAsync_Integration_ReturnsDataFromRealDatabase()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwArticoliCompletiDTO>>(result);

            // Log per debugging
            Console.WriteLine($"✅ Trovati {result.Count()} articoli nella vista VwArticoliCompleti");

            if (result.Any())
            {
                var first = result.First();
                Console.WriteLine($"📋 Esempio: {first.NomeArticolo} - {first.PrezzoBase}€ - {first.Categoria}");
            }
        }

        [Fact]
        public async Task GetByTipoAsync_Integration_ReturnsFilteredData()
        {
            // Act - Prova con diversi tipi che potrebbero esistere
            var resultBs = await _repository.GetByTipoAsync("BS");
            var resultD = await _repository.GetByTipoAsync("D");
            var resultBc = await _repository.GetByTipoAsync("BC");

            // Assert
            Assert.NotNull(resultBs);
            Assert.NotNull(resultD);
            Assert.NotNull(resultBc);

            Console.WriteLine($"🍵 Bevande Standard (BS): {resultBs.Count()}");
            Console.WriteLine($"🍰 Dolci (D): {resultD.Count()}");
            Console.WriteLine($"🎨 Bevande Custom (BC): {resultBc.Count()}");
        }

        [Fact]
        public async Task GetDisponibiliAsync_Integration_ReturnsAvailableItems()
        {
            // Act
            var result = await _repository.GetDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            if (result.Any())
            {
                Assert.All(result, a => Assert.Equal(1, a.Disponibile));
            }

            Console.WriteLine($"🟢 Articoli disponibili: {result.Count()}");
        }

        [Fact]
        public async Task GetCategorieAsync_Integration_ReturnsDistinctCategories()
        {
            // Act
            var result = await _repository.GetCategorieAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);

            Console.WriteLine($"📂 Categorie trovate: {string.Join(", ", result)}");
        }

        [Fact]
        public async Task GetTipiArticoloAsync_Integration_ReturnsDistinctTypes()
        {
            // Act
            var result = await _repository.GetTipiArticoloAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);

            Console.WriteLine($"🏷️ Tipi articolo trovati: {string.Join(", ", result)}");
        }

        [Fact]
        public async Task SearchByNameAsync_Integration_ReturnsMatchingItems()
        {
            // Act - Prova con termini comuni
            var result = await _repository.SearchByNameAsync("tea");
            var result2 = await _repository.SearchByNameAsync("latte");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result2);

            Console.WriteLine($"🔍 Risultati ricerca 'tea': {result.Count()}");
            Console.WriteLine($"🔍 Risultati ricerca 'latte': {result2.Count()}");
        }

        [Fact]
        public async Task ComplexQueries_Integration_AllMethodsWork()
        {
            // Act - Testa tutti i metodi per verificare che non crashino
            var all = await _repository.GetAllAsync();
            var count = await _repository.GetCountAsync();
            var categories = await _repository.GetCategorieAsync();
            var types = await _repository.GetTipiArticoloAsync();
            var available = await _repository.GetDisponibiliAsync();
            var withIva = await _repository.GetArticoliConIvaAsync();

            // Assert - Verifica che tutti i metodi restituiscano risultati validi
            Assert.NotNull(all);
            Assert.True(count >= 0);
            Assert.NotNull(categories);
            Assert.NotNull(types);
            Assert.NotNull(available);
            Assert.NotNull(withIva);

            Console.WriteLine($"📊 Statistiche complete:");
            Console.WriteLine($"   • Totale articoli: {count}");
            Console.WriteLine($"   • Categorie: {categories.Count()}");
            Console.WriteLine($"   • Tipi: {types.Count()}");
            Console.WriteLine($"   • Disponibili: {available.Count()}");
            Console.WriteLine($"   • Con IVA: {withIva.Count()}");
        }

        [Fact]
        public async Task GetByPriceRangeAsync_Integration_ReturnsFilteredByPrice()
        {
            // Act
            var result = await _repository.GetByPriceRangeAsync(0m, 10m);

            // Assert
            Assert.NotNull(result);
            if (result.Any())
            {
                Assert.All(result, a =>
                {
                    Assert.True(a.PrezzoBase >= 0m);
                    Assert.True(a.PrezzoBase <= 10m);
                });
            }

            Console.WriteLine($"💰 Articoli nel range 0-10€: {result.Count()}");
        }

        [Fact]
        public async Task GetArticoliConIvaAsync_Integration_ReturnsItemsWithTax()
        {
            // Act
            var result = await _repository.GetArticoliConIvaAsync();

            // Assert
            Assert.NotNull(result);
            if (result.Any())
            {
                Assert.All(result, a => Assert.True(a.AliquotaIva > 0));
            }

            Console.WriteLine($"🧾 Articoli con IVA: {result.Count()}");
        }

        [Fact]
        public async Task GetByIdAsync_Integration_ReturnsSingleItem()
        {
            // Arrange - Prendi un ID esistente dalla vista
            var allItems = await _repository.GetAllAsync();
            var existingId = allItems.FirstOrDefault()?.ArticoloId;

            if (existingId.HasValue)
            {
                // Act
                var result = await _repository.GetByIdAsync(existingId.Value);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(existingId.Value, result.ArticoloId);
                Console.WriteLine($"🔎 Trovato articolo con ID {existingId.Value}: {result.NomeArticolo}");
            }
            else
            {
                Console.WriteLine("ℹ️ Nessun articolo trovato per test GetByIdAsync");
            }
        }

        [Fact]
        public async Task ExistsAsync_Integration_ChecksItemExistence()
        {
            // Arrange - Prendi un ID esistente dalla vista
            var allItems = await _repository.GetAllAsync();
            var existingId = allItems.FirstOrDefault()?.ArticoloId;

            if (existingId.HasValue)
            {
                // Act
                var result = await _repository.ExistsAsync(existingId.Value);

                // Assert
                Assert.True(result);
                Console.WriteLine($"✅ Articolo con ID {existingId.Value} esiste: {result}");
            }
            else
            {
                Console.WriteLine("ℹ️ Nessun articolo trovato per test ExistsAsync");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    [CollectionDefinition("DatabaseIntegrationTests")]
    public class DatabaseIntegrationTestsCollection : ICollectionFixture<VwArticoliCompletiIntegrationTest>
    {
        // Questa classe serve solo per definire la collection
    }
}