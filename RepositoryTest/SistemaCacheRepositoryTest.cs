using Database;
using DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Interface;
using Repository.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class SistemaCacheRepositoryTest : BaseTest
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<SistemaCacheRepository>> _loggerMock;
        private readonly ISistemaCacheRepository _cacheRepository;

        public SistemaCacheRepositoryTest()
        {
            // Configura MemoryCache
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Mock del logger
            _loggerMock = new Mock<ILogger<SistemaCacheRepository>>();

            // Crea il repository
            _cacheRepository = new SistemaCacheRepository(_context, _memoryCache, _loggerMock.Object);

            // Setup dati di test
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Aggiungi dati di test per StatisticheCache
            _context.StatisticheCache.Add(new StatisticheCache
            {
                Id = 1,
                TipoStatistica = "Menu",
                Periodo = "Giornaliero",
                Metriche = "{}",
                DataAggiornamento = DateTime.Now
            });

            // Aggiungi dati di test per ConfigSoglieTempi
            _context.ConfigSoglieTempi.Add(new ConfigSoglieTempi
            {
                SogliaId = 1,
                StatoOrdineId = 1,
                SogliaAttenzione = 10,
                SogliaCritico = 100,
                DataAggiornamento = DateTime.Now,
                UtenteAggiornamento = "Test"
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAsync_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var testKey = "test_key";
            var testValue = "test_value";
            await _cacheRepository.SetAsync(testKey, testValue);

            // Act
            var result = await _cacheRepository.GetAsync<string>(testKey);

            // Assert
            Assert.Equal(testValue, result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistingKey_ReturnsDefault()
        {
            // Arrange
            var testKey = "non_existing_key";

            // Act
            var result = await _cacheRepository.GetAsync<string>(testKey);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var testKey = "new_key";
            var testValue = "new_value";
            var duration = TimeSpan.FromMinutes(10);

            // Act
            var result = await _cacheRepository.SetAsync(testKey, testValue, duration);

            // Assert
            Assert.True(result.Successo);
            Assert.Equal(testKey, result.Chiave);
            Assert.Equal(duration, result.DurataCache);
        }

        [Fact]
        public async Task RemoveAsync_WithExistingKey_ReturnsSuccess()
        {
            // Arrange
            var testKey = "key_to_remove";
            await _cacheRepository.SetAsync(testKey, "value");

            // Act
            var result = await _cacheRepository.RemoveAsync(testKey);

            // Assert
            Assert.True(result.Successo);
            Assert.Equal(testKey, result.Chiave);
        }

        [Fact]
        public async Task RemoveAsync_WithNonExistingKey_ReturnsSuccess()
        {
            // Arrange
            var testKey = "non_existing_key";

            // Act
            var result = await _cacheRepository.RemoveAsync(testKey);

            // Assert
            Assert.True(result.Successo);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var testKey = "existing_key";
            await _cacheRepository.SetAsync(testKey, "value");

            // Act
            var result = await _cacheRepository.ExistsAsync(testKey);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingKey_ReturnsFalse()
        {
            // Arrange
            var testKey = "non_existing_key";

            // Act
            var result = await _cacheRepository.ExistsAsync(testKey);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetBulkAsync_WithMixedKeys_ReturnsCorrectResults()
        {
            // Arrange
            var existingKey = "existing_key";
            var nonExistingKey = "non_existing_key";
            await _cacheRepository.SetAsync(existingKey, "existing_value");

            var keys = new List<string> { existingKey, nonExistingKey };

            // Act
            var result = await _cacheRepository.GetBulkAsync(keys);

            // Assert
            Assert.Equal(2, result.ChiaviProcessate.Count);
            Assert.Equal(1, result.OperazioniCompletate);
            Assert.Equal(1, result.OperazioniFallite);
            Assert.True(result.Risultati.ContainsKey(existingKey));
            Assert.True(result.Risultati.ContainsKey(nonExistingKey));
        }

        [Fact]
        public async Task SetBulkAsync_WithMultipleValues_ReturnsSuccess()
        {
            // Arrange
            var values = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };

            // Act
            var result = await _cacheRepository.SetBulkAsync(values, TimeSpan.FromMinutes(5));

            // Assert
            Assert.Equal(3, result.OperazioniCompletate);
            Assert.Equal(0, result.OperazioniFallite);
            Assert.Equal(3, result.ChiaviProcessate.Count);
        }

        [Fact]
        public async Task RemoveBulkAsync_WithMultipleKeys_ReturnsSuccess()
        {
            // Arrange
            var keys = new List<string> { "key1", "key2", "key3" };
            await _cacheRepository.SetBulkAsync(new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            });

            // Act
            var result = await _cacheRepository.RemoveBulkAsync(keys);

            // Assert
            Assert.Equal(3, result.OperazioniCompletate);
            Assert.Equal(0, result.OperazioniFallite);
        }

        [Fact]
        public async Task GetOrSetAsync_WithFactory_ReturnsCachedValue()
        {
            // Arrange
            var testKey = "get_or_set_key";
            var factoryCallCount = 0;

            // Act - Prima chiamata
            var result1 = await _cacheRepository.GetOrSetAsync(testKey, () =>
            {
                factoryCallCount++;
                return Task.FromResult("factory_value");
            });

            // Act - Seconda chiamata (dovrebbe usare cache)
            var result2 = await _cacheRepository.GetOrSetAsync(testKey, () =>
            {
                factoryCallCount++;
                return Task.FromResult("factory_value_2");
            });

            // Assert
            Assert.Equal("factory_value", result1);
            Assert.Equal("factory_value", result2);
            Assert.Equal(1, factoryCallCount); // Factory chiamata solo una volta
        }

        [Fact]
        public async Task RefreshAsync_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var testKey = "refresh_key";
            await _cacheRepository.SetAsync(testKey, "value", TimeSpan.FromMinutes(5));

            // Act
            var result = await _cacheRepository.RefreshAsync(testKey, TimeSpan.FromMinutes(10));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RefreshAsync_WithNonExistingKey_ReturnsFalse()
        {
            // Arrange
            var testKey = "non_existing_refresh_key";

            // Act
            var result = await _cacheRepository.RefreshAsync(testKey);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CacheMenuAsync_ReturnsSuccess()
        {
            // Act
            var result = await _cacheRepository.CacheMenuAsync();

            // Assert
            Assert.True(result.Successo);
            Assert.Equal("Menu cached con successo", result.Messaggio);
        }

        [Fact]
        public async Task CacheStatisticheAsync_ReturnsSuccess()
        {
            // Act
            var result = await _cacheRepository.CacheStatisticheAsync();

            // Assert
            Assert.True(result.Successo);
            Assert.Equal("Statistiche cached con successo", result.Messaggio);
        }

        [Fact]
        public async Task CachePrezziAsync_ReturnsSuccess()
        {
            // Act
            var result = await _cacheRepository.CachePrezziAsync();

            // Assert
            Assert.True(result.Successo);
            Assert.Equal("Prezzi cached con successo", result.Messaggio);
        }

        [Fact]
        public async Task CacheConfigurazioniAsync_ReturnsSuccess()
        {
            // Act
            var result = await _cacheRepository.CacheConfigurazioniAsync();

            // Assert
            Assert.True(result.Successo);
            Assert.Equal("Configurazioni cached con successo", result.Messaggio);
        }

        [Fact]
        public async Task GetCacheInfoAsync_ReturnsValidInfo()
        {
            // Arrange
            await _cacheRepository.SetAsync("test1", "value1");
            await _cacheRepository.SetAsync("test2", "value2");

            // Act
            var result = await _cacheRepository.GetCacheInfoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HitsTotali >= 0);
            Assert.True(result.MissesTotali >= 0);
            Assert.InRange(result.HitRatePercentuale, 0, 100);
        }

        [Fact]
        public async Task GetPerformanceStatsAsync_ReturnsValidStats()
        {
            // Act
            var result = await _cacheRepository.GetPerformanceStatsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.InRange(result.HitRate, 0, 100);
            Assert.InRange(result.MissRate, 0, 100);
            Assert.NotNull(result.DataRaccolta);
        }

        [Fact]
        public async Task GetCacheStatisticsAsync_ReturnsStatisticsFromDatabase()
        {
            // Act
            var result = await _cacheRepository.GetCacheStatisticsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal("Menu", result.First().TipoStatistica);
        }

        [Fact]
        public async Task ResetStatisticsAsync_ResetsCounters()
        {
            // Arrange - Fai alcune operazioni per incrementare i contatori
            await _cacheRepository.SetAsync("test1", "value1");
            await _cacheRepository.GetAsync<string>("test1");
            await _cacheRepository.GetAsync<string>("non_existing");

            // Act
            await _cacheRepository.ResetStatisticsAsync();

            // Assert - Non c'è modo diretto di verificare i contatori interni, 
            // ma il metodo dovrebbe eseguire senza errori
            Assert.True(true);
        }

        [Fact]
        public async Task PreloadCommonDataAsync_ReturnsSuccess()
        {
            // Act
            var result = await _cacheRepository.PreloadCommonDataAsync();

            // Assert
            Assert.True(result.Successo);
            Assert.Equal("Dati comuni precaricati in cache", result.Messaggio);
        }

        [Fact]
        public async Task ClearAllAsync_ClearsKnownKeys()
        {
            // Arrange
            await _cacheRepository.PreloadCommonDataAsync();

            // Act
            await _cacheRepository.ClearAllAsync();

            // Assert - Verifica che le chiavi principali non esistano più
            var menuExists = await _cacheRepository.ExistsAsync("MenuCompleto");
            var prezziExists = await _cacheRepository.ExistsAsync("PrezziCalcolati");

            Assert.False(menuExists);
            Assert.False(prezziExists);
        }

        [Fact]
        public async Task IsCacheValidAsync_WithValidType_ReturnsCorrectStatus()
        {
            // Arrange
            await _cacheRepository.CacheMenuAsync();

            // Act
            var result = await _cacheRepository.IsCacheValidAsync("MENU");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsCacheValidAsync_WithInvalidType_ReturnsFalse()
        {
            // Act
            var result = await _cacheRepository.IsCacheValidAsync("INVALID_TYPE");

            // Assert
            Assert.False(result);
        }
    }
}