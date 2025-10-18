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
    public class LogAccessiRepositoryTest : BaseTest
    {
        private readonly LogAccessiRepository _repository;
        private readonly BubbleTeaContext _context;

        public LogAccessiRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"LogAccessiTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new LogAccessiRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea dati necessari per i test
            var tavoli = new List<Tavolo>
            {
                new Tavolo
                {
                    TavoloId = 1,
                    Disponibile = true,
                    Numero = 1,
                    Zona = "terrazza"
                }
            };

            var clienti = new List<Cliente>
            {
                new Cliente
                {
                    ClienteId = 1,
                    TavoloId = 1,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new Cliente
                {
                    ClienteId = 2,
                    TavoloId = 1,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

            var utenti = new List<Utenti>
            {
                new Utenti
                {
                    UtenteId = 1,
                    ClienteId = null,
                    Email = "gestore@bubbleteazen.com",
                    PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                    TipoUtente = "gestore",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    UltimoAccesso = null,
                    Attivo = true
                },
                new Utenti
                {
                    UtenteId = 2,
                    ClienteId = null,
                    Email = "operatore@bubbleteazen.com",
                    PasswordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye3s3B9yX7U7Jq.7c6q8q7q6q8q7q6q8q7",
                    TipoUtente = "operatore",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    UltimoAccesso = null,
                    Attivo = true
                }
            };

            var logAccessi = new List<LogAccessi>
            {
                new LogAccessi
                {
                    LogId = 1,
                    UtenteId = 1,
                    ClienteId = null,
                    TipoAccesso = "Login",
                    Esito = "Successo",
                    IpAddress = "192.168.1.100",
                    UserAgent = "Mozilla/5.0...",
                    DataCreazione = DateTime.Now.AddHours(-3),
                    Dettagli = "Accesso gestore"
                },
                new LogAccessi
                {
                    LogId = 2,
                    UtenteId = 2,
                    ClienteId = null,
                    TipoAccesso = "Login",
                    Esito = "Fallito",
                    IpAddress = "192.168.1.101",
                    UserAgent = "Mozilla/5.0...",
                    DataCreazione = DateTime.Now.AddHours(-2),
                    Dettagli = "Password errata"
                },
                new LogAccessi
                {
                    LogId = 3,
                    UtenteId = null,
                    ClienteId = 1,
                    TipoAccesso = "Accesso QR",
                    Esito = "Successo",
                    IpAddress = "192.168.1.102",
                    UserAgent = "Mobile Safari",
                    DataCreazione = DateTime.Now.AddHours(-1),
                    Dettagli = "Accesso cliente tramite QR"
                },
                new LogAccessi
                {
                    LogId = 4,
                    UtenteId = 1,
                    ClienteId = null,
                    TipoAccesso = "Modifica Ordine",
                    Esito = "Successo",
                    IpAddress = "192.168.1.100",
                    UserAgent = "Mozilla/5.0...",
                    DataCreazione = DateTime.Now.AddMinutes(-30),
                    Dettagli = "Ordine #123 modificato"
                },
                new LogAccessi
                {
                    LogId = 5,
                    UtenteId = null,
                    ClienteId = 2,
                    TipoAccesso = "Accesso QR",
                    Esito = "Fallito",
                    IpAddress = "192.168.1.103",
                    UserAgent = "Mobile Chrome",
                    DataCreazione = DateTime.Now.AddMinutes(-15),
                    Dettagli = "QR code scaduto"
                }
            };

            _context.Tavolo.AddRange(tavoli);
            _context.Cliente.AddRange(clienti);
            _context.Utenti.AddRange(utenti);
            _context.LogAccessi.AddRange(logAccessi);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLogAccessi()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLogAccessi()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.LogId);
            Assert.Equal(1, result.UtenteId);
            Assert.Equal("Login", result.TipoAccesso);
            Assert.Equal("Successo", result.Esito);
            Assert.Equal("192.168.1.100", result.IpAddress);
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
        public async Task GetByUtenteIdAsync_ShouldReturnFilteredLogAccessi()
        {
            // Act
            var result = await _repository.GetByUtenteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l => Assert.Equal(1, l.UtenteId));
        }

        [Fact]
        public async Task GetByClienteIdAsync_ShouldReturnFilteredLogAccessi()
        {
            // Act
            var result = await _repository.GetByClienteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, l => Assert.Equal(1, l.ClienteId));
        }

        [Fact]
        public async Task GetByTipoAccessoAsync_ShouldReturnFilteredLogAccessi()
        {
            // Act
            var result = await _repository.GetByTipoAccessoAsync("Login");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, l => Assert.Equal("Login", l.TipoAccesso));
        }

        [Fact]
        public async Task GetByEsitoAsync_ShouldReturnFilteredLogAccessi()
        {
            // Act
            var result = await _repository.GetByEsitoAsync("Successo");

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, l => Assert.Equal("Successo", l.Esito));
        }

        [Fact]
        public async Task GetByPeriodoAsync_ShouldReturnLogAccessiInPeriod()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-2.5);
            var dataFine = DateTime.Now.AddMinutes(-20);

            // Act
            var result = await _repository.GetByPeriodoAsync(dataInizio, dataFine);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, l =>
            {
                Assert.True(l.DataCreazione >= dataInizio);
                Assert.True(l.DataCreazione <= dataFine);
            });
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewLogAccessi()
        {
            // Arrange
            var newLog = new LogAccessiDTO
            {
                UtenteId = 2,
                ClienteId = null,
                TipoAccesso = "Logout",
                Esito = "Successo",
                IpAddress = "192.168.1.104",
                UserAgent = "Mozilla/5.0...",
                Dettagli = "Logout utente"
            };

            // Act
            await _repository.AddAsync(newLog);

            // Assert
            Assert.True(newLog.LogId > 0);
            var result = await _repository.GetByIdAsync(newLog.LogId);
            Assert.NotNull(result);
            Assert.Equal("Logout", result.TipoAccesso);
            Assert.Equal("Successo", result.Esito);
            Assert.Equal("192.168.1.104", result.IpAddress);
            Assert.NotNull(result.DataCreazione);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingLogAccessi()
        {
            // Arrange
            var updateDto = new LogAccessiDTO
            {
                LogId = 1,
                UtenteId = 1,
                ClienteId = null,
                TipoAccesso = "Login Modificato",
                Esito = "Successo",
                IpAddress = "192.168.1.200",
                UserAgent = "Chrome/91.0...",
                Dettagli = "Accesso gestore - MODIFICATO"
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Login Modificato", result.TipoAccesso);
            Assert.Equal("192.168.1.200", result.IpAddress);
            Assert.Equal("Chrome/91.0...", result.UserAgent);
            Assert.Equal("Accesso gestore - MODIFICATO", result.Dettagli);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveLogAccessi()
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
        public async Task GetNumeroAccessiAsync_ShouldReturnTotalCount()
        {
            // Act
            var result = await _repository.GetNumeroAccessiAsync();

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public async Task GetNumeroAccessiAsync_WithPeriod_ShouldReturnFilteredCount()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-2.5);
            var dataFine = DateTime.Now.AddMinutes(-20);

            // Act
            var result = await _repository.GetNumeroAccessiAsync(dataInizio, dataFine);

            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingLogAccessi_ShouldThrowException()
        {
            // Arrange
            var updateDto = new LogAccessiDTO
            {
                LogId = 999,
                UtenteId = 1,
                ClienteId = null,
                TipoAccesso = "Test",
                Esito = "Successo",
                IpAddress = "192.168.1.100",
                Dettagli = "Test"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}