using Database;
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
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);

            Console.WriteLine($"📋 Menu completo: {result.Count} elementi");
        }

        [Fact]
        public async Task GetPrimoPianoAsync_Integration_ReturnsFeaturedItems()
        {
            // Act
            var result = await _repository.GetPrimoPianoAsync(6);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<VwMenuDinamicoDTO>>(result);

            Console.WriteLine($"⭐ Primo piano: {result.Count} elementi");

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

            Console.WriteLine($"🟢 Bevande disponibili: {result.Count}");
        }

        [Fact]
        public async Task GetCategorieDisponibiliAsync_Integration_ReturnsCategories()
        {
            // Act
            var result = await _repository.GetCategorieDisponibiliAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<string>>(result);

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

            Console.WriteLine($"🔍 Risultati ricerca 'tea': {resultTea.Count}");
            Console.WriteLine($"🔍 Risultati ricerca 'latte': {resultLatte.Count}");
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

            Console.WriteLine($"📊 Statistiche Menu Dinamico:");
            Console.WriteLine($"   • Menu completo: {menu.Count}");
            Console.WriteLine($"   • Primo piano: {primoPiano.Count}");
            Console.WriteLine($"   • Disponibili: {disponibili.Count}");
            Console.WriteLine($"   • Categorie: {categorie.Count}");
            Console.WriteLine($"   • Count disponibili: {count}");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}