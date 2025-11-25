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
        
        public StatisticheCacheRepositoryTest()
        {
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
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestGetById",
                Periodo = "2024-01-24",
                Metriche = "{\"test\": \"getbyid\"}"
            };
            var addedCache = await _repository.AddAsync(newCache); // ✅ USA RISULTATO

            // Act
            var result = await _repository.GetByIdAsync(addedCache.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedCache.Id, result.Id);
            Assert.Equal("TestGetById", result.TipoStatistica);
            Assert.Equal("2024-01-24", result.Periodo);
            Assert.Contains("getbyid", result.Metriche);
            Assert.NotEqual(default, result.DataAggiornamento);
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
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestGetByTipoPeriodo",
                Periodo = "2024-01-25",
                Metriche = "{\"test\": \"tipoperiodo\"}"
            };
            await _repository.AddAsync(newCache);

            // Act
            var result = await _repository.GetByTipoAndPeriodoAsync("TestGetByTipoPeriodo", "2024-01-25");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestGetByTipoPeriodo", result.TipoStatistica);
            Assert.Equal("2024-01-25", result.Periodo);
            Assert.Contains("tipoperiodo", result.Metriche);
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
            var result = await _repository.AddAsync(newCache); // ✅ USA IL RISULTATO

            // Assert
            Assert.True(result.Id > 0); // ✅ VERIFICA ID GENERATO
            Assert.Equal("TempiPreparazione", result.TipoStatistica);
            Assert.Equal("2024-01-17", result.Periodo);
            Assert.NotEqual(default, result.DataAggiornamento); // ✅ VERIFICA DATA CREAZIONE
        }


        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingStatisticheCache()
        {
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestUpdate",
                Periodo = "2024-01-18",
                Metriche = "{\"test\": \"originale\"}"
            };
            var addedCache = await _repository.AddAsync(newCache); // ✅ USA RISULTATO

            var updateDto = new StatisticheCacheDTO
            {
                Id = addedCache.Id,
                TipoStatistica = "TestUpdateModificato",
                Periodo = "2024-01-18-modificato",
                Metriche = "{\"test\": \"modificato\"}"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(addedCache.Id);
            Assert.NotNull(result);
            Assert.Equal("TestUpdateModificato", result.TipoStatistica);
            Assert.Equal("2024-01-18-modificato", result.Periodo);
            Assert.Contains("modificato", result.Metriche);
        }
        [Fact]
        public async Task DeleteAsync_ShouldRemoveStatisticheCache()
        {
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestDelete",
                Periodo = "2024-01-19",
                Metriche = "{\"test\": \"delete\"}"
            };
            var addedCache = await _repository.AddAsync(newCache); // ✅ USA RISULTATO

            // Act
            await _repository.DeleteAsync(addedCache.Id);

            // Assert
            var result = await _repository.GetByIdAsync(addedCache.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var newCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestExists",
                Periodo = "2024-01-20",
                Metriche = "{\"test\": \"exists\"}"
            };
            var addedCache = await _repository.AddAsync(newCache); // ✅ USA RISULTATO

            // Act
            var result = await _repository.ExistsAsync(addedCache.Id);

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

        [Fact]
        public async Task UpdateAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Arrange
            var updateDto = new StatisticheCacheDTO
            {
                Id = 999,
                TipoStatistica = "Inesistente",
                Periodo = "2024-01-01",
                Metriche = "{\"test\": \"data\"}"
            };

            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _repository.UpdateAsync(updateDto)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_ForNonExistingId()
        {
            // Act & Assert - ✅ SILENT FAIL, NO EXCEPTION
            var exception = await Record.ExceptionAsync(() =>
                _repository.DeleteAsync(999)
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange
            var cache1 = new StatisticheCacheDTO
            {
                TipoStatistica = "TestDuplicato",
                Periodo = "2024-01-21",
                Metriche = "{\"test\": \"primo\"}"
            };
            await _repository.AddAsync(cache1);

            var cache2 = new StatisticheCacheDTO
            {
                TipoStatistica = "TestDuplicato", // ✅ STESSO TipoStatistica
                Periodo = "2024-01-21",           // ✅ STESSO Periodo
                Metriche = "{\"test\": \"secondo\"}"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.AddAsync(cache2)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_ForDuplicateCombinazione()
        {
            // Arrange - Crea due record con combinazioni DIVERSE
            var cache1 = new StatisticheCacheDTO
            {
                TipoStatistica = "TestUpdateDuplicato1",
                Periodo = "2024-01-22",
                Metriche = "{\"test\": \"primo\"}"
            };
            var addedCache1 = await _repository.AddAsync(cache1);

            var cache2 = new StatisticheCacheDTO
            {
                TipoStatistica = "TestUpdateDuplicato2",
                Periodo = "2024-01-23",
                Metriche = "{\"test\": \"secondo\"}"
            };
            var addedCache2 = await _repository.AddAsync(cache2);

            // Act & Assert - Prova a fare l'update del secondo record con la STESSA combinazione del primo
            var updateDto = new StatisticheCacheDTO
            {
                Id = addedCache2.Id,
                TipoStatistica = "TestUpdateDuplicato1", // ✅ STESSO TipoStatistica del primo
                Periodo = "2024-01-22",                  // ✅ STESSO Periodo del primo
                Metriche = "{\"test\": \"modificato\"}"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.UpdateAsync(updateDto)
            );

            Assert.Contains("esiste già", exception.Message.ToLower());
        }

        [Fact]
        public async Task GetStatisticheCarrelloByPeriodoAsync_WithValidPeriodo_ShouldReturnStatisticheCarrello()
        {
            // Arrange
            var statisticheCarrello = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 10,
                TotaleProdottiVenduti = 50,
                FatturatoTotale = 500.75m,
                DistribuzionePerTipologia =
                [
                    new() { TipoArticolo = "BS", Descrizione = "Bevanda Standard", QuantitaTotale = 30 }
                ],
                ProdottiPiuVenduti =
                [
                    new() { TipoArticolo = "BS", ArticoloId = 1, NomeProdotto = "Bubble Tea Classico", QuantitaVenduta = 15 }
                ],
                FasciaOrariaPiuAttiva = "14:00-16:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };

            await _repository.SalvaStatisticheCarrelloAsync("2024-01-25", statisticheCarrello);

            // Act
            var result = await _repository.GetStatisticheCarrelloByPeriodoAsync("2024-01-25");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.TotaleOrdini);
            Assert.Equal(50, result.TotaleProdottiVenduti);
            Assert.Equal(500.75m, result.FatturatoTotale);
            Assert.Single(result.DistribuzionePerTipologia);
            Assert.Single(result.ProdottiPiuVenduti);
        }

        [Fact]
        public async Task GetStatisticheCarrelloByPeriodoAsync_WithInvalidPeriodo_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetStatisticheCarrelloByPeriodoAsync("PeriodoInesistente");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SalvaStatisticheCarrelloAsync_ShouldSaveAndRetrieveStatistiche()
        {
            // Arrange
            var statisticheCarrello = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 5,
                TotaleProdottiVenduti = 25,
                FatturatoTotale = 250.50m,
                DistribuzionePerTipologia =
                [
                    new() { TipoArticolo = "D", Descrizione = "Dolce", QuantitaTotale = 10 }
                ],
                ProdottiPiuVenduti =
                [
                    new() { TipoArticolo = "D", ArticoloId = 2, NomeProdotto = "Tiramisù", QuantitaVenduta = 8 }
                ],
                FasciaOrariaPiuAttiva = "18:00-20:00",
                DataRiferimento = DateTime.UtcNow.Date.AddDays(-1),
                DataAggiornamento = DateTime.UtcNow
            };

            // Act
            await _repository.SalvaStatisticheCarrelloAsync("2024-01-26", statisticheCarrello);

            // Assert
            var result = await _repository.GetStatisticheCarrelloByPeriodoAsync("2024-01-26");
            Assert.NotNull(result);
            Assert.Equal(5, result.TotaleOrdini);
            Assert.Equal(25, result.TotaleProdottiVenduti);
            Assert.Equal("D", result.DistribuzionePerTipologia[0].TipoArticolo);
            Assert.Equal("Tiramisù", result.ProdottiPiuVenduti[0].NomeProdotto);
        }

        [Fact]
        public async Task IsStatisticheCarrelloValideAsync_WithValidCache_ShouldReturnTrue()
        {
            // Arrange
            var statisticheCarrello = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 3,
                TotaleProdottiVenduti = 12,
                FatturatoTotale = 120.00m,
                DistribuzionePerTipologia = [],
                ProdottiPiuVenduti = [],
                FasciaOrariaPiuAttiva = "12:00-14:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };

            await _repository.SalvaStatisticheCarrelloAsync("2024-01-27", statisticheCarrello);

            // Act
            var result = await _repository.IsStatisticheCarrelloValideAsync("2024-01-27", TimeSpan.FromHours(1));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsStatisticheCarrelloValideAsync_WithExpiredCache_ShouldReturnFalse()
        {
            // Arrange - Simula cache scaduta usando un periodo esistente dai dati di test
            // "ProdottiPopolari" ha DataAggiornamento = DateTime.Now.AddDays(-1)

            // Act
            var result = await _repository.IsStatisticheCarrelloValideAsync("2024-01", TimeSpan.FromHours(12));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetPeriodiDisponibiliCarrelloAsync_ShouldReturnAllCarrelloPeriods()
        {
            // Arrange
            var statisticheCarrello = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 1,
                TotaleProdottiVenduti = 5,
                FatturatoTotale = 50.00m,
                DistribuzionePerTipologia = [],
                ProdottiPiuVenduti = [],
                FasciaOrariaPiuAttiva = "10:00-12:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };

            await _repository.SalvaStatisticheCarrelloAsync("2024-01-28", statisticheCarrello);
            await _repository.SalvaStatisticheCarrelloAsync("2024-01-29", statisticheCarrello);

            // Act
            var result = await _repository.GetPeriodiDisponibiliCarrelloAsync();

            // Assert
            var periodi = result.ToList();
            Assert.Contains("2024-01-28", periodi);
            Assert.Contains("2024-01-29", periodi);
            Assert.All(periodi, p => Assert.NotNull(p));
        }

        // ✅ TEST PER STATISTICHE CACHE CONTROLLER - INTEGRATION
        [Fact]
        public async Task GetAllStatisticheCache_ShouldReturnAllRecords()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Console.WriteLine($"✅ GetAllStatisticheCache: {result.Count()} records");
        }

        [Fact]
        public async Task GetStatisticheCacheById_WithValidId_ShouldReturnRecord()
        {
            // Arrange - Crea un record di test
            var testCache = new StatisticheCacheDTO
            {
                TipoStatistica = "TestIntegration",
                Periodo = "2024-01-30",
                Metriche = "{\"test\": \"integration\"}"
            };
            var addedCache = await _repository.AddAsync(testCache);

            // Act
            var result = await _repository.GetByIdAsync(addedCache.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedCache.Id, result.Id);
            Assert.Equal("TestIntegration", result.TipoStatistica);
            Console.WriteLine($"✅ GetStatisticheCacheById: ID={result.Id}, Tipo={result.TipoStatistica}");
        }

        [Fact]
        public async Task GetStatisticheCacheByTipo_ShouldReturnFilteredRecords()
        {
            // Act
            var result = await _repository.GetByTipoAsync("VenditeGiornaliere");

            // Assert
            Assert.NotNull(result);
            Assert.All(result, item => Assert.Equal("VenditeGiornaliere", item.TipoStatistica));
            Console.WriteLine($"✅ GetStatisticheCacheByTipo: {result.Count()} records VenditeGiornaliere");
        }

        [Fact]
        public async Task GetStatisticheCarrelloByPeriodo_ShouldReturnCarrelloStats()
        {
            // Arrange - Prepara dati carrello
            var carrelloStats = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 8,
                TotaleProdottiVenduti = 40,
                FatturatoTotale = 400.25m,
                DistribuzionePerTipologia = new List<DistribuzioneProdottoDTO>(),
                ProdottiPiuVenduti = new List<ProdottoTopDTO>(),
                FasciaOrariaPiuAttiva = "16:00-18:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };
            await _repository.SalvaStatisticheCarrelloAsync("2024-01-31", carrelloStats);

            // Act
            var result = await _repository.GetStatisticheCarrelloByPeriodoAsync("2024-01-31");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.TotaleOrdini);
            Assert.Equal(400.25m, result.FatturatoTotale);
            Console.WriteLine($"✅ GetStatisticheCarrelloByPeriodo: {result.TotaleOrdini} ordini");
        }

        [Fact]
        public async Task SalvaStatisticheCarrello_ShouldPersistData()
        {
            // Arrange
            var carrelloStats = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 12,
                TotaleProdottiVenduti = 60,
                FatturatoTotale = 650.80m,
                DistribuzionePerTipologia = new List<DistribuzioneProdottoDTO>(),
                ProdottiPiuVenduti = new List<ProdottoTopDTO>(),
                FasciaOrariaPiuAttiva = "19:00-21:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };

            // Act
            await _repository.SalvaStatisticheCarrelloAsync("2024-02-01", carrelloStats);

            // Assert
            var result = await _repository.GetStatisticheCarrelloByPeriodoAsync("2024-02-01");
            Assert.NotNull(result);
            Assert.Equal(12, result.TotaleOrdini);
            Console.WriteLine($"✅ SalvaStatisticheCarrello: {result.TotaleOrdini} ordini salvati");
        }

        [Fact]
        public async Task IsStatisticheCarrelloValide_WithFreshData_ShouldReturnTrue()
        {
            // Arrange
            var carrelloStats = new StatisticheCarrelloDTO
            {
                TotaleOrdini = 3,
                TotaleProdottiVenduti = 15,
                FatturatoTotale = 150.00m,
                DistribuzionePerTipologia = new List<DistribuzioneProdottoDTO>(),
                ProdottiPiuVenduti = new List<ProdottoTopDTO>(),
                FasciaOrariaPiuAttiva = "11:00-13:00",
                DataRiferimento = DateTime.UtcNow.Date,
                DataAggiornamento = DateTime.UtcNow
            };
            await _repository.SalvaStatisticheCarrelloAsync("2024-02-02", carrelloStats);

            // Act
            var result = await _repository.IsStatisticheCarrelloValideAsync("2024-02-02", TimeSpan.FromHours(24));

            // Assert
            Assert.True(result);
            Console.WriteLine($"✅ IsStatisticheCarrelloValide: Cache valida = {result}");
        }

        [Fact]
        public async Task GetPeriodiDisponibiliCarrello_ShouldReturnPeriodsList()
        {
            // Act
            var result = await _repository.GetPeriodiDisponibiliCarrelloAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<string>>(result);
            Console.WriteLine($"✅ GetPeriodiDisponibiliCarrello: {result.Count()} periodi disponibili");
        }

        [Fact]
        public async Task AggiornaCacheStatistiche_ShouldUpdateExisting()
        {
            // Arrange
            var nuoveMetriche = "{\"test\": \"aggiornamento\", \"valore\": 999}";

            // Act
            await _repository.AggiornaCacheAsync("VenditeGiornaliere", "2024-01-16", nuoveMetriche);

            // Assert
            var result = await _repository.GetByTipoAndPeriodoAsync("VenditeGiornaliere", "2024-01-16");
            Assert.NotNull(result);
            Assert.Contains("999", result.Metriche);
            Console.WriteLine($"✅ AggiornaCacheStatistiche: Metriche aggiornate");
        }
    }
}