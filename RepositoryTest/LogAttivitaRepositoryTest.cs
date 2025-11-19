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
        private readonly LogAttivitaRepository _repository;        

        public LogAttivitaRepositoryTest()
        {
            _repository = new LogAttivitaRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var logAttivita = new List<LogAttivita>
            {
                new LogAttivita
                {
                    LogId = 1,
                    TipoAttivita = "PuliziaDatabase",
                    Descrizione = "Pulizia tabelle temporanee",
                    DataEsecuzione = DateTime.Now.AddHours(-4),
                    Dettagli = "Eliminati 150 record scaduti dalla tabella temp_orders"
                },
                new LogAttivita
                {
                    LogId = 2,
                    TipoAttivita = "Backup",
                    Descrizione = "Backup database giornaliero",
                    DataEsecuzione = DateTime.Now.AddHours(-3),
                    Dettagli = "Backup completato con successo. Dimensione: 45MB"
                },
                new LogAttivita
                {
                    LogId = 3,
                    TipoAttivita = "AggiornamentoMenu",
                    Descrizione = "Aggiornamento prezzi menu",
                    DataEsecuzione = DateTime.Now.AddHours(-2),
                    Dettagli = "Aggiornati prezzi per 12 articoli"
                },
                new LogAttivita
                {
                    LogId = 4,
                    TipoAttivita = "PuliziaDatabase",
                    Descrizione = "Pulizia log vecchi",
                    DataEsecuzione = DateTime.Now.AddHours(-1),
                    Dettagli = "Eliminati 2000 record di log più vecchi di 30 giorni"
                },
                new LogAttivita
                {
                    LogId = 5,
                    TipoAttivita = "Sincronizzazione",
                    Descrizione = "Sincronizzazione con sistema pagamenti",
                    DataEsecuzione = DateTime.Now.AddMinutes(-30),
                    Dettagli = "Sincronizzati 25 pagamenti pendenti"
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
        public async Task UpdateAsync_ShouldUpdateExistingLogAttivita()
        {
            // Arrange
            var updateDto = new LogAttivitaDTO
            {
                LogId = 1,
                TipoAttivita = "PuliziaDatabaseModificato",
                Descrizione = "Pulizia tabelle temporanee - AGGIORNATO",
                DataEsecuzione = DateTime.Now.AddHours(-1),
                Dettagli = "Eliminati 200 record scaduti - MODIFICATO"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("PuliziaDatabaseModificato", result.TipoAttivita);
            Assert.Equal("Pulizia tabelle temporanee - AGGIORNATO", result.Descrizione);
            Assert.Contains("200 record", result.Dettagli);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveLogAttivita()
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
        public async Task GetNumeroAttivitaAsync_ShouldReturnTotalCount()
        {
            // Act
            var result = await _repository.GetNumeroAttivitaAsync();

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
        public async Task UpdateAsync_WithNonExistingLogAttivita_ShouldNotThrow()
        {
            // Arrange
            var updateDto = new LogAttivitaDTO
            {
                LogId = 999,
                TipoAttivita = "Test",
                Descrizione = "Test",
                DataEsecuzione = DateTime.Now,
                Dettagli = "Test"
            };

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateDto));
            Assert.Null(exception);
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
            Assert.NotEqual(DateTime.MinValue, result.DataEsecuzione); // ✅ CORRETTO: sostituito Assert.NotNull
            Assert.InRange(result.DataEsecuzione, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }

        [Fact]
        public async Task GetByUtenteIdAsync_ShouldReturnFilteredLogAttivita()
        {
            // Arrange - Aggiungi log con utenteId
            var logWithUser = new LogAttivitaDTO
            {
                TipoAttivita = "TestUtente",
                Descrizione = "Test attività con utente",
                Dettagli = "Test",
                UtenteId = 1
            };
            await _repository.AddAsync(logWithUser);

            // Act
            var result = await _repository.GetByUtenteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, l => Assert.Equal(1, l.UtenteId));
        }

        [Fact]
        public async Task GetStatisticheAttivitaAsync_ShouldReturnCorrectStatistics()
        {
            // Act
            var result = await _repository.GetStatisticheAttivitaAsync();

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

            // Act
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
    }
}