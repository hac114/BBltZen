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
                    Priorita = null
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
                    Priorita = 1
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
                    Priorita = null
                }
            };

            _context.Tavolo.AddRange(tavoli);
            _context.Cliente.AddRange(clienti);
            _context.Ordine.AddRange(ordini);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllOrdini()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnOrdine()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrdineId);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal(16.81m, result.Totale);
            Assert.Equal(1, result.StatoOrdineId);
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
        public async Task GetByClienteIdAsync_ShouldReturnFilteredOrdini()
        {
            // Act
            var result = await _repository.GetByClienteIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.ClienteId));
        }

        [Fact]
        public async Task GetByStatoOrdineIdAsync_ShouldReturnFilteredOrdini()
        {
            // Act
            var result = await _repository.GetByStatoOrdineIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.StatoOrdineId));
        }

        [Fact]
        public async Task GetByStatoPagamentoIdAsync_ShouldReturnFilteredOrdini()
        {
            // Act
            var result = await _repository.GetByStatoPagamentoIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, o => Assert.Equal(1, o.StatoPagamentoId));
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewOrdine()
        {
            // Arrange
            var newOrdine = new OrdineDTO
            {
                ClienteId = 2,
                StatoOrdineId = 1,
                StatoPagamentoId = 1,
                Totale = 18.75m,
                Priorita = null
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
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingOrdine()
        {
            // Arrange
            var updateDto = new OrdineDTO
            {
                OrdineId = 1,
                ClienteId = 1,
                StatoOrdineId = 3, // Stato modificato
                StatoPagamentoId = 3, // Stato pagamento modificato
                Totale = 20.00m, // Totale modificato
                Priorita = 2 // Priorità modificata
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
                Priorita = null
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}
