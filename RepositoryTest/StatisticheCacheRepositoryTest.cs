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
    public class StatisticheCacheRepositoryTest : BaseTest
    {
        private readonly StatisticheCacheRepository _repository;
        private readonly BubbleTeaContext _context;

        public StatisticheCacheRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"StatisticheCacheTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new StatisticheCacheRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var statisticheCache = new List<StatisticheCache>
            {
                new StatisticheCache
                {
                    Id = 1,
                    TipoStatistica = "VenditeGiornaliere",
                    Periodo = "2024-01-15",
                    Metriche = "{\"totaleVendite\": 1250.50, \"numeroOrdini\": 45}",
                    DataAggiornamento = DateTime.Now.AddHours(-1)
                },
                new StatisticheCache
                {
                    Id = 2,
                    TipoStatistica = "VenditeGiornaliere",
                    Periodo = "2024-01-16",
                    Metriche = "{\"totaleVendite\": 980.75, \"numeroOrdini\": 32}",
                    DataAggiornamento = DateTime.Now.AddMinutes(-30)
                },
                new StatisticheCache
                {
                    Id = 3,
                    TipoStatistica = "ProdottiPopolari",
                    Periodo = "2024-01",
                    Metriche = "{\"prodotti\": [{\"nome\": \"Bubble Tea Classico\", \"vendite\": 120}, {\"nome\": \"Tè Verde\", \"vendite\": 85}]}",
                    DataAggiornamento = DateTime.Now.AddDays(-1)
                },
                new StatisticheCache
                {
                    Id = 4,
                    TipoStatistica = "ClientiAttivi",
                    Periodo = "2024-01",
                    Metriche = "{\"clientiAttivi\": 15, \"nuoviClienti\": 8}",
                    DataAggiornamento = DateTime.Now.AddHours(-2)
                }
            };

            _context.StatisticheCache.AddRange(statisticheCache);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStatisticheCache()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnStatisticheCache()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("VenditeGiornaliere", result.TipoStatistica);
            Assert.Equal("2024-01-15", result.Periodo);
            Assert.Contains("totaleVendite", result.Metriche);
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
        public async Task GetByTipoAsync_ShouldReturnFilteredStatisticheCache()
        {
            // Act
            var result = await _repository.GetByTipoAsync("VenditeGiornaliere");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, s => Assert.Equal("VenditeGiornaliere", s.TipoStatistica));
        }

        [Fact]
        public async Task GetByTipoAndPeriodoAsync_ShouldReturnSpecificCache()
        {
            // Act
            var result = await _repository.GetByTipoAndPeriodoAsync("VenditeGiornaliere", "2024-01-15");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("VenditeGiornaliere", result.TipoStatistica);
            Assert.Equal("2024-01-15", result.Periodo);
        }

        [Fact]
        public async Task GetByTipoAndPeriodoAsync_WithInvalidParams_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByTipoAndPeriodoAsync("Inesistente", "2024-01-01");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewStatisticheCache()
        {
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TempiPreparazione",
                Periodo = "2024-01-17",
                Metriche = "{\"tempoMedio\": 8.5, \"tempoMassimo\": 15.2}"
            };

            // Act
            await _repository.AddAsync(newCache);

            // Assert
            Assert.True(newCache.Id > 0);
            var result = await _repository.GetByIdAsync(newCache.Id);
            Assert.NotNull(result);
            Assert.Equal("TempiPreparazione", result.TipoStatistica);
            Assert.Equal("2024-01-17", result.Periodo);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingStatisticheCache()
        {
            // Arrange
            var updateDto = new StatisticheCacheDTO
            {
                Id = 1,
                TipoStatistica = "VenditeGiornaliereModificato",
                Periodo = "2024-01-15-modificato",
                Metriche = "{\"totaleVendite\": 1500.00, \"numeroOrdini\": 50}"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("VenditeGiornaliereModificato", result.TipoStatistica);
            Assert.Equal("2024-01-15-modificato", result.Periodo);
            Assert.Contains("1500.00", result.Metriche);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStatisticheCache()
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
        public async Task AggiornaCacheAsync_ShouldUpdateExistingCache()
        {
            // Arrange
            var nuoveMetriche = "{\"totaleVendite\": 2000.00, \"numeroOrdini\": 60}";

            // Act
            await _repository.AggiornaCacheAsync("VenditeGiornaliere", "2024-01-15", nuoveMetriche);

            // Assert
            var result = await _repository.GetByTipoAndPeriodoAsync("VenditeGiornaliere", "2024-01-15");
            Assert.NotNull(result);
            Assert.Contains("2000.00", result.Metriche);
        }

        [Fact]
        public async Task AggiornaCacheAsync_ShouldCreateNewCache_WhenNotExists()
        {
            // Arrange
            var nuoveMetriche = "{\"tempoMedio\": 7.8, \"tempoMassimo\": 12.5}";

            // Act
            await _repository.AggiornaCacheAsync("TempiPreparazione", "2024-01-17", nuoveMetriche);

            // Assert
            var result = await _repository.GetByTipoAndPeriodoAsync("TempiPreparazione", "2024-01-17");
            Assert.NotNull(result);
            Assert.Contains("7.8", result.Metriche);
        }

        [Fact]
        public async Task IsCacheValidaAsync_WithValidCache_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.IsCacheValidaAsync("VenditeGiornaliere", "2024-01-16", TimeSpan.FromHours(1));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsCacheValidaAsync_WithExpiredCache_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.IsCacheValidaAsync("ProdottiPopolari", "2024-01", TimeSpan.FromHours(12));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsCacheValidaAsync_WithNonExistingCache_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.IsCacheValidaAsync("Inesistente", "2024-01-01", TimeSpan.FromHours(1));

            // Assert
            Assert.False(result);
        }
    }
}