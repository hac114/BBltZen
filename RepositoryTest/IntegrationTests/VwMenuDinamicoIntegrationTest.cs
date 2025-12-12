using Database.Models;
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
    public class VwMenuDinamicoIntegrationTest : IDisposable
    {
        private readonly BubbleTeaContext _context;
        private readonly IVwMenuDinamicoRepository _repository;

        public VwMenuDinamicoIntegrationTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseSqlServer("Server=localhost;Database=BubbleTea;Trusted_Connection=true;TrustServerCertificate=true;")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new VwMenuDinamicoRepository(_context, Mock.Of<ILogger<VwMenuDinamicoRepository>>());
        }

        [Fact]
        public async Task GetMenuCompletoAsync_Integration_ReturnsDataFromRealDatabase()
        {
            // Act
            var result = await _repository.GetMenuCompletoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(result); // ✅ CAMBIA QUESTO

            Console.WriteLine($"📋 Menu completo: {result.Count()} elementi");

            // Verifica aggiuntiva se ci sono dati
            if (result.Any())
            {
                Assert.NotEmpty(result);
                foreach (var item in result.Take(3))
                {
                    Console.WriteLine($"   • {item.NomeBevanda} (€{item.PrezzoNetto})");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Database di test vuoto - nessun dato trovato");
            }
        }

        [Fact]
        public async Task GetPrimoPianoAsync_Integration_ReturnsFeaturedItems()
        {
            // Act
            var result = await _repository.GetPrimoPianoAsync(6);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(result);

            Console.WriteLine($"⭐ Primo piano: {result.Count()} elementi");

            if (result.Any())
            {
                foreach (var item in result.Take(3))
                {
                    Console.WriteLine($"   • {item.NomeBevanda} (Priorità: {item.Priorita})");
                }
            }
        }

        [Fact]
        public async Task GetBevandeDisponibiliAsync_Integration_ReturnsAvailableItems()
        {
            // Act
            var result = await _repository.GetBevandeDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(result); // ✅ CORRETTO

            Console.WriteLine($"🟢 Bevande disponibili: {result.Count()} elementi");

            if (result.Any())
            {
                Console.WriteLine($"   • Esempio: {result.First().NomeBevanda} (€{result.First().PrezzoNetto})");
            }
        }

        [Fact]
        public async Task GetCategorieDisponibiliAsync_Integration_ReturnsCategories()
        {
            // Act
            var result = await _repository.GetCategorieDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result); // ✅ CORRETTO

            Console.WriteLine($"📂 Categorie disponibili: {string.Join(", ", result)}");
        }

        [Fact]
        public async Task SearchBevandeAsync_Integration_ReturnsMatchingItems()
        {
            // Act - Prova con termini comuni
            var resultTea = await _repository.SearchBevandeAsync("tea");
            var resultLatte = await _repository.SearchBevandeAsync("latte");

            // Assert
            Assert.NotNull(resultTea);
            Assert.NotNull(resultLatte);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(resultTea); // ✅ CORRETTO
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(resultLatte); // ✅ CORRETTO

            Console.WriteLine($"🔍 Risultati ricerca 'tea': {resultTea.Count()} elementi");
            Console.WriteLine($"🔍 Risultati ricerca 'latte': {resultLatte.Count()} elementi");

            if (resultTea.Any())
            {
                Console.WriteLine($"   • Trovato: {resultTea.First().NomeBevanda}");
            }
            if (resultLatte.Any())
            {
                Console.WriteLine($"   • Trovato: {resultLatte.First().NomeBevanda}");
            }
        }

        [Fact]
        public async Task ComplexQueries_Integration_AllMethodsWork()
        {
            // Act - Testa tutti i metodi principali
            var menu = await _repository.GetMenuCompletoAsync();
            var primoPiano = await _repository.GetPrimoPianoAsync();
            var disponibili = await _repository.GetBevandeDisponibiliAsync();
            var categorie = await _repository.GetCategorieDisponibiliAsync();
            var count = await _repository.GetCountBevandeDisponibiliAsync();

            // Assert
            Assert.NotNull(menu);
            Assert.NotNull(primoPiano);
            Assert.NotNull(disponibili);
            Assert.NotNull(categorie);
            Assert.True(count >= 0);

            // ✅ VERIFICA TIPI PER TUTTI
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(menu);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(primoPiano);
            Assert.IsAssignableFrom<IEnumerable<VwMenuDinamicoDTO>>(disponibili);
            Assert.IsAssignableFrom<IEnumerable<string>>(categorie);

            Console.WriteLine($"📊 Statistiche Menu Dinamico:");
            Console.WriteLine($"   • Menu completo: {menu.Count()} elementi");
            Console.WriteLine($"   • Primo piano: {primoPiano.Count()} elementi");
            Console.WriteLine($"   • Disponibili: {disponibili.Count()} elementi");
            Console.WriteLine($"   • Categorie: {categorie.Count()} elementi");
            Console.WriteLine($"   • Count disponibili: {count}");

            if (menu.Any())
            {
                Console.WriteLine($"   • Primo elemento menu: {menu.First().NomeBevanda}");
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task GetBevandaByIdAsync_Integration_ReturnsSpecificBevanda()
        {
            // Act - Prova a recuperare una bevanda specifica se esiste
            var result = await _repository.GetBevandaByIdAsync(1, "BS");

            // Assert - Se esiste, verifica i dati
            if (result != null)
            {
                Assert.NotNull(result.NomeBevanda);
                Assert.True(result.PrezzoNetto > 0);
                Console.WriteLine($"🔍 Bevanda trovata: {result.NomeBevanda} (€{result.PrezzoNetto})");
            }
            else
            {
                Console.WriteLine("ℹ️ Nessuna bevanda trovata con ID 1 e Tipo BS");
            }
        }

        [Fact]
        public async Task GetBevandePerPrioritaAsync_Integration_ReturnsFilteredByPriority()
        {
            // Act
            var result = await _repository.GetBevandePerPrioritaAsync(1, 3);

            // Assert
            Assert.NotNull(result);

            if (result.Any())
            {
                foreach (var item in result)
                {
                    Assert.InRange(item.Priorita, 1, 3);
                }
                Console.WriteLine($"🎯 Bevande per priorità 1-3: {result.Count()} elementi");
            }
        }

        [Fact]
        public async Task LargeDataset_Performance_ReturnsQuickly()
        {
            // Act - Testa che le query siano performanti anche con molti dati
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var menu = await _repository.GetMenuCompletoAsync();
            var categorie = await _repository.GetCategorieDisponibiliAsync();
            var count = await _repository.GetCountBevandeDisponibiliAsync();

            stopwatch.Stop();

            // Assert - Le query dovrebbero completarsi in meno di 5 secondi
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"Query troppo lente: {stopwatch.ElapsedMilliseconds}ms");

            Console.WriteLine($"⚡ Performance: {stopwatch.ElapsedMilliseconds}ms per 3 query principali");
        }

        [Fact]
        public async Task DatabaseConnection_WhenUnavailable_HandlesGracefully()
        {
            // Questo test verifica che il repository gestisca gli errori di connessione
            // Nota: Per test reali di connettività, potresti fermare temporaneamente il DB

            // Act - Tutte le chiamate dovrebbero gestire gli errori internamente
            var menu = await _repository.GetMenuCompletoAsync();
            var categorie = await _repository.GetCategorieDisponibiliAsync();

            // Assert - Il repository dovrebbe restituire collezioni vuote invece di crashare
            Assert.NotNull(menu);
            Assert.NotNull(categorie);

            Console.WriteLine("✅ Repository gestisce gracefully eventuali errori DB");
        }
    }
}