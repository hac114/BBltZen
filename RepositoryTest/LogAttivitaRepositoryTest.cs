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
    public class LogAttivitaRepositoryTest : BaseTest
    {
        private readonly ILogAttivitaRepository _repository; // ✅ USA INTERFACCIA

        public LogAttivitaRepositoryTest()
        {
            _repository = new LogAttivitaRepository(_context); // ✅ IMPLEMENTAZIONE
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // ✅ AGGIUNGI UTENTI PER TESTARE TIPO_UTENTE
            var utenti = new List<Utenti>
            {
                new()
                {
                    UtenteId = 1,
                    TipoUtente = "gestore",
                    Email = "gestore@test.com",
                    Attivo = true
                },
                new()
                {
                    UtenteId = 2,
                    TipoUtente = "cliente",
                    Email = "cliente@test.com",
                    Attivo = true
                }
            };

            _context.Utenti.AddRange(utenti);

            var logAttivita = new List<LogAttivita>
            {
                new()
                {
                    LogId = 1,
                    TipoAttivita = "PuliziaDatabase",
                    Descrizione = "Pulizia tabelle temporanee",
                    DataEsecuzione = DateTime.Now.AddHours(-4),
                    Dettagli = "Eliminati 150 record scaduti dalla tabella temp_orders",
                    UtenteId = 1 // ✅ ASSEGNA UTENTE
                },
                new()
                {
                    LogId = 2,
                    TipoAttivita = "Backup",
                    Descrizione = "Backup database giornaliero",
                    DataEsecuzione = DateTime.Now.AddHours(-3),
                    Dettagli = "Backup completato con successo. Dimensione: 45MB",
                    UtenteId = 1 // ✅ ASSEGNA UTENTE
                },
                new()
                {
                    LogId = 3,
                    TipoAttivita = "AggiornamentoMenu",
                    Descrizione = "Aggiornamento prezzi menu",
                    DataEsecuzione = DateTime.Now.AddHours(-2),
                    Dettagli = "Aggiornati prezzi per 12 articoli"
                    // ✅ NULL UTENTE = "Sistema"
                },
                new()
                {
                    LogId = 4,
                    TipoAttivita = "PuliziaDatabase",
                    Descrizione = "Pulizia log vecchi",
                    DataEsecuzione = DateTime.Now.AddHours(-1),
                    Dettagli = "Eliminati 2000 record di log più vecchi di 30 giorni",
                    UtenteId = 2 // ✅ ASSEGNA UTENTE
                },
                new()
                {
                    LogId = 5,
                    TipoAttivita = "Sincronizzazione",
                    Descrizione = "Sincronizzazione con sistema pagamenti",
                    DataEsecuzione = DateTime.Now.AddMinutes(-30),
                    Dettagli = "Sincronizzati 25 pagamenti pendenti"
                    // ✅ NULL UTENTE = "Sistema"
                }
            };

            _context.LogAttivita.AddRange(logAttivita);
            _context.SaveChanges();
        }

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnAllLogAttivita()
        //{
        //    // Act
        //    var result = await _repository.GetAllAsync();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(5, result.Count());
        //}        

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLogAttivita()
        {
            // Act - ✅ AGGIUNGI PARAMETRI PAGINAZIONE
            var result = await _repository.GetByIdAsync(1, page: 1, pageSize: 10);
            var log = result.Data.First(); // ✅ USA .Data.First()

            // Assert
            Assert.NotNull(log);
            Assert.Equal(1, log.LogId);
            Assert.Equal("PuliziaDatabase", log.TipoAttivita);
            Assert.Equal("Pulizia tabelle temporanee", log.Descrizione);
            Assert.Contains("temp_orders", log.Dettagli);
        }


        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act - ✅ AGGIUNGI PARAMETRI PAGINAZIONE
            var result = await _repository.GetByIdAsync(999, page: 1, pageSize: 10);

            // Assert
            Assert.Empty(result.Data); // ✅ NON .Null, ma .Empty
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_ShouldReturnFilteredLogAttivita()
        {
            // Act - ✅ AGGIUNGI PARAMETRI PAGINAZIONE
            var result = await _repository.GetByTipoAttivitaAsync("PuliziaDatabase", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l => Assert.Equal("PuliziaDatabase", l.TipoAttivita));
        }


        //[Fact]
        //public async Task GetByPeriodoAsync_ShouldReturnLogAttivitaInPeriod()
        //{
        //    // Arrange
        //    var dataInizio = DateTime.Now.AddHours(-3.5);
        //    var dataFine = DateTime.Now.AddMinutes(-45);

        //    // Act
        //    var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

        //    // Assert
        //    var resultList = result.ToList();
        //    Assert.Equal(3, resultList.Count);
        //    Assert.All(resultList, l =>
        //    {
        //        Assert.True(l.DataEsecuzione >= dataInizio);
        //        Assert.True(l.DataEsecuzione <= dataFine);
        //    });
        //}

        [Fact]
        public async Task AddAsync_ShouldAddNewLogAttivita()
        {
            // Arrange
            var newLog = new LogAttivitaDTO
            {
                TipoAttivita = "ReportGenerazione",
                Descrizione = "Generazione report vendite mensili",
                Dettagli = "Report generato per il mese di Gennaio 2024"
            };

            // Act
            var result = await _repository.AddAsync(newLog);

            // Assert
            Assert.True(result.LogId > 0);
            Assert.Equal("ReportGenerazione", result.TipoAttivita);
            Assert.Equal("Generazione report vendite mensili", result.Descrizione);
            Assert.Contains("Gennaio 2024", result.Dettagli);
            Assert.NotEqual(DateTime.MinValue, result.DataEsecuzione);
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
        public async Task GetNumeroAttivitaAsync_ShouldReturnTotalCount()
        {
            // Act
            var result = await _repository.GetNumeroAttivitaAsync(null, null);

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetNumeroAttivitaAsync_WithPeriod_ShouldReturnFilteredCount()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act
            var result = await _repository.GetNumeroAttivitaAsync(dataInizio, dataFine);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task AddAsync_ShouldSetCorrectTimestamps()
        {
            // Arrange
            var newLog = new LogAttivitaDTO
            {
                TipoAttivita = "TestTimestamp",
                Descrizione = "Test verifica timestamp",
                Dettagli = "Test"
            };

            // Act
            var result = await _repository.AddAsync(newLog);

            // Assert
            Assert.NotEqual(DateTime.MinValue, result.DataEsecuzione);
            Assert.InRange(result.DataEsecuzione, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }

        [Fact]
        public async Task GetByUtenteIdAsync_ShouldReturnFilteredLogAttivita()
        {
            // Arrange
            var logWithUser = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtente",
                Descrizione = "Test attività con utente",
                Dettagli = "Test",
                UtenteId = 999
            };
            await _repository.AddAsync(logWithUser);

            // Act - ✅ AGGIUNGI PARAMETRI PAGINAZIONE
            var result = await _repository.GetByUtenteIdAsync(999, page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert - ✅ ORA FUNZIONA: resultList contiene LogAttivitaDTO con UtenteId
            Assert.Single(resultList);
            Assert.All(resultList, l => Assert.Equal(999, l.UtenteId)); // ✅ l è LogAttivitaDTO, ha UtenteId
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_ShouldReturnCorrectStatistics()
        {
            // Act
            var result = await _repository.GetStatisticheAttivitaAsync(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result["PuliziaDatabase"]);
            Assert.Equal(1, result["Backup"]);
            Assert.Equal(1, result["AggiornamentoMenu"]);
            Assert.Equal(1, result["Sincronizzazione"]);
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_WithPeriod_ShouldReturnFilteredStatistics()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act
            var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

            // Assert
            Assert.NotNull(result);
            Assert.True(result["PuliziaDatabase"] >= 1);
            Assert.True(result["Backup"] >= 1);
        }

        [Fact]
        public async Task AddAsync_ShouldIncludeUtenteId()
        {
            // Arrange
            var newLog = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtenteId",
                Descrizione = "Test con utente ID",
                Dettagli = "Test",
                UtenteId = 2
            };

            // Act
            var result = await _repository.AddAsync(newLog);

            // Assert
            Assert.Equal(2, result.UtenteId);

            // ✅ VERIFICA RECUPERO
            var retrievedResult = await _repository.GetByIdAsync(result.LogId, page: 1, pageSize: 10);
            var retrieved = retrievedResult.Data.First();
            Assert.NotNull(retrieved);
            Assert.Equal(2, retrieved.UtenteId);
        }

        // ✅ NUOVI TEST PER METODI FRONTEND

        [Fact]
        public async Task GetByPeriodoPerFrontendAsync_ShouldReturnFrontendDTOs()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act
            var result = await _repository.GetByPeriodoPerFrontendAsync(dataInizio, dataFine, page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.True(l.LogId > 0);
                Assert.True(l.DataEsecuzione >= dataInizio);
                Assert.True(l.DataEsecuzione <= dataFine);
            });
        }

        [Fact]
        public async Task GetByTipoAttivitaPerFrontendAsync_ShouldReturnFrontendDTOs()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync("PuliziaDatabase", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.Equal("PuliziaDatabase", l.TipoAttivita);
                Assert.True(l.LogId > 0);
            });
        }

        [Fact]
        public async Task CleanupOldLogsAsync_ShouldRemoveOldLogs()
        {
            // Arrange
            var oldLog = new LogAttivita
            {
                TipoAttivita = "TestVecchio",
                Descrizione = "Log vecchio per test cleanup",
                DataEsecuzione = DateTime.Now.AddDays(-100),
                Dettagli = "Test"
            };
            _context.LogAttivita.Add(oldLog);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _repository.CleanupOldLogsAsync(90);

            // Assert
            Assert.True(deletedCount >= 1);
            var remainingOldLogs = await _context.LogAttivita
                .Where(l => l.TipoAttivita == "TestVecchio")
                .ToListAsync();
            Assert.Empty(remainingOldLogs);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_WithCustomRetention_ShouldWork()
        {
            // Arrange
            var recentLog = new LogAttivita
            {
                TipoAttivita = "TestRecente",
                Descrizione = "Log recente",
                DataEsecuzione = DateTime.Now.AddDays(-7),
                Dettagli = "Test"
            };
            _context.LogAttivita.Add(recentLog);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _repository.CleanupOldLogsAsync(30);

            // Assert
            var recentLogs = await _context.LogAttivita
                .Where(l => l.TipoAttivita == "TestRecente")
                .ToListAsync();
            Assert.NotEmpty(recentLogs);
        }


        [Fact]
        public async Task FrontendDTOs_ShouldContainCalculatedProperties()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 10);
            var firstLog = result.Data.First(); // ✅ USA .Data.First()

            // Assert
            Assert.NotNull(firstLog.DataFormattata);
            Assert.NotNull(firstLog.UtenteDisplay);
            Assert.NotNull(firstLog.TipoAttivitaFormattato);
            Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}", firstLog.DataFormattata);
        }


        [Fact]
        public async Task FrontendDTOs_WithUtenteId_ShouldDisplayCorrectly()
        {
            // Arrange
            var logWithUser = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtenteFrontend",
                Descrizione = "Test utente frontend",
                UtenteId = 1
            };
            await _repository.AddAsync(logWithUser);

            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync("TestUtenteFrontend", page: 1, pageSize: 10);
            var log = result.Data.First(); // ✅ USA .Data.First()

            // Assert
            Assert.Equal("gestore", log.UtenteDisplay);
            Assert.Equal("gestore", log.TipoUtente);
        }

        [Fact]
        public async Task FrontendDTOs_WithoutUtenteId_ShouldDisplaySistema()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 10);
            var systemLog = result.Data.First(l => !l.UtenteId.HasValue); // ✅ USA .Data.First()

            // Assert
            Assert.Equal("Sistema", systemLog.UtenteDisplay);
            Assert.Null(systemLog.TipoUtente);
        }

        [Fact]
        public async Task FrontendDTOs_ShouldContainTipoUtente()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 10);
            var logWithUser = result.Data.First(l => l.UtenteId.HasValue); // ✅ USA .Data.First()

            // Assert
            Assert.NotNull(logWithUser.TipoUtente);
            Assert.Contains(logWithUser.TipoUtente, new[] { "gestore", "cliente" });
        }

        [Fact]
        public async Task FrontendDTOs_ShouldHaveCorrectUtenteDisplay()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            var gestoreLogs = resultList.Where(l => l.UtenteId == 1).ToList();
            var clienteLogs = resultList.Where(l => l.UtenteId == 2).ToList();
            var sistemaLogs = resultList.Where(l => !l.UtenteId.HasValue).ToList();

            Assert.All(gestoreLogs, l => Assert.Equal("gestore", l.UtenteDisplay));
            Assert.All(clienteLogs, l => Assert.Equal("cliente", l.UtenteDisplay));
            Assert.All(sistemaLogs, l => Assert.Equal("Sistema", l.UtenteDisplay));
        }


        [Fact]
        public async Task GetByTipoUtenteAsync_ShouldReturnFilteredLogs()
        {
            // Act
            var result = await _repository.GetByTipoUtenteAsync("gestore", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.NotEmpty(resultList);
            Assert.All(resultList, l => Assert.Equal("gestore", l.TipoUtente));
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_WithPartialMatch_ShouldWork()
        {
            // Act
            var result = await _repository.GetByTipoUtenteAsync("GEST", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.NotEmpty(resultList);
            Assert.All(resultList, l => Assert.Equal("gestore", l.TipoUtente));
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_WithEmptyString_ShouldReturnAll()
        {
            // Act
            var result = await _repository.GetByTipoUtenteAsync("", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(5, resultList.Count);
        }

        [Fact]
        public async Task GetByUtenteIdAsync_WithoutParameter_ShouldReturnAll()
        {
            // Act
            var result = await _repository.GetByUtenteIdAsync(null, page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(5, resultList.Count);
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_ShouldBeCaseInsensitive()
        {
            // Act
            var resultLower = await _repository.GetByTipoAttivitaAsync("pulizia", page: 1, pageSize: 10);
            var resultUpper = await _repository.GetByTipoAttivitaAsync("PULIZIA", page: 1, pageSize: 10);
            var resultMixed = await _repository.GetByTipoAttivitaAsync("Pulizia", page: 1, pageSize: 10);

            // Assert - ✅ USA .Data.Count()
            Assert.Equal(2, resultLower.Data.Count());
            Assert.Equal(2, resultUpper.Data.Count());
            Assert.Equal(2, resultMixed.Data.Count());
        }

        // ✅ NUOVO TEST: verifica che la paginazione funzioni correttamente
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLogAttivita()
        {
            // Act - ✅ USA GetByIdAsync(null, ...) invece di GetAllAsync()
            var result = await _repository.GetByIdAsync(null, page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count()); // ✅ USA result.Data.Count()
        }

        // ✅ NUOVO TEST: verifica la seconda pagina
        [Fact]
        public async Task GetAllAsync_WithSecondPage_ShouldWork()
        {
            // Act - ✅ USA GetByIdAsync(null, ...) invece di GetAllAsync()
            var result = await _repository.GetByIdAsync(null, page: 2, pageSize: 2);

            // Assert
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.Data.Count());
            Assert.True(result.HasPrevious);
            Assert.True(result.HasNext);
        }

        // ✅ NUOVO TEST: verifica l'ultima pagina
        [Fact]
        public async Task GetAllAsync_WithLastPage_ShouldWork()
        {
            // Act - 5 elementi con pageSize 2, l'ultima pagina è la 3
            var result = await _repository.GetByIdAsync(null, page: 3, pageSize: 2);

            // Assert - Ultima pagina dovrebbe avere 1 elemento
            Assert.Equal(3, result.Page);
            Assert.Single(result.Data);
            Assert.True(result.HasPrevious);
            Assert.False(result.HasNext); // Ultima pagina
        }

        // ✅ NUOVO TEST: verifica paginazione frontend
        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldSupportPagination()
        {
            // Act - ✅ USA GetByTipoAttivitaPerFrontendAsync(null, ...) invece di GetAllPerFrontendAsync()
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 3);

            // Assert
            Assert.Equal(1, result.Page);
            Assert.Equal(3, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.Data.Count());
        }

        // ✅ NUOVO TEST: verifica che ValidatePagination limiti a max 100
        [Fact]
        public async Task GetAllAsync_WithLargePageSize_ShouldBeLimitedTo100()
        {
            // Act - ✅ USA GetByIdAsync(null, ...) invece di GetAllAsync()
            var result = await _repository.GetByIdAsync(null, page: 1, pageSize: 200);

            // Assert
            Assert.Equal(100, result.PageSize); // ✅ DOVREBBE ESSERE LIMITATO A 100
        }

        // ✅ NUOVO TEST: verifica che l'endpoint senza ID restituisca tutto
        [Fact]
        public async Task GetByIdAsync_WithoutId_ShouldReturnAllLogs()
        {
            // Act
            var result = await _repository.GetByIdAsync(null, page: 1, pageSize: 10);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        // ✅ NUOVO TEST: verifica che l'endpoint frontend senza parametro restituisca tutto
        [Fact]
        public async Task GetByTipoAttivitaPerFrontendAsync_WithoutParameter_ShouldReturnAllLogs()
        {
            // Act - ✅ USA GetByTipoAttivitaPerFrontendAsync SENZA PARAMETRO
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync(null, page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.Equal(5, resultList.Count);
            Assert.All(resultList, l => Assert.True(l.LogId > 0));
        }

        // ✅ NUOVO TEST: verifica che l'endpoint tipo utente senza parametro restituisca tutto
        [Fact]
        public async Task GetByTipoUtenteAsync_WithoutParameter_ShouldReturnAllLogs()
        {
            // Act - chiama senza tipoUtente
            var result = await _repository.GetByTipoUtenteAsync(null, page: 1, pageSize: 10);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(5, result.Data.Count());
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_ShouldIncludeTipoUtenteFromJoin()
        {
            // Act - ✅ USA GetByTipoUtenteAsync
            var result = await _repository.GetByTipoUtenteAsync("gestore", page: 1, pageSize: 10);
            var resultList = result.Data.ToList(); // ✅ USA .Data.ToList()

            // Assert
            Assert.NotEmpty(resultList);
            Assert.All(resultList, l => Assert.Equal("gestore", l.TipoUtente));
        }

        [Fact]
        public async Task GetAllMethods_ShouldSupportPagination()
        {
            // Act - pagina 1 con 2 elementi
            var result = await _repository.GetByIdAsync(null, page: 1, pageSize: 2);

            // Assert
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Data.Count());
            Assert.False(result.HasPrevious);
            Assert.True(result.HasNext);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSupportPagination()
        {
            // Act - ✅ USA GetByIdAsync(null, ...) invece di GetAllAsync()
            var result = await _repository.GetByIdAsync(null, page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Data.Count());
        }
    }
}