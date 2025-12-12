using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Database.Models;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class OrderTotalServiceController : SecureBaseController
    {
        private readonly IOrderTotalServiceRepository _orderTotalService;
        private readonly BubbleTeaContext _context;

        public OrderTotalServiceController(
            IOrderTotalServiceRepository orderTotalService,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<OrderTotalServiceController> logger)
            : base(environment, logger)
        {
            _orderTotalService = orderTotalService;
            _context = context;
        }

        [HttpGet("calculate/{orderId}")]
        [AllowAnonymous]
        public async Task<ActionResult<OrderTotalDTO>> CalculateOrderTotal(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<OrderTotalDTO>("ID ordine non valido");

                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound<OrderTotalDTO>("Ordine non trovato");

                _logger.LogInformation("Calcolo totale ordine: {OrderId}", orderId);
                var result = await _orderTotalService.CalculateOrderTotalAsync(orderId);

                LogAuditTrail("CALCULATE_ORDER_TOTAL", "OrderTotalService", orderId.ToString());
                LogSecurityEvent("OrderTotalCalculated", new
                {
                    orderId, // ✅ SEMPLIFICATO
                    result.SubTotale, // ✅ SEMPLIFICATO
                    result.TotaleIVA, // ✅ SEMPLIFICATO
                    result.TotaleGenerale, // ✅ SEMPLIFICATO
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Ordine non valido per calcolo: {OrderId}", orderId);
                return SafeBadRequest<OrderTotalDTO>(
                    _environment.IsDevelopment()
                        ? argEx.Message
                        : "Ordine non valido per il calcolo"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore calcolo totale ordine: {OrderId}", orderId);
                return SafeInternalError<OrderTotalDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore calcolo totale ordine {orderId}: {ex.Message}"
                        : "Errore interno nel calcolo totale ordine"
                );
            }
        }

        [HttpPost("update/{orderId}")]
        //[Authorize(Roles = "admin,manager,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<OrderUpdateTotalDTO>> UpdateOrderTotal(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<OrderUpdateTotalDTO>("ID ordine non valido");

                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound<OrderUpdateTotalDTO>("Ordine non trovato");

                _logger.LogInformation("Aggiornamento totale ordine: {OrderId}", orderId);
                var result = await _orderTotalService.UpdateOrderTotalAsync(orderId);

                LogAuditTrail("UPDATE_ORDER_TOTAL", "OrderTotalService", orderId.ToString());
                LogSecurityEvent("OrderTotalUpdated", new
                {
                    orderId, // ✅ SEMPLIFICATO
                    result.VecchioTotale, // ✅ SEMPLIFICATO
                    result.NuovoTotale, // ✅ SEMPLIFICATO
                    result.Differenza, // ✅ SEMPLIFICATO
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(result);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Ordine non trovato per aggiornamento: {OrderId}", orderId);
                return SafeNotFound<OrderUpdateTotalDTO>(
                    _environment.IsDevelopment()
                        ? argEx.Message
                        : "Ordine non trovato"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento totale ordine: {OrderId}", orderId);
                return SafeInternalError<OrderUpdateTotalDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore aggiornamento totale ordine {orderId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento totale ordine"
                );
            }
        }

        [HttpGet("item-tax/{orderItemId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<decimal>> CalculateItemTax(int orderItemId)
        {
            try
            {
                if (orderItemId <= 0)
                    return SafeBadRequest<decimal>("ID order item non valido");

                // ✅ Controllo esistenza order item con BubbleTeaContext
                var itemExists = await _context.OrderItem.AnyAsync(oi => oi.OrderItemId == orderItemId);
                if (!itemExists)
                    return SafeNotFound<decimal>("Order item non trovato");

                var tax = await _orderTotalService.CalculateItemTaxAsync(orderItemId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_ITEM_TAX", "OrderTotalService", orderItemId.ToString());

                return Ok(tax);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, $"OrderItem non trovato: {orderItemId}");
                return SafeNotFound<decimal>(
                    _environment.IsDevelopment()
                        ? argEx.Message
                        : "Order item non trovato"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore calcolo IVA item: {orderItemId}");
                return SafeInternalError<decimal>(
                    _environment.IsDevelopment()
                        ? $"Errore calcolo IVA item {orderItemId}: {ex.Message}"
                        : "Errore interno nel calcolo IVA item"
                );
            }
        }

        [HttpGet("tax-rate/{taxRateId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SafeBadRequest<decimal>("ID tax rate non valido");

                // ✅ Controllo esistenza tax rate con BubbleTeaContext
                var taxRateExists = await _context.TaxRates.AnyAsync(tr => tr.TaxRateId == taxRateId);
                if (!taxRateExists)
                    return SafeNotFound<decimal>("Tax rate non trovato");

                var taxRate = await _orderTotalService.GetTaxRateAsync(taxRateId);

                // ✅ Log per audit
                LogAuditTrail("GET_TAX_RATE", "OrderTotalService", taxRateId.ToString());

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero aliquota IVA: {taxRateId}");
                return SafeInternalError<decimal>(
                    _environment.IsDevelopment()
                        ? $"Errore recupero aliquota IVA {taxRateId}: {ex.Message}"
                        : "Errore interno nel recupero aliquota IVA"
                );
            }
        }

        [HttpGet("validate/{orderId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<bool>> ValidateOrderForCalculation(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<bool>("ID ordine non valido");

                // ✅ Controllo esistenza ordine con BubbleTeaContext
                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound<bool>("Ordine non trovato");

                var isValid = await _orderTotalService.ValidateOrderForCalculationAsync(orderId);

                // ✅ Log per audit
                LogAuditTrail("VALIDATE_ORDER_CALCULATION", "OrderTotalService", orderId.ToString());

                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore validazione ordine: {orderId}");
                return SafeInternalError<bool>(
                    _environment.IsDevelopment()
                        ? $"Errore validazione ordine {orderId}: {ex.Message}"
                        : "Errore interno nella validazione ordine"
                );
            }
        }

        [HttpGet("recalculate/{orderId}")]
        //[Authorize(Roles = "admin,manager,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<decimal>> RecalculateOrderTotalFromScratch(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<decimal>("ID ordine non valido");

                // ✅ Controllo esistenza ordine con BubbleTeaContext
                var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                if (!orderExists)
                    return SafeNotFound<decimal>("Ordine non trovato");

                var total = await _orderTotalService.RecalculateOrderTotalFromScratchAsync(orderId);

                // ✅ Log per audit
                LogAuditTrail("RECALCULATE_ORDER_TOTAL", "OrderTotalService", orderId.ToString());
                LogSecurityEvent("OrderTotalRecalculated", new
                {
                    OrderId = orderId,
                    NewTotal = total,
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(total);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, $"Ordine non trovato: {orderId}");
                return SafeNotFound<decimal>(
                    _environment.IsDevelopment()
                        ? argEx.Message
                        : "Ordine non trovato"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore ricalcolo totale ordine: {orderId}");
                return SafeInternalError<decimal>(
                    _environment.IsDevelopment()
                        ? $"Errore ricalcolo totale ordine {orderId}: {ex.Message}"
                        : "Errore interno nel ricalcolo totale ordine"
                );
            }
        }

        [HttpGet("invalid-totals")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<int>>> GetOrdersWithInvalidTotals()
        {
            try
            {
                var orders = await _orderTotalService.GetOrdersWithInvalidTotalsAsync();

                LogAuditTrail("GET_ORDERS_INVALID_TOTALS", "OrderTotalService", $"Count: {orders.Count()}");
                LogSecurityEvent("InvalidTotalsChecked", new
                {
                    Count = orders.Count(),
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca ordini con totali non validi");
                return SafeInternalError<IEnumerable<int>>(
                    _environment.IsDevelopment()
                        ? $"Errore ricerca ordini con totali non validi: {ex.Message}"
                        : "Errore interno nella ricerca ordini con totali non validi"
                );
            }
        }

        [HttpPost("bulk-recalculate")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<Dictionary<int, decimal>>> BulkRecalculateOrders([FromBody] List<int> orderIds)
        {
            try
            {
                if (orderIds == null || orderIds.Count == 0)
                    return SafeBadRequest<Dictionary<int, decimal>>("Lista ordini vuota");

                if (orderIds.Count > 100)
                    return SafeBadRequest<Dictionary<int, decimal>>("Troppi ordini per il ricalcolo di massa (max 100)");

                var results = new Dictionary<int, decimal>();

                foreach (var orderId in orderIds)
                {
                    try
                    {
                        // ✅ Controllo esistenza ordine
                        var orderExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderId);
                        if (!orderExists)
                        {
                            _logger.LogWarning($"Ordine {orderId} non trovato durante bulk recalculate");
                            continue;
                        }

                        var total = await _orderTotalService.RecalculateOrderTotalFromScratchAsync(orderId);
                        results[orderId] = total;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore ricalcolo ordine {orderId} durante bulk operation");
                        results[orderId] = -1; // Valore di errore
                    }
                }

                // ✅ Log per audit
                LogAuditTrail("BULK_RECALCULATE_ORDERS", "OrderTotalService", $"Processed: {orderIds.Count}, Successful: {results.Count(r => r.Value > 0)}");
                LogSecurityEvent("BulkRecalculationPerformed", new
                {
                    TotalOrders = orderIds.Count,
                    Successful = results.Count(r => r.Value > 0),
                    Failed = results.Count(r => r.Value <= 0),
                    User = User.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricalcolo di massa ordini");
                return SafeInternalError<Dictionary<int, decimal>>(
                    _environment.IsDevelopment()
                        ? $"Errore ricalcolo di massa ordini: {ex.Message}"
                        : "Errore interno nel ricalcolo di massa ordini"
                );
            }
        }

        [HttpGet("exists/{orderId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(int orderId)
        {
            try
            {
                if (orderId <= 0)
                    return SafeBadRequest<bool>("ID ordine non valido");

                var exists = await _orderTotalService.ExistsAsync(orderId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica esistenza ordine: {OrderId}", orderId);
                return SafeInternalError<bool>("Errore durante la verifica esistenza");
            }
        }
    }
}