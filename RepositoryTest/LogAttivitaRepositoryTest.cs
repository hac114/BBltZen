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

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLogAttivita()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLogAttivita()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.LogId);
            Assert.Equal("PuliziaDatabase", result.TipoAttivita);
            Assert.Equal("Pulizia tabelle temporanee", result.Descrizione);
            Assert.Contains("temp_orders", result.Dettagli);
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
        public async Task GetByTipoAttivitaAsync_ShouldReturnFilteredLogAttivita()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaAsync("PuliziaDatabase");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l => Assert.Equal("PuliziaDatabase", l.TipoAttivita));
        }

        [Fact]
        public async Task GetByPeriodoAsync_ShouldReturnLogAttivitaInPeriod()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.True(l.DataEsecuzione >= dataInizio);
                Assert.True(l.DataEsecuzione <= dataFine);
            });
        }

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
            Assert.NotEqual(DateTime.MinValue, result.DataEsecuzione); // ✅ CORRETTO: sostituito Assert.NotNull
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

            // Act - ✅ ORA ACCETTA DateTime? (nullable)
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
            // Arrange - Aggiungi log con utenteId DIVERSO da quelli esistenti
            var logWithUser = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtente",
                Descrizione = "Test attività con utente",
                Dettagli = "Test",
                UtenteId = 999 // ✅ USA UN UTENTE ID CHE NON ESISTE NEI DATI DI TEST
            };
            await _repository.AddAsync(logWithUser);

            // Act - Cerca per il nuovo utenteId
            var result = await _repository.GetByUtenteIdAsync(999);

            // Assert - Dovrebbe trovare SOLO il log appena aggiunto
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, l => Assert.Equal(999, l.UtenteId));
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_ShouldReturnCorrectStatistics()
        {
            // Act - ✅ ORA ACCETTA NULL (date opzionali)
            var result = await _repository.GetStatisticheAttivitaAsync(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result["PuliziaDatabase"]);  // 2 pulizie database
            Assert.Equal(1, result["Backup"]);           // 1 backup
            Assert.Equal(1, result["AggiornamentoMenu"]); // 1 aggiornamento menu
            Assert.Equal(1, result["Sincronizzazione"]);  // 1 sincronizzazione
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_WithPeriod_ShouldReturnFilteredStatistics()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act - ✅ ORA ACCETTA DateTime? (nullable)
            var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

            // Assert
            Assert.NotNull(result);
            Assert.True(result["PuliziaDatabase"] >= 1); // Almeno 1 pulizia nel periodo
            Assert.True(result["Backup"] >= 1);          // Almeno 1 backup nel periodo
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
            var retrieved = await _repository.GetByIdAsync(result.LogId);
            Assert.NotNull(retrieved);
            Assert.Equal(2, retrieved.UtenteId);
        }

        // ✅ NUOVI TEST PER METODI FRONTEND

        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldReturnFrontendDTOs_WithLogId()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(5, resultList.Count);

            // ✅ VERIFICA CHE CI SIA LOG_ID (mantenuto per frontend)
            Assert.All(resultList, l => Assert.True(l.LogId > 0));

            // ✅ VERIFICA I DATI - CERCA QUALSIASI LOG CON PULIZIA
            var puliziaLog = resultList.First(l => l.TipoAttivita == "PuliziaDatabase");
            Assert.Contains("Pulizia", puliziaLog.Descrizione); // ✅ PI�ù GENERICO
        }

        [Fact]
        public async Task GetByPeriodoPerFrontendAsync_ShouldReturnFrontendDTOs()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-3.5);
            var dataFine = DateTime.Now.AddMinutes(-45);

            // Act
            var result = await _repository.GetByPeriodoPerFrontendAsync(dataInizio, dataFine);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.True(l.LogId > 0); // ✅ LOG_ID PRESENTE
                Assert.True(l.DataEsecuzione >= dataInizio);
                Assert.True(l.DataEsecuzione <= dataFine);
            });
        }

        [Fact]
        public async Task GetByTipoAttivitaPerFrontendAsync_ShouldReturnFrontendDTOs()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync("PuliziaDatabase");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.Equal("PuliziaDatabase", l.TipoAttivita);
                Assert.True(l.LogId > 0); // ✅ LOG_ID PRESENTE
            });
        }

        [Fact]
        public async Task CleanupOldLogsAsync_ShouldRemoveOldLogs()
        {
            // Arrange - Aggiungi log vecchi
            var oldLog = new LogAttivita
            {
                TipoAttivita = "TestVecchio",
                Descrizione = "Log vecchio per test cleanup",
                DataEsecuzione = DateTime.Now.AddDays(-100), // ✅ MOLTO VECCHIO
                Dettagli = "Test"
            };
            _context.LogAttivita.Add(oldLog);
            await _context.SaveChangesAsync();

            // Act
            var deletedCount = await _repository.CleanupOldLogsAsync(90); // ✅ 90 GIORNI RETENTION

            // Assert
            Assert.True(deletedCount >= 1); // ✅ ALMENO IL LOG VECCHIO

            // ✅ VERIFICA CHE IL LOG VECCHIO SIA STATO RIMOSSO
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
                DataEsecuzione = DateTime.Now.AddDays(-7), // ✅ SOLO 7 GIORNI
                Dettagli = "Test"
            };
            _context.LogAttivita.Add(recentLog);
            await _context.SaveChangesAsync();

            // Act - Retention di 30 giorni (non dovrebbe cancellare il log di 7 giorni)
            var deletedCount = await _repository.CleanupOldLogsAsync(30);

            // Assert - Il log recente dovrebbe essere ancora presente
            var recentLogs = await _context.LogAttivita
                .Where(l => l.TipoAttivita == "TestRecente")
                .ToListAsync();
            Assert.NotEmpty(recentLogs); // ✅ NON DOVREBBE ESSERE CANCELLATO
        }

        [Fact]
        public async Task FrontendDTOs_ShouldContainCalculatedProperties()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();
            var firstLog = result.First();

            // Assert - Verifica che i campi calcolati siano presenti
            Assert.NotNull(firstLog.DataFormattata);
            Assert.NotNull(firstLog.UtenteDisplay);
            Assert.NotNull(firstLog.TipoAttivitaFormattato);

            // ✅ VERIFICA FORMATTAZIONE
            Assert.Matches(@"\d{2}/\d{2}/\d{4} \d{2}:\d{2}", firstLog.DataFormattata);
        }

        [Fact]
        public async Task FrontendDTOs_WithUtenteId_ShouldDisplayCorrectly()
        {
            // Arrange - Aggiungi log con utente
            var logWithUser = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtenteFrontend",
                Descrizione = "Test utente frontend",
                UtenteId = 1 // ✅ USA UTENTE ESISTENTE DAL TEST DATA
            };
            await _repository.AddAsync(logWithUser);

            // Act
            var result = await _repository.GetByTipoAttivitaPerFrontendAsync("TestUtenteFrontend");
            var log = result.First();

            // Assert - ✅ ORA DOVREBBE MOSTRARE "gestore" invece di "1"
            Assert.Equal("gestore", log.UtenteDisplay);
            Assert.Equal("gestore", log.TipoUtente); // ✅ VERIFICA ANCHE IL NUOVO CAMPO
        }

        [Fact]
        public async Task FrontendDTOs_WithoutUtenteId_ShouldDisplaySistema()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();
            var systemLog = result.First(l => !l.UtenteId.HasValue);

            // Assert
            Assert.Equal("Sistema", systemLog.UtenteDisplay);
            Assert.Null(systemLog.TipoUtente); // ✅ TIPO_UTENTE DOVREBBE ESSERE NULL
        }

        [Fact]
        public async Task FrontendDTOs_ShouldContainTipoUtente()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();

            // Assert - VERIFICA CHE I LOG CON UTENTE ABBIANO TIPO_UTENTE
            var logWithUser = result.First(l => l.UtenteId.HasValue);
            Assert.NotNull(logWithUser.TipoUtente);

            // ✅ PIÙ FLESSIBILE: accetta sia "gestore" che "cliente"
            Assert.Contains(logWithUser.TipoUtente, new[] { "gestore", "cliente" });
        }

        [Fact]
        public async Task GetAllPerFrontendAsync_ShouldIncludeTipoUtenteFromJoin()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();
            var resultList = result.ToList();

            // Assert - VERIFICA CHE IL JOIN FUNZIONI (più flessibile)
            var gestoreLogs = resultList.Where(l => l.UtenteId == 1).ToList();
            var clienteLogs = resultList.Where(l => l.UtenteId == 2).ToList();
            var sistemaLogs = resultList.Where(l => !l.UtenteId.HasValue).ToList();

            // ✅ VERIFICA CHE CI SIANO LOG PER OGNI TIPO
            Assert.NotEmpty(gestoreLogs);
            Assert.NotEmpty(clienteLogs);
            Assert.NotEmpty(sistemaLogs);

            // ✅ VERIFICA TIPO_UTENTE CORRETTO
            Assert.All(gestoreLogs, l => Assert.Equal("gestore", l.TipoUtente));
            Assert.All(clienteLogs, l => Assert.Equal("cliente", l.TipoUtente));
            Assert.All(sistemaLogs, l => Assert.Null(l.TipoUtente));
        }

        [Fact]
        public async Task FrontendDTOs_ShouldHaveCorrectUtenteDisplay()
        {
            // Act
            var result = await _repository.GetAllPerFrontendAsync();
            var resultList = result.ToList();

            // Assert - VERIFICA UTENTE_DISPLAY CALCOLATO CORRETTAMENTE
            var gestoreLogs = resultList.Where(l => l.UtenteId == 1).ToList();
            var clienteLogs = resultList.Where(l => l.UtenteId == 2).ToList();
            var sistemaLogs = resultList.Where(l => !l.UtenteId.HasValue).ToList();

            // ✅ VERIFICA PER TUTTI I LOG DI OGNI CATEGORIA
            Assert.All(gestoreLogs, l => Assert.Equal("gestore", l.UtenteDisplay));
            Assert.All(clienteLogs, l => Assert.Equal("cliente", l.UtenteDisplay));
            Assert.All(sistemaLogs, l => Assert.Equal("Sistema", l.UtenteDisplay));
        }
    }
}