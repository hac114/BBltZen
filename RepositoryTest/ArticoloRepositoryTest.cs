using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class ArticoloRepositoryTest : BaseTest
    {
        private readonly ArticoloRepository _repository;
        private readonly BubbleTeaContext _context;

        public ArticoloRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"ArticoloTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new ArticoloRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea Articoli
            var articoli = new List<Articolo>
            {
                new Articolo
                {
                    ArticoloId = 1,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-10),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new Articolo
                {
                    ArticoloId = 2,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now.AddDays(-5),
                    DataAggiornamento = DateTime.Now
                },
                new Articolo
                {
                    ArticoloId = 3,
                    Tipo = "BC",
                    DataCreazione = DateTime.Now.AddDays(-3),
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new Articolo
                {
                    ArticoloId = 4,
                    Tipo = "DOLCE",
                    DataCreazione = DateTime.Now.AddDays(-2),
                    DataAggiornamento = DateTime.Now
                },
                new Articolo
                {
                    ArticoloId = 5,
                    Tipo = "DOLCE",
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now
                }
            };

            _context.Articolo.AddRange(articoli);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllArticoli()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnArticolo()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal("BS", result.Tipo);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByTipoAsync_ShouldReturnFilteredArticoli()
        {
            // Act
            var result = await _repository.GetByTipoAsync("BS");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, a => Assert.Equal("BS", a.Tipo));
        }

        [Fact]
        public async Task GetByTipoAsync_WithInvalidTipo_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByTipoAsync("INVALIDO");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewArticolo()
        {
            // Arrange
            var newArticolo = new ArticoloDTO
            {
                Tipo = "BC"
            };

            // Act
            await _repository.AddAsync(newArticolo);

            // Assert
            Assert.True(newArticolo.ArticoloId > 0);
            var result = await _repository.GetByIdAsync(newArticolo.ArticoloId);
            Assert.NotNull(result);
            Assert.Equal("BC", result.Tipo);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingArticolo()
        {
            // Arrange
            var updateDto = new ArticoloDTO
            {
                ArticoloId = 1,
                Tipo = "DOLCE"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("DOLCE", result.Tipo);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveArticolo()
        {
            // Act
            await _repository.DeleteAsync(1);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTipoAsync_WithExistingTipo_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("BS");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTipoAsync_WithNonExistingTipo_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("INVALIDO");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTipoAsync_WithExcludeId_ShouldReturnCorrectResult()
        {
            // Act
            var result = await _repository.ExistsByTipoAsync("BS", 1);

            // Assert
            Assert.True(result); // Dovrebbe esistere ancora l'articolo 2 con tipo "BS"
        }
    }
}