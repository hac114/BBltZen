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
    public class OrdineRepositoryTest : BaseTest
    {
        private readonly OrdineRepository _repository;
        private readonly BubbleTeaContext _context;

        public OrdineRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"OrdineTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new OrdineRepository(_context);

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

            // ✅ AGGIUNTA SESSIONE ID PER TESTARE LE MODIFICHE
            var sessioneId1 = Guid.NewGuid();
            var sessioneId2 = Guid.NewGuid();

            var ordini = new List<Ordine>
            {
                new Ordine
                {
                    OrdineId = 1,
                    ClienteId = 1,
                    DataCreazione = DateTime.Now.AddDays(-2),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    StatoOrdineId = 1,
                    StatoPagamentoId = 1,
                    Totale = 16.81m,
                    Priorita = 1,
                    SessioneId = sessioneId1 // ✅ ORDINE CON SESSIONE
                },
                new Ordine
                {
                    OrdineId = 2,
                    ClienteId = 1,
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now,
                    StatoOrdineId = 2,
                    StatoPagamentoId = 2,
                    Totale = 25.50m,
                    Priorita = 1,
                    SessioneId = null // ✅ ORDINE SENZA SESSIONE
                },
                new Ordine
                {
                    OrdineId = 3,
                    ClienteId = 2,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    StatoOrdineId = 1,
                    StatoPagamentoId = 1,
                    Totale = 12.30m,
                    Priorita = 1,
                    SessioneId = sessioneId2 // ✅ ORDINE CON SESSIONE
                }
            };

            _context.Tavolo.AddRange(tavoli);
            _context.Cliente.AddRange(clienti);
            _context.Ordine.AddRange(ordini);
            _context.SaveChanges();
        }

        // ✅ TEST ESISTENTI ADEGUATI PER VERIFICARE SESSIONE ID

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllOrdiniWithSessioneId()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());

            // ✅ VERIFICA SESSIONE ID IN TUTTI I RISULTATI
            var resultList = result.ToList();
            Assert.NotNull(resultList[0].SessioneId); // Ordine 1 ha sessione
            Assert.Null(resultList[1].SessioneId);    // Ordine 2 non ha sessione  
            Assert.NotNull(resultList[2].SessioneId); // Ordine 3 ha sessione
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnOrdineWithSessioneId()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrdineId);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(16.81m, result.Totale);
            Assert.Equal(1, result.StatoOrdineId);
            Assert.NotNull(result.SessioneId); // ✅ VERIFICA SESSIONE ID
        }

        [Fact]
        public async Task GetByIdAsync_WithValidIdNoSessione_ShouldReturnOrdineWithNullSessioneId()
        {
            // Act
            var result = await _repository.GetByIdAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.OrdineId);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(25.50m, result.Totale);
            Assert.Equal(2, result.StatoOrdineId);
            Assert.Null(result.SessioneId); // ✅ VERIFICA SESSIONE ID NULL
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
        public async Task GetByClienteIdAsync_ShouldReturnFilteredOrdiniWithSessioneId()
        {
            // Act
            var result = await _repository.GetByClienteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.ClienteId));

            // ✅ VERIFICA SESSIONE ID NEI RISULTATI FILTRATI
            var ordineConSessione = resultList.First(o => o.OrdineId == 1);
            var ordineSenzaSessione = resultList.First(o => o.OrdineId == 2);
            Assert.NotNull(ordineConSessione.SessioneId);
            Assert.Null(ordineSenzaSessione.SessioneId);
        }

        [Fact]
        public async Task GetByStatoOrdineIdAsync_ShouldReturnFilteredOrdiniWithSessioneId()
        {
            // Act
            var result = await _repository.GetByStatoOrdineIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.StatoOrdineId));

            // ✅ VERIFICA SESSIONE ID NEI RISULTATI FILTRATI
            Assert.All(resultList, o => Assert.NotNull(o.SessioneId)); // Entrambi hanno sessione
        }

        [Fact]
        public async Task GetByStatoPagamentoIdAsync_ShouldReturnFilteredOrdiniWithSessioneId()
        {
            // Act
            var result = await _repository.GetByStatoPagamentoIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.StatoPagamentoId));

            // ✅ VERIFICA SESSIONE ID NEI RISULTATI FILTRATI
            Assert.All(resultList, o => Assert.NotNull(o.SessioneId)); // Entrambi hanno sessione
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewOrdineWithSessioneId()
        {
            // Arrange
            var newOrdine = new OrdineDTO
            {
                ClienteId = 2,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 18.75m,
                Priorita = 1,
                SessioneId = Guid.NewGuid() // ✅ SESSIONE ID
            };

            // Act
            var result = await _repository.AddAsync(newOrdine);

            // Assert
            Assert.True(result.OrdineId > 0);
            var savedOrdine = await _repository.GetByIdAsync(result.OrdineId);
            Assert.NotNull(savedOrdine);
            Assert.Equal(2, savedOrdine.ClienteId);
            Assert.Equal(18.75m, savedOrdine.Totale);
            Assert.NotNull(savedOrdine.DataCreazione);
            Assert.NotNull(savedOrdine.DataAggiornamento);
            Assert.Equal(newOrdine.SessioneId, savedOrdine.SessioneId); // ✅ VERIFICA SESSIONE ID
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewOrdineWithoutSessioneId()
        {
            // Arrange
            var newOrdine = new OrdineDTO
            {
                ClienteId = 2,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 18.75m,
                Priorita = 1,
                SessioneId = null // ✅ SESSIONE ID NULL
            };

            // Act
            var result = await _repository.AddAsync(newOrdine);

            // Assert
            Assert.True(result.OrdineId > 0);
            var savedOrdine = await _repository.GetByIdAsync(result.OrdineId);
            Assert.NotNull(savedOrdine);
            Assert.Equal(2, savedOrdine.ClienteId);
            Assert.Equal(18.75m, savedOrdine.Totale);
            Assert.NotNull(savedOrdine.DataCreazione);
            Assert.NotNull(savedOrdine.DataAggiornamento);
            Assert.Null(savedOrdine.SessioneId); // ✅ VERIFICA SESSIONE ID NULL
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingOrdineWithSessioneId()
        {
            // Arrange
            var newSessioneId = Guid.NewGuid();
            var updateDto = new OrdineDTO
            {
                OrdineId = 1,
                ClienteId = 1,
                StatoOrdineId = 3, // Stato modificato
                StatoPagamentoId = 3, // Stato pagamento modificato
                Totale = 20.00m, // Totale modificato
                Priorita = 2, // Priorità modificata
                SessioneId = newSessioneId // ✅ SESSIONE ID MODIFICATO
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(3, result.StatoOrdineId);
            Assert.Equal(3, result.StatoPagamentoId);
            Assert.Equal(20.00m, result.Totale);
            Assert.Equal(2, result.Priorita);
            Assert.Equal(newSessioneId, result.SessioneId); // ✅ VERIFICA SESSIONE ID AGGIORNATO
        }

        [Fact]
        public async Task UpdateAsync_ShouldRemoveSessioneIdFromOrdine()
        {
            // Arrange
            var updateDto = new OrdineDTO
            {
                OrdineId = 1, // Ordine che ha SessioneId
                ClienteId = 1,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 16.81m,
                Priorita = 1,
                SessioneId = null // ✅ RIMUOVI SESSIONE ID
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Null(result.SessioneId); // ✅ VERIFICA SESSIONE ID RIMOSSO
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveOrdine()
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
        public async Task UpdateAsync_WithNonExistingOrdine_ShouldThrowException()
        {
            // Arrange
            var updateDto = new OrdineDTO
            {
                OrdineId = 999,
                ClienteId = 1,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 10.00m,
                Priorita = 1,
                SessioneId = Guid.NewGuid() // ✅ SESSIONE ID
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(updateDto));
        }

        // ✅ NUOVI TEST PER I METODI AGGIUNTI IN OrdineRepository.cs

        [Fact]
        public async Task GetBySessioneIdAsync_ShouldReturnFilteredOrdini()
        {
            // Arrange - Ottieni una sessioneId esistente
            var ordineConSessione = await _repository.GetByIdAsync(1);
            var sessioneId = ordineConSessione.SessioneId;
            Assert.NotNull(sessioneId);

            // Act
            var result = await _repository.GetBySessioneIdAsync(sessioneId.Value);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, o => Assert.Equal(sessioneId, o.SessioneId));
        }

        [Fact]
        public async Task GetBySessioneIdAsync_WithNonExistingSessione_ShouldReturnEmpty()
        {
            // Arrange
            var nonExistingSessioneId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBySessioneIdAsync(nonExistingSessioneId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrdiniConSessioneAsync_ShouldReturnOnlyOrdiniWithSessione()
        {
            // Act
            var result = await _repository.GetOrdiniConSessioneAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count); // Ordini 1 e 3 hanno sessione
            Assert.All(resultList, o => Assert.NotNull(o.SessioneId));
        }

        [Fact]
        public async Task GetOrdiniSenzaSessioneAsync_ShouldReturnOnlyOrdiniWithoutSessione()
        {
            // Act
            var result = await _repository.GetOrdiniSenzaSessioneAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList); // Solo ordine 2 non ha sessione
            Assert.All(resultList, o => Assert.Null(o.SessioneId));
        }
    }
}