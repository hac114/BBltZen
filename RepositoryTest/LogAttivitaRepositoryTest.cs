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
        private readonly BubbleTeaContext _context;

        public LogAttivitaRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"LogAttivitaTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
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
            await _repository.AddAsync(newLog);

            // Assert
            Assert.True(newLog.LogId > 0);
            var result = await _repository.GetByIdAsync(newLog.LogId);
            Assert.NotNull(result);
            Assert.Equal("ReportGenerazione", result.TipoAttivita);
            Assert.Equal("Generazione report vendite mensili", result.Descrizione);
            Assert.Contains("Gennaio 2024", result.Dettagli);
            Assert.NotNull(result.DataEsecuzione);
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
        public async Task UpdateAsync_WithNonExistingLogAttivita_ShouldThrowException()
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
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}