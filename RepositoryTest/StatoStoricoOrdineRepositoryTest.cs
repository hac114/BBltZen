using Database.Models;
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
    public class StatoStoricoOrdineRepositoryTest : BaseTest
    {
        private readonly StatoStoricoOrdineRepository _repository;        

        public StatoStoricoOrdineRepositoryTest()
        {            
            _repository = new StatoStoricoOrdineRepository(_context);

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
                }
            };

            var statiOrdine = new List<StatoOrdine>
            {
                new StatoOrdine
                {
                    StatoOrdineId = 1,
                    StatoOrdine1 = "In Attesa",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 2,
                    StatoOrdine1 = "In Preparazione",
                    Terminale = false
                },
                new StatoOrdine
                {
                    StatoOrdineId = 3,
                    StatoOrdine1 = "Completato",
                    Terminale = true
                }
            };

            var ordini = new List<Ordine>
            {
                new Ordine
                {
                    OrdineId = 1,
                    ClienteId = 1,
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    StatoOrdineId = 1,
                    StatoPagamentoId = 1,
                    Totale = 16.81m,
                    Priorita = 1
                },
                new Ordine
                {
                    OrdineId = 2,
                    ClienteId = 1,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    StatoOrdineId = 2,
                    StatoPagamentoId = 1,
                    Totale = 25.50m,
                    Priorita = 1
                }
            };

            var storicoOrdini = new List<StatoStoricoOrdine>
            {
                new StatoStoricoOrdine
                {
                    StatoStoricoOrdineId = 1,
                    OrdineId = 1,
                    StatoOrdineId = 1,
                    Inizio = DateTime.Now.AddHours(-3),
                    Fine = DateTime.Now.AddHours(-2)
                },
                new StatoStoricoOrdine
                {
                    StatoStoricoOrdineId = 2,
                    OrdineId = 1,
                    StatoOrdineId = 2,
                    Inizio = DateTime.Now.AddHours(-2),
                    Fine = DateTime.Now.AddHours(-1)
                },
                new StatoStoricoOrdine
                {
                    StatoStoricoOrdineId = 3,
                    OrdineId = 1,
                    StatoOrdineId = 3,
                    Inizio = DateTime.Now.AddHours(-1),
                    Fine = null // Stato attuale
                },
                new StatoStoricoOrdine
                {
                    StatoStoricoOrdineId = 4,
                    OrdineId = 2,
                    StatoOrdineId = 1,
                    Inizio = DateTime.Now.AddHours(-1),
                    Fine = null // Stato attuale
                }
            };

            _context.Tavolo.AddRange(tavoli);
            _context.Cliente.AddRange(clienti);
            _context.StatoOrdine.AddRange(statiOrdine);
            _context.Ordine.AddRange(ordini);
            _context.StatoStoricoOrdine.AddRange(storicoOrdini);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStatiStoricoOrdine()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnStatoStoricoOrdine()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.StatoStoricoOrdineId);
            Assert.Equal(1, result.OrdineId);
            Assert.Equal(1, result.StatoOrdineId);
            Assert.NotNull(result.Fine);
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
        public async Task GetByOrdineIdAsync_ShouldReturnFilteredStatiStoricoOrdine()
        {
            // Act
            var result = await _repository.GetByOrdineIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, s => Assert.Equal(1, s.OrdineId));
        }

        [Fact]
        public async Task GetByStatoOrdineIdAsync_ShouldReturnFilteredStatiStoricoOrdine()
        {
            // Act
            var result = await _repository.GetByStatoOrdineIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, s => Assert.Equal(1, s.StatoOrdineId));
        }

        [Fact]
        public async Task GetStoricoCompletoOrdineAsync_ShouldReturnCompleteHistory()
        {
            // Act
            var result = await _repository.GetStoricoCompletoOrdineAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.All(resultList, s => Assert.Equal(1, s.OrdineId));
        }

        [Fact]
        public async Task GetStatoAttualeOrdineAsync_ShouldReturnCurrentState()
        {
            // Act
            var result = await _repository.GetStatoAttualeOrdineAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrdineId);
            Assert.Equal(3, result.StatoOrdineId);
            Assert.Null(result.Fine); // Stato attuale non ha Fine
        }

        [Fact]
        public async Task GetStatoAttualeOrdineAsync_WithNoCurrentState_ShouldReturnNull()
        {
            // Arrange - Chiudi tutti gli stati
            await _repository.ChiudiStatoAttualeAsync(1, DateTime.Now);

            // Act
            var result = await _repository.GetStatoAttualeOrdineAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewStatoStoricoOrdine()
        {
            // Arrange
            var newStatoStorico = new StatoStoricoOrdineDTO
            {
                OrdineId = 2,
                StatoOrdineId = 2,
                Inizio = DateTime.Now,
                Fine = null
            };

            // Act
            var result = await _repository.AddAsync(newStatoStorico); // ✅ CORREGGI: assegna risultato

            // Assert
            Assert.True(result.StatoStoricoOrdineId > 0); // ✅ VERIFICA sul risultato
            Assert.Equal(2, result.OrdineId);
            Assert.Equal(2, result.StatoOrdineId);
            Assert.Null(result.Fine);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingStatoStoricoOrdine()
        {
            // Arrange
            var updateDto = new StatoStoricoOrdineDTO
            {
                StatoStoricoOrdineId = 1,
                OrdineId = 1,
                StatoOrdineId = 2, // Stato modificato
                Inizio = DateTime.Now.AddHours(-4), // Inizio modificato
                Fine = DateTime.Now.AddHours(-3) // Fine modificata
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(2, result.StatoOrdineId);
            Assert.NotNull(result.Fine);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveStatoStoricoOrdine()
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
        public async Task ChiudiStatoAttualeAsync_ShouldCloseCurrentState()
        {
            // Arrange
            var fine = DateTime.Now;

            // Act
            var result = await _repository.ChiudiStatoAttualeAsync(1, fine);

            // Assert
            Assert.True(result);
            var statoAttuale = await _repository.GetStatoAttualeOrdineAsync(1);
            Assert.Null(statoAttuale); // Non dovrebbe più esserci uno stato attuale
        }

        [Fact]
        public async Task ChiudiStatoAttualeAsync_WithNoCurrentState_ShouldReturnFalse()
        {
            // Arrange - Ordine che non esiste
            var fine = DateTime.Now;

            // Act
            var result = await _repository.ChiudiStatoAttualeAsync(999, fine);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddAsync_ShouldSetCorrectTimestamps()
        {
            // Arrange
            var newStatoStorico = new StatoStoricoOrdineDTO
            {
                OrdineId = 2,
                StatoOrdineId = 2,
                Inizio = DateTime.Now,
                Fine = null
            };

            // Act
            var result = await _repository.AddAsync(newStatoStorico);
            
            // ✅ VERIFICA DIRETTAMENTE IL RANGE
            Assert.InRange(result.Inizio, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
        }

        [Fact]
        public async Task AddAsync_WithInvalidDates_ShouldThrowException()
        {
            // Arrange
            var invalidStatoStorico = new StatoStoricoOrdineDTO
            {
                OrdineId = 2,
                StatoOrdineId = 2,
                Inizio = DateTime.Now,
                Fine = DateTime.Now.AddHours(-1) // Fine precedente a inizio
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddAsync(invalidStatoStorico));
        }

        [Fact]
        public async Task OrdineHasStatoAsync_ShouldReturnCorrectValue()
        {
            // Act & Assert
            Assert.True(await _repository.OrdineHasStatoAsync(1, 1)); // Ordine 1 ha stato 1
            Assert.True(await _repository.OrdineHasStatoAsync(1, 2)); // Ordine 1 ha stato 2
            Assert.True(await _repository.OrdineHasStatoAsync(1, 3)); // Ordine 1 ha stato 3
            Assert.False(await _repository.OrdineHasStatoAsync(1, 4)); // Ordine 1 non ha stato 4
        }

        [Fact]
        public async Task GetDataInizioStatoAsync_ShouldReturnCorrectDate()
        {
            // Act
            var result = await _repository.GetDataInizioStatoAsync(1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.InRange(result.Value, DateTime.Now.AddHours(-4), DateTime.Now.AddHours(-2));
        }

        [Fact]
        public async Task GetNumeroStatiByOrdineAsync_ShouldReturnCorrectCount()
        {
            // Act
            var ordine1Count = await _repository.GetNumeroStatiByOrdineAsync(1);
            var ordine2Count = await _repository.GetNumeroStatiByOrdineAsync(2);

            // Assert
            Assert.Equal(3, ordine1Count); // 3 stati per ordine 1
            Assert.Equal(1, ordine2Count); // 1 stato per ordine 2
        }

        [Fact]
        public async Task GetStoricoByPeriodoAsync_ShouldReturnFilteredStorico()
        {
            // Arrange
            var dataInizio = DateTime.Now.AddHours(-2.5);
            var dataFine = DateTime.Now.AddMinutes(-30); // -0.5h

            // Act
            var result = await _repository.GetStoricoByPeriodoAsync(dataInizio, dataFine);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count); // ✅ CORREGGI: 3 invece di 2
            Assert.All(resultList, s =>
            {
                Assert.True(s.Inizio >= dataInizio);
                Assert.True(s.Fine == null || s.Fine <= dataFine);
            });
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingStatoStorico_ShouldNotThrow()
        {
            // Arrange
            var updateDto = new StatoStoricoOrdineDTO
            {
                StatoStoricoOrdineId = 999,
                OrdineId = 1,
                StatoOrdineId = 2,
                Inizio = DateTime.Now,
                Fine = null
            };

            // Act & Assert - ✅ CORREGGI: Non dovrebbe lanciare eccezione
            var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(updateDto));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ChiudiStatoAttualeAsync_ShouldSetCorrectFineDate()
        {
            // Arrange
            var fine = DateTime.Now;

            // Act
            var result = await _repository.ChiudiStatoAttualeAsync(1, fine);

            // Assert
            Assert.True(result);
            var storicoChiuso = await _context.StatoStoricoOrdine
                .FirstOrDefaultAsync(s => s.OrdineId == 1 && s.Fine == fine);
            Assert.NotNull(storicoChiuso);
            Assert.Equal(fine, storicoChiuso.Fine);
        }
    }
}