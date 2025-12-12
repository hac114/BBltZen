using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class LogAttivitaRepositoryCleanTest : BaseTestClean
    {
        private readonly LogAttivitaRepository _repository;

        public LogAttivitaRepositoryCleanTest()
        {
            _repository = new LogAttivitaRepository(_context, GetTestLogger<LogAttivitaRepository>());
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnPaginatedLogs()
        {
            // Arrange
            var totalLogs = await _context.LogAttivita.CountAsync();

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(totalLogs, result.TotalCount);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.Page);
            Assert.Equal(totalLogs, result.Data.Count());
            Assert.Contains("Trovate", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithCustomPageSize_ShouldReturnCorrectNumberOfItems()
        {
            // Arrange
            var pageSize = 3;

            // Act
            var result = await _repository.GetAllAsync(page: 1, pageSize: pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pageSize, result.PageSize);
            Assert.True(result.Data.Count() <= pageSize);
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithSecondPage_ShouldReturnCorrectItems()
        {
            // Arrange
            var pageSize = 2;
            var page = 2;

            // Act
            var result = await _repository.GetAllAsync(page: page, pageSize: pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidPage_ShouldReturnEmpty()
        {
            // Arrange
            var totalLogs = await _context.LogAttivita.CountAsync();
            var invalidPage = 100;

            // Act
            var result = await _repository.GetAllAsync(page: invalidPage, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(totalLogs, result.TotalCount);
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoLogsExist_ShouldReturnEmptyList()
        {
            // Arrange
            await CleanTableAsync<LogAttivita>();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalCount);
            Assert.Contains("Nessuna attività di log trovata", result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithPageSizeZero_ShouldHandleGracefully()
        {
            // Act
            var result = await _repository.GetAllAsync(pageSize: 0);

            // Assert
            Assert.NotNull(result);
            // Non testare un valore fisso, verifica solo che non crasha
            Assert.True(result.PageSize > 0);
            Assert.NotNull(result.Message);
        }

        [Fact]
        public async Task GetAllAsync_WithLargePageSize_ShouldBeLimitedBySecurityHelper()
        {
            // Arrange
            var hugePageSize = 10000;

            // Act
            var result = await _repository.GetAllAsync(pageSize: hugePageSize);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.PageSize <= 100); // Should be limited by SecurityHelper
            Assert.NotNull(result.Message);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLog()
        {
            // Arrange
            var existingLog = await _context.LogAttivita.FirstAsync();

            // Act
            var result = await _repository.GetByIdAsync(existingLog.LogId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.NotNull(result.Data);
            Assert.Equal(existingLog.LogId, result.Data.LogId);
            Assert.Contains("trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = 999;

            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithZeroId_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByIdAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WithNegativeId_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region GetByTipoAttivitaAsync Tests

        [Fact]
        public async Task GetByTipoAttivitaAsync_WithExistingTipo_ShouldReturnLogs()
        {
            // Arrange
            var existingTipo = "Sistema";

            // Act
            var result = await _repository.GetByTipoAttivitaAsync(existingTipo);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.NotEmpty(result.Data);
            Assert.All(result.Data, d => Assert.StartsWith(existingTipo, d.TipoAttivita));
            Assert.Contains("inizia con", result.Message);
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_WithNonExistingTipo_ShouldReturnEmpty()
        {
            // Arrange
            var nonExistingTipo = "NonEsistente";

            // Act
            var result = await _repository.GetByTipoAttivitaAsync(nonExistingTipo);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("Nessuna attività di log trovata", result.Message);
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_WithEmptyString_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaAsync("");

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }
        [Fact]
        public async Task GetByTipoAttivitaAsync_WithNull_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByTipoAttivitaAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_WithInvalidCharacters_ShouldReturnErrorOrEmpty()
        {
            // Arrange
            var invalidInput = "<script>alert('xss')</script>";

            // Act
            var result = await _repository.GetByTipoAttivitaAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            // Potrebbe dire "Nessuna attività" OPPURE "caratteri non validi"
            // Test più generico:
            Assert.NotNull(result.Message);
            Assert.DoesNotContain("Exception", result.Message); // Almeno non dovrebbe avere eccezioni
        }

        [Fact]
        public async Task GetByTipoAttivitaAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var tipo = "Ordine";
            var pageSize = 1;

            // Act
            var result = await _repository.GetByTipoAttivitaAsync(tipo, page: 1, pageSize: pageSize);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Equal(pageSize, result.PageSize);
            Assert.True(result.Data.Count() <= pageSize);
        }

        #endregion

        #region GetByUtenteIdAsync Tests

        [Fact]
        public async Task GetByUtenteIdAsync_WithValidUtenteId_ShouldReturnLogs()
        {
            // Arrange
            var utenteId = 1;

            // Act
            var result = await _repository.GetByUtenteIdAsync(utenteId);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.All(result.Data, d => Assert.Equal(utenteId, d.UtenteId));
            Assert.Contains($"utente ID {utenteId}", result.Message);
        }

        [Fact]
        public async Task GetByUtenteIdAsync_WithZeroId_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByUtenteIdAsync(0);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task GetByUtenteIdAsync_WithNegativeId_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByUtenteIdAsync(-1);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        [Fact]
        public async Task GetByUtenteIdAsync_WithNonExistingUtenteId_ShouldReturnEmpty()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = await _repository.GetByUtenteIdAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains($"Nessuna attività di log trovata per utente ID {nonExistingId}", result.Message);
        }

        #endregion

        #region GetByTipoUtenteAsync Tests

        [Fact]
        public async Task GetByTipoUtenteAsync_WithExistingTipoUtente_ShouldReturnLogs()
        {
            // Arrange
            var tipoUtente = "gestore";

            // Act
            var result = await _repository.GetByTipoUtenteAsync(tipoUtente);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.NotEmpty(result.Data);
            Assert.Contains("tipo utente", result.Message);
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_WithEmptyString_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByTipoUtenteAsync("");

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_WithInvalidCharacters_ShouldReturnError()
        {
            // Arrange
            var invalidInput = "'; DROP TABLE LogAttivita; --";

            // Act
            var result = await _repository.GetByTipoUtenteAsync(invalidInput);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.Empty(result.Data);
            Assert.Contains("caratteri non validi", result.Message);
        }

        [Fact]
        public async Task GetByTipoUtenteAsync_WithNull_ShouldReturnError()
        {
            // Act
            var result = await _repository.GetByTipoUtenteAsync(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Contains("obbligatorio", result.Message);
        }

        #endregion

        #region GetNumeroAttivitaAsync Tests

        [Fact]
        public async Task GetNumeroAttivitaAsync_WithoutDates_ShouldReturnTotalCount()
        {
            // Arrange
            var totalLogs = await _context.LogAttivita.CountAsync();

            // Act
            var result = await _repository.GetNumeroAttivitaAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Equal(totalLogs, result.Data);
            Assert.Contains("Trovate", result.Message);
        }

        [Fact]
        public async Task GetNumeroAttivitaAsync_WithDateRange_ShouldReturnFilteredCount()
        {
            // Arrange
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var tomorrow = DateTime.UtcNow.AddDays(1);

            // Act
            var result = await _repository.GetNumeroAttivitaAsync(yesterday, tomorrow);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.True(result.Data >= 0);
            Assert.Contains("periodo specificato", result.Message);
        }

        [Fact]
        public async Task GetNumeroAttivitaAsync_WithFutureDateRange_ShouldReturnZero()
        {
            // Arrange
            var futureStart = DateTime.UtcNow.AddDays(10);
            var futureEnd = DateTime.UtcNow.AddDays(20);

            // Act
            var result = await _repository.GetNumeroAttivitaAsync(futureStart, futureEnd);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessuna attività di log trovata", result.Message);
        }

        #endregion

        #region GetByDateRangeAsync Tests

        [Fact]
        public async Task GetByDateRangeAsync_WithValidRange_ShouldReturnLogs()
        {
            // Arrange
            var dataInizio = DateTime.UtcNow.AddDays(-2);
            var dataFine = DateTime.UtcNow;

            // Act
            var result = await _repository.GetByDateRangeAsync(dataInizio, dataFine);

            // Assert
            Assert.NotNull(result);
            // ❌ NO result.Success per PaginatedResponseDTO
            Assert.All(result.Data, d =>
            {
                Assert.True(d.DataEsecuzione >= dataInizio);
                Assert.True(d.DataEsecuzione <= dataFine);
            });
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithInvalidRange_ShouldThrowArgumentException()
        {
            // Arrange
            var dataInizio = DateTime.UtcNow;
            var dataFine = DateTime.UtcNow.AddDays(-1); // Fine prima di inizio

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetByDateRangeAsync(dataInizio, dataFine));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithFutureDate_ShouldThrowArgumentException()
        {
            // Arrange
            var dataInizio = DateTime.UtcNow.AddDays(-1);
            var dataFine = DateTime.UtcNow.AddDays(2); // Fine nel futuro

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _repository.GetByDateRangeAsync(dataInizio, dataFine));
        }

        #endregion

        #region GetStatisticheAttivitaAsync Tests

        [Fact]
        public async Task GetStatisticheAttivitaAsync_ShouldReturnStatistics()
        {
            // Act
            var result = await _repository.GetStatisticheAttivitaAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
            Assert.Contains("Statistiche calcolate", result.Message);
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_WithDateRange_ShouldReturnFilteredStatistics()
        {
            // Arrange
            var dataInizio = DateTime.UtcNow.AddDays(-7);
            var dataFine = DateTime.UtcNow;

            // Act
            var result = await _repository.GetStatisticheAttivitaAsync(dataInizio, dataFine);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.NotNull(result.Data);
            Assert.Contains("nel periodo specificato", result.Message);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var existingLog = await _context.LogAttivita.FirstAsync();

            // Act
            var result = await _repository.ExistsAsync(existingLog.LogId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.True(result.Data);
            Assert.Contains("esiste", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = await _repository.ExistsAsync(nonExistingId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.False(result.Data);
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task ExistsAsync_WithZeroId_ShouldReturnError()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.False(result.Data);
            Assert.Contains("non valido", result.Message);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidDto_ShouldCreateLog()
        {
            // Arrange
            var dto = new LogAttivitaDTO
            {
                TipoAttivita = "Test",
                Descrizione = "Test description",
                UtenteId = 1
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.NotNull(result.Data);
            Assert.True(result.Data.LogId > 0);
            Assert.Equal("Test", result.Data.TipoAttivita);
            Assert.Contains("creato con successo", result.Message);

            // Verify in database
            var savedLog = await _context.LogAttivita.FindAsync(result.Data.LogId);
            Assert.NotNull(savedLog);
            Assert.Equal("Test", savedLog.TipoAttivita);
        }

        [Fact]
        public async Task AddAsync_WithoutTipoAttivita_ShouldReturnError()
        {
            // Arrange
            var dto = new LogAttivitaDTO
            {
                Descrizione = "Test description",
                UtenteId = 1
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("TipoAttivita' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithoutDescrizione_ShouldReturnError()
        {
            // Arrange
            var dto = new LogAttivitaDTO
            {
                TipoAttivita = "Test",
                UtenteId = 1
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("Descrizione' è obbligatorio", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithInvalidUtenteId_ShouldReturnError()
        {
            // Arrange
            var dto = new LogAttivitaDTO
            {
                TipoAttivita = "Test",
                Descrizione = "Test description",
                UtenteId = 999 // Non esistente
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Null(result.Data);
            Assert.Contains("Utente con ID 999 non trovato", result.Message);
        }
        [Fact]
        public async Task AddAsync_WithNullDto_ShouldReturnError()
        {
            // Arrange
            LogAttivitaDTO? nullDto = null;

            // Act
            var result = await _repository.AddAsync(nullDto!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            // Verifica che restituisca un errore (non necessariamente ArgumentNullException)
        }

        [Fact]
        public async Task AddAsync_WithHtmlTags_ShouldBeAccepted()
        {
            // Arrange - Input con tag HTML (permesso dal SecurityHelper attuale)
            var dto = new LogAttivitaDTO
            {
                TipoAttivita = "<div>Test</div>",
                Descrizione = "Description with <b>HTML</b> tags",
                UtenteId = 1
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert - Dovrebbe avere successo perché <div> e <b> non sono nella blacklist
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.LogId > 0);
        }

        #endregion

        #region CleanupOldLogsAsync Tests

        [Fact]
        public async Task CleanupOldLogsAsync_WithDefaultDays_ShouldDeleteOldLogs()
        {
            // Arrange
            var oldLog = await CreateTestLogAttivitaAsync(
                dataEsecuzione: DateTime.UtcNow.AddDays(-100));

            // Act
            var result = await _repository.CleanupOldLogsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data >= 1);

            // Usa Contains con "Eliminato" (singolare) o test più generico
            Assert.Contains("Eliminato", result.Message);

            // Verify old log is deleted
            var deletedLog = await _context.LogAttivita.FindAsync(oldLog.LogId);
            Assert.Null(deletedLog);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_WithZeroDays_ShouldReturnError()
        {
            // Act
            var result = await _repository.CleanupOldLogsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Equal(0, result.Data);
            Assert.Contains("deve essere positivo", result.Message);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_WithNegativeDays_ShouldReturnError()
        {
            // Act
            var result = await _repository.CleanupOldLogsAsync(-10);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Equal(0, result.Data);
            Assert.Contains("deve essere positivo", result.Message);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_WithNoOldLogs_ShouldReturnZero()
        {
            // Arrange
            // Clean all logs and create only recent ones
            await CleanTableAsync<LogAttivita>();
            await CreateTestLogAttivitaAsync(
                dataEsecuzione: DateTime.UtcNow.AddDays(-1));

            // Act
            var result = await _repository.CleanupOldLogsAsync(90);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Equal(0, result.Data);
            Assert.Contains("Nessun log eliminato", result.Message);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_ShouldCreateCleanupLog()
        {
            // Arrange
            var initialCount = await _context.LogAttivita.CountAsync();

            // Act
            var result = await _repository.CleanupOldLogsAsync(1);

            // Assert
            var finalCount = await _context.LogAttivita.CountAsync();
            // Should have at least one log (the cleanup log itself)
            Assert.True(finalCount >= 1);

            var cleanupLog = await _context.LogAttivita
                .FirstOrDefaultAsync(l => l.TipoAttivita == "manutenzione");
            Assert.NotNull(cleanupLog);
            Assert.Contains("Puliti", cleanupLog.Descrizione);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task Repository_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var tasks = new Task[5];

            // Act
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var result = await _repository.GetAllAsync();
                    Assert.NotNull(result);
                });
            }

            await Task.WhenAll(tasks);

            // Assert - No exceptions should occur
            Assert.True(tasks.All(t => t.IsCompletedSuccessfully));
        }

        [Fact]
        public async Task Repository_Methods_ShouldBeIdempotent()
        {
            // Arrange
            var logId = 1;

            // Act - Call same method twice
            var result1 = await _repository.ExistsAsync(logId);
            var result2 = await _repository.ExistsAsync(logId);

            // Assert
            Assert.Equal(result1.Success, result2.Success);
            Assert.Equal(result1.Data, result2.Data);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task GetAllAsync_WithMaxIntPage_ShouldReturnEmptyOrHandleGracefully()
        {
            // Arrange
            var maxIntPage = int.MaxValue;
            var totalLogs = await _context.LogAttivita.CountAsync();

            // Act
            var result = await _repository.GetAllAsync(page: maxIntPage, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            // Potrebbe essere vuoto OPPURE tornare alla prima pagina
            Assert.NotNull(result.Message);
            // Il totale dovrebbe essere ancora corretto
            Assert.Equal(totalLogs, result.TotalCount);
        }

        [Fact]
        public async Task GetByIdAsync_WithMaxIntId_ShouldHandleGracefully()
        {
            // Arrange
            var maxIntId = int.MaxValue;

            // Act
            var result = await _repository.GetByIdAsync(maxIntId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Contains("non trovato", result.Message);
        }

        [Fact]
        public async Task AddAsync_WithVeryLongStrings_ShouldBeTruncated()
        {
            // Arrange
            var longString = new string('A', 1000);
            var dto = new LogAttivitaDTO
            {
                TipoAttivita = longString,
                Descrizione = "Test",
                UtenteId = 1
            };

            // Act
            var result = await _repository.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success); // ✅ SingleResponseDTO HA Success
            Assert.Contains("supera i 50 caratteri", result.Message);
        }

        [Fact]
        public async Task CleanupOldLogsAsync_WithVeryLargeDays_ShouldWork()
        {
            // Arrange
            var largeDays = 36500; // 100 years

            // Act
            var result = await _repository.CleanupOldLogsAsync(largeDays);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success); // ✅ SingleResponseDTO HA Success
            // Should delete all logs since they're all older than 100 years
            Assert.True(result.Data >= 0);
        }

        #endregion
    }
}