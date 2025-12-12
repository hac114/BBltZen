using Database.Models;
using DTO;
using Microsoft.Extensions.Logging;
using Moq;
using Repository.Service;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepositoryTest
{
    public class OrderTotalServiceRepositoryTest : BaseTest
    {
        private readonly OrderTotalServiceRepository _service;
        private readonly Mock<ILogger<OrderTotalServiceRepository>> _loggerMock;

        public OrderTotalServiceRepositoryTest()
        {
            _loggerMock = new Mock<ILogger<OrderTotalServiceRepository>>();
            _service = new OrderTotalServiceRepository(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task CalculateOrderTotalAsync_ValidOrder_ReturnsCorrectTotal()
        {
            // Arrange
            var ordine = CreateTestOrdine();
            var taxRate = CreateTestTaxRate(22.00m);
            var articolo = CreateTestArticolo("BS");

            var orderItem = CreateTestOrderItem(ordine.OrdineId, articolo.ArticoloId, "BS", 2, 5.00m, 1.00m, taxRate.TaxRateId);

            // Act
            var result = await _service.CalculateOrderTotalAsync(ordine.OrdineId);

            // Assert
            Assert.Equal(ordine.OrdineId, result.OrderId);
            Assert.Single(result.Items);

            // Calcoli attesi:
            // Imponibile = (5.00 * 2) - 1.00 = 9.00
            // IVA = 9.00 * 0.22 = 1.98
            // Totale = 9.00 + 1.98 = 10.98
            Assert.Equal(9.00m, result.SubTotale);
            Assert.Equal(1.98m, result.TotaleIVA);
            Assert.Equal(10.98m, result.TotaleGenerale);

            // ✅ VERIFICA IL PRIMO ITEM
            var firstItem = result.Items.First();
            Assert.Equal(orderItem.OrderItemId, firstItem.OrderItemId);
            Assert.Equal(9.00m, firstItem.Imponibile);
            Assert.Equal(10.98m, firstItem.TotaleIVATO);
        }

        [Fact]
        public async Task UpdateOrderTotalAsync_UpdatesOrderTotal()
        {
            // Arrange
            var ordine = CreateTestOrdine();
            var taxRate = CreateTestTaxRate(10.00m);
            var articolo = CreateTestArticolo("D");

            var orderItem = CreateTestOrderItem(ordine.OrdineId, articolo.ArticoloId, "D", 3, 4.00m, 0.50m, taxRate.TaxRateId);

            // Act
            var result = await _service.UpdateOrderTotalAsync(ordine.OrdineId);

            // Assert
            Assert.Equal(ordine.OrdineId, result.OrderId);
            Assert.True(result.NuovoTotale > 0);

            // ✅ CORREZIONE: Gestione caso null
            // Verifica che l'ordine sia stato aggiornato nel database
            var updatedOrder = _context.Ordine.Find(ordine.OrdineId);
            Assert.NotNull(updatedOrder); // ✅ VERIFICA NON NULL
            Assert.Equal(result.NuovoTotale, updatedOrder!.Totale); // ✅ USA ! per dire al compilatore che non è null
        }

        [Fact]
        public async Task CalculateItemTaxAsync_CalculatesCorrectTax()
        {
            // Arrange
            var ordine = CreateTestOrdine();
            var taxRate = CreateTestTaxRate(22.00m);
            var articolo = CreateTestArticolo("BS");

            var orderItem = CreateTestOrderItem(ordine.OrdineId, articolo.ArticoloId, "BS", 2, 10.00m, 1.00m, taxRate.TaxRateId);

            // Act
            var tax = await _service.CalculateItemTaxAsync(orderItem.OrderItemId);

            // Assert
            // Imponibile = (10.00 * 2) - 1.00 = 19.00
            // IVA = 19.00 * 0.22 = 4.18
            Assert.Equal(4.18m, tax);
        }

        // Helper methods
        private Ordine CreateTestOrdine()
        {
            var ordine = new Ordine
            {
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                Totale = 0
            };
            _context.Ordine.Add(ordine);
            _context.SaveChanges();
            return ordine;
        }

        private TaxRates CreateTestTaxRate(decimal aliquota)
        {
            var taxRate = new TaxRates
            {
                Aliquota = aliquota,
                Descrizione = $"Test IVA {aliquota}%",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };
            _context.TaxRates.Add(taxRate);
            _context.SaveChanges();
            return taxRate;
        }

        private Articolo CreateTestArticolo(string tipo)
        {
            var articolo = new Articolo
            {
                Tipo = tipo,
                DataCreazione = DateTime.Now
            };
            _context.Articolo.Add(articolo);
            _context.SaveChanges();
            return articolo;
        }

        private OrderItem CreateTestOrderItem(int ordineId, int articoloId, string tipoArticolo, int quantita, decimal prezzoUnitario, decimal sconto, int taxRateId)
        {
            var orderItem = new OrderItem
            {
                OrdineId = ordineId,
                ArticoloId = articoloId,
                TipoArticolo = tipoArticolo,
                Quantita = quantita,
                PrezzoUnitario = prezzoUnitario,
                ScontoApplicato = sconto,
                Imponibile = (prezzoUnitario * quantita) - sconto,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                TaxRateId = taxRateId
            };
            _context.OrderItem.Add(orderItem);
            _context.SaveChanges();
            return orderItem;
        }

        [Fact]
        public async Task ExistsAsync_WithExistingOrder_ReturnsTrue()
        {
            // Arrange
            var ordine = CreateTestOrdine();

            // Act
            var result = await _service.ExistsAsync(ordine.OrdineId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingOrder_ReturnsFalse()
        {
            // Act
            var result = await _service.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetOrdersWithInvalidTotalsAsync_WithValidOrders_ReturnsEmpty()
        {
            // Arrange
            var ordine = CreateTestOrdine();
            var taxRate = CreateTestTaxRate(22.00m);
            var articolo = CreateTestArticolo("BS");

            var orderItem = CreateTestOrderItem(ordine.OrdineId, articolo.ArticoloId, "BS", 2, 5.00m, 1.00m, taxRate.TaxRateId);

            // Aggiorna il totale per renderlo valido
            await _service.UpdateOrderTotalAsync(ordine.OrdineId);

            // Act
            var result = await _service.GetOrdersWithInvalidTotalsAsync();

            // Assert
            Assert.Empty(result);
        }
    }
}