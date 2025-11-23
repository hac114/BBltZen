using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class OrderTotalServiceRepository : IOrderTotalServiceRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly ILogger<OrderTotalServiceRepository> _logger;

        public OrderTotalServiceRepository(
            BubbleTeaContext context,
            ILogger<OrderTotalServiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<OrderTotalDTO> MapToOrderTotalDTO(int orderId, List<OrderItem> orderItems)
        {
            var result = new OrderTotalDTO
            {
                OrderId = orderId,
                DataCalcolo = DateTime.UtcNow
            };

            foreach (var item in orderItems)
            {
                var itemTotal = await CalculateOrderItemTotalAsync(item);
                result.Items.Add(itemTotal);

                result.SubTotale += itemTotal.Imponibile;
                result.TotaleIVA += (itemTotal.TotaleIVATO - itemTotal.Imponibile);
            }

            result.TotaleGenerale = result.SubTotale + result.TotaleIVA;
            return result;
        }

        private OrderUpdateTotalDTO MapToOrderUpdateTotalDTO(int orderId, decimal oldTotal, decimal newTotal)
        {
            return new OrderUpdateTotalDTO
            {
                OrderId = orderId,
                VecchioTotale = oldTotal,
                NuovoTotale = newTotal,
                Differenza = newTotal - oldTotal,
                DataAggiornamento = DateTime.UtcNow
            };
        }

        private OrderItemTotalDTO MapToOrderItemTotalDTO(OrderItem item, decimal taxRate, decimal imponibile, decimal totaleIvato)
        {
            return new OrderItemTotalDTO
            {
                OrderItemId = item.OrderItemId,
                ArticoloId = item.ArticoloId,
                TipoArticolo = item.TipoArticolo,
                Quantita = item.Quantita,
                PrezzoUnitario = item.PrezzoUnitario,
                ScontoApplicato = item.ScontoApplicato,
                Imponibile = Math.Round(imponibile, 2),
                TotaleIVATO = Math.Round(totaleIvato, 2),
                AliquotaIVA = taxRate
            };
        }

        public async Task<OrderTotalDTO> CalculateOrderTotalAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Calcolo totale ordine: {OrderId}", orderId);

                if (!await ValidateOrderForCalculationAsync(orderId))
                    throw new ArgumentException($"Ordine non valido per il calcolo: {orderId}");

                var order = await _context.Ordine.FindAsync(orderId);
                if (order == null)
                    throw new ArgumentException($"Ordine non trovato: {orderId}");

                var orderItems = await _context.OrderItem
                    .Where(oi => oi.OrdineId == orderId)
                    .ToListAsync();

                // ✅ USA MapDTO INVECE DI COSTRUTTORE DIRETTO
                var result = await MapToOrderTotalDTO(orderId, orderItems);

                _logger.LogInformation("Ordine {OrderId}: SubTotale={SubTotale:C}, IVA={TotaleIVA:C}, Totale={TotaleGenerale:C}",
                    orderId, result.SubTotale, result.TotaleIVA, result.TotaleGenerale);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore calcolo totale ordine: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderUpdateTotalDTO> UpdateOrderTotalAsync(int orderId)
        {
            try
            {
                _logger.LogInformation("Aggiornamento totale ordine: {OrderId}", orderId);

                var order = await _context.Ordine.FindAsync(orderId);
                if (order == null)
                    throw new ArgumentException($"Ordine non trovato: {orderId}");

                var oldTotal = order.Totale;
                var calculation = await CalculateOrderTotalAsync(orderId);

                order.Totale = calculation.TotaleGenerale;
                order.DataAggiornamento = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // ✅ USA MapDTO INVECE DI COSTRUTTORE DIRETTO
                var result = MapToOrderUpdateTotalDTO(orderId, oldTotal, calculation.TotaleGenerale);

                _logger.LogInformation("Ordine {OrderId} aggiornato: {OldTotal:C} -> {NewTotal:C} (diff: {Difference:C})",
                    orderId, oldTotal, calculation.TotaleGenerale, result.Differenza);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento totale ordine: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<decimal> CalculateItemTaxAsync(int orderItemId)
        {
            try
            {
                var orderItem = await _context.OrderItem.FindAsync(orderItemId);
                if (orderItem == null)
                    throw new ArgumentException($"OrderItem non trovato: {orderItemId}");

                var taxRate = await GetTaxRateAsync(orderItem.TaxRateId);
                var imponibile = orderItem.PrezzoUnitario * orderItem.Quantita - orderItem.ScontoApplicato;
                var iva = imponibile * (taxRate / 100);

                return iva;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore calcolo IVA item: {orderItemId}");
                throw;
            }
        }

        public async Task<decimal> GetTaxRateAsync(int taxRateId)
        {
            try
            {
                var taxRate = await _context.TaxRates
                    .Where(tr => tr.TaxRateId == taxRateId)
                    .Select(tr => tr.Aliquota)
                    .FirstOrDefaultAsync();

                return taxRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero aliquota IVA: {taxRateId}");
                return 22.00m; // Default IVA standard
            }
        }

        public async Task<bool> ValidateOrderForCalculationAsync(int orderId)
        {
            try
            {
                var order = await _context.Ordine.FindAsync(orderId);
                if (order == null) return false;

                // Verifica che l'ordine non sia in uno stato finale
                var statiFinali = new[] { 4, 5 }; // Completato, Annullato

                // CORREZIONE: Rimuovi .HasValue e .Value
                if (statiFinali.Contains(order.StatoOrdineId)) // RIGO 155 CORRETTO
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore validazione ordine: {orderId}");
                return false;
            }
        }

        public async Task<decimal> RecalculateOrderTotalFromScratchAsync(int orderId)
        {
            try
            {
                _logger.LogInformation($"Ricalcolo totale da zero ordine: {orderId}");

                var calculation = await CalculateOrderTotalAsync(orderId);
                return calculation.TotaleGenerale;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore ricalcolo totale ordine: {orderId}");
                throw;
            }
        }

        public async Task<IEnumerable<int>> GetOrdersWithInvalidTotalsAsync()
        {
            try
            {
                var ordersWithIssues = new List<int>();

                var orders = await _context.Ordine
                    .Where(o => o.StatoOrdineId != 5)
                    .ToListAsync();

                foreach (var order in orders)
                {
                    var calculatedTotal = await RecalculateOrderTotalFromScratchAsync(order.OrdineId);
                    if (Math.Abs(order.Totale - calculatedTotal) > 0.01m)
                    {
                        ordersWithIssues.Add(order.OrdineId);
                    }
                }

                _logger.LogInformation("Trovati {Count} ordini con totali non validi", ordersWithIssues.Count);
                return ordersWithIssues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca ordini con totali non validi");
                return Enumerable.Empty<int>();
            }
        }

        private async Task<OrderItemTotalDTO> CalculateOrderItemTotalAsync(OrderItem item)
        {
            var taxRate = await GetTaxRateAsync(item.TaxRateId);
            var imponibile = (item.PrezzoUnitario * item.Quantita) - item.ScontoApplicato;
            var totaleIvato = imponibile * (1 + (taxRate / 100));

            // ✅ USA MapDTO INVECE DI COSTRUTTORE DIRETTO
            return MapToOrderItemTotalDTO(item, taxRate, imponibile, totaleIvato);
        }

        public async Task<bool> ExistsAsync(int orderId)
        {
            try
            {
                var exists = await _context.Ordine
                    .AnyAsync(o => o.OrdineId == orderId);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica esistenza ordine: {OrderId}", orderId);
                return false;
            }
        }
    }
}