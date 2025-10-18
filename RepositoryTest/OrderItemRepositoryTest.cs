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
    public class OrderItemRepositoryTest : BaseTest
    {
        private readonly OrderItemRepository _repository;
        private readonly BubbleTeaContext _context;

        public OrderItemRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<BubbleTeaContext>()
                .UseInMemoryDatabase(databaseName: $"OrderItemTest_{Guid.NewGuid()}")
                .Options;

            _context = new BubbleTeaContext(options);
            _repository = new OrderItemRepository(_context);

            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Crea dati necessari per i test
            var taxRates = new List<TaxRates>
            {
                new TaxRates
                {
                    TaxRateId = 1,
                    Aliquota = 22.00m,
                    Descrizione = "IVA Standard",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new TaxRates
                {
                    TaxRateId = 2,
                    Aliquota = 10.00m,
                    Descrizione = "IVA Ridotta",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                }
            };

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

            var articoli = new List<Articolo>
            {
                new Articolo
                {
                    ArticoloId = 1,
                    Tipo = "BS",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new Articolo
                {
                    ArticoloId = 2,
                    Tipo = "BC",
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now
                },
                new Articolo
                {
                    ArticoloId = 3,
                    Tipo = "DOLCE",
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
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    StatoOrdineId = 1,
                    StatoPagamentoId = 1,
                    Totale = 16.81m,
                    Priorita = null
                },
                new Ordine
                {
                    OrdineId = 2,
                    ClienteId = 1,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    StatoOrdineId = 1,
                    StatoPagamentoId = 1,
                    Totale = 16.10m,
                    Priorita = null
                }
            };

            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderItemId = 1,
                    OrdineId = 1,
                    ArticoloId = 1,
                    Quantita = 2,
                    PrezzoUnitario = 5.00m,
                    ScontoApplicato = 0.50m,
                    Imponibile = 9.50m,
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    TipoArticolo = "Bevanda Standard",
                    TotaleIvato = 11.59m,
                    TaxRateId = 1
                },
                new OrderItem
                {
                    OrderItemId = 2,
                    OrdineId = 1,
                    ArticoloId = 2,
                    Quantita = 1,
                    PrezzoUnitario = 6.00m,
                    ScontoApplicato = 0m,
                    Imponibile = 6.00m,
                    DataCreazione = DateTime.Now.AddDays(-1),
                    DataAggiornamento = DateTime.Now.AddDays(-1),
                    TipoArticolo = "Bevanda Custom",
                    TotaleIvato = 7.32m,
                    TaxRateId = 1
                },
                new OrderItem
                {
                    OrderItemId = 3,
                    OrdineId = 2,
                    ArticoloId = 3,
                    Quantita = 3,
                    PrezzoUnitario = 4.00m,
                    ScontoApplicato = 1.00m,
                    Imponibile = 11.00m,
                    DataCreazione = DateTime.Now,
                    DataAggiornamento = DateTime.Now,
                    TipoArticolo = "Dolce",
                    TotaleIvato = 13.42m,
                    TaxRateId = 2
                }
            };

            _context.TaxRates.AddRange(taxRates);
            _context.Tavolo.AddRange(tavoli);
            _context.Cliente.AddRange(clienti);
            _context.Articolo.AddRange(articoli);
            _context.Ordine.AddRange(ordini);
            _context.OrderItem.AddRange(orderItems);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllOrderItems()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnOrderItem()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrderItemId);
            Assert.Equal(1, result.OrdineId);
            Assert.Equal(1, result.ArticoloId);
            Assert.Equal("Bevanda Standard", result.TipoArticolo);
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
        public async Task GetByOrderIdAsync_ShouldReturnFilteredOrderItems()
        {
            // Act
            var result = await _repository.GetByOrderIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, oi => Assert.Equal(1, oi.OrdineId));
        }

        [Fact]
        public async Task GetByOrderIdAsync_WithInvalidOrderId_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByOrderIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByArticoloIdAsync_ShouldReturnFilteredOrderItems()
        {
            // Act
            var result = await _repository.GetByArticoloIdAsync(1);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.All(resultList, oi => Assert.Equal(1, oi.ArticoloId));
        }

        [Fact]
        public async Task GetByArticoloIdAsync_WithInvalidArticoloId_ShouldReturnEmpty()
        {
            // Act
            var result = await _repository.GetByArticoloIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewOrderItem()
        {
            // Arrange
            var newOrderItem = new OrderItemDTO
            {
                OrdineId = 2,
                ArticoloId = 2,
                Quantita = 2,
                PrezzoUnitario = 5.50m,
                ScontoApplicato = 0.25m,
                Imponibile = 10.75m,
                TipoArticolo = "Bevanda Custom",
                TotaleIvato = 13.12m,
                TaxRateId = 1
            };

            // Act
            await _repository.AddAsync(newOrderItem);

            // Assert
            Assert.True(newOrderItem.OrderItemId > 0);
            var result = await _repository.GetByIdAsync(newOrderItem.OrderItemId);
            Assert.NotNull(result);
            Assert.Equal(2, result.OrdineId);
            Assert.Equal(2, result.ArticoloId);
            Assert.Equal(2, result.Quantita);
            Assert.Equal("Bevanda Custom", result.TipoArticolo);
            Assert.NotNull(result.DataCreazione);
            Assert.NotNull(result.DataAggiornamento);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingOrderItem()
        {
            // Arrange
            var updateDto = new OrderItemDTO
            {
                OrderItemId = 1,
                OrdineId = 1,
                ArticoloId = 1,
                Quantita = 5, // Quantità modificata
                PrezzoUnitario = 5.00m,
                ScontoApplicato = 1.00m, // Sconto modificato
                Imponibile = 24.00m,
                TipoArticolo = "Bevanda Standard Modificata",
                TotaleIvato = 29.28m,
                TaxRateId = 1
            };

            // Act
            await _repository.UpdateAsync(updateDto);

            // Assert
            var result = await _repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(5, result.Quantita);
            Assert.Equal(1.00m, result.ScontoApplicato);
            Assert.Equal("Bevanda Standard Modificata", result.TipoArticolo);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveOrderItem()
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
        public async Task UpdateAsync_WithNonExistingOrderItem_ShouldThrowException()
        {
            // Arrange
            var updateDto = new OrderItemDTO
            {
                OrderItemId = 999,
                OrdineId = 1,
                ArticoloId = 1,
                Quantita = 1,
                PrezzoUnitario = 5.00m,
                ScontoApplicato = 0m,
                Imponibile = 5.00m,
                TipoArticolo = "Test",
                TotaleIvato = 6.10m,
                TaxRateId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(updateDto));
        }
    }
}