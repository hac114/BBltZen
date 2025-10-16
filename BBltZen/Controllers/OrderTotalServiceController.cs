using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderTotalServiceController : ControllerBase
    {
        private readonly IOrderTotalServiceRepository _orderTotalService;
        private readonly ILogger<OrderTotalServiceController> _logger;

        public OrderTotalServiceController(
            IOrderTotalServiceRepository orderTotalService,
            ILogger<OrderTotalServiceController> logger)
        {
            _orderTotalService = orderTotalService;
            _logger = logger;
        }

        [HttpGet("calculate/{orderId}")]
        public async Task<ActionResult<OrderTotalDTO>> CalculateOrderTotal(int orderId)
        {
            try
            {
                _logger.LogInformation($"Calcolo totale ordine: {orderId}");
                var result = await _orderTotalService.CalculateOrderTotalAsync(orderId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Ordine non trovato: {orderId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore calcolo totale ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("update/{orderId}")]
        public async Task<ActionResult<OrderUpdateTotalDTO>> UpdateOrderTotal(int orderId)
        {
            try
            {
                _logger.LogInformation($"Aggiornamento totale ordine: {orderId}");
                var result = await _orderTotalService.UpdateOrderTotalAsync(orderId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Ordine non trovato per aggiornamento: {orderId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore aggiornamento totale ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("item-tax/{orderItemId}")]
        public async Task<ActionResult<decimal>> CalculateItemTax(int orderItemId)
        {
            try
            {
                var tax = await _orderTotalService.CalculateItemTaxAsync(orderItemId);
                return Ok(tax);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"OrderItem non trovato: {orderItemId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore calcolo IVA item: {orderItemId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("tax-rate/{taxRateId}")]
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                var taxRate = await _orderTotalService.GetTaxRateAsync(taxRateId);
                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero aliquota IVA: {taxRateId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("validate/{orderId}")]
        public async Task<ActionResult<bool>> ValidateOrderForCalculation(int orderId)
        {
            try
            {
                var isValid = await _orderTotalService.ValidateOrderForCalculationAsync(orderId);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore validazione ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("recalculate/{orderId}")]
        public async Task<ActionResult<decimal>> RecalculateOrderTotalFromScratch(int orderId)
        {
            try
            {
                var total = await _orderTotalService.RecalculateOrderTotalFromScratchAsync(orderId);
                return Ok(total);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Ordine non trovato: {orderId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore ricalcolo totale ordine: {orderId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("invalid-totals")]
        public async Task<ActionResult<List<int>>> GetOrdersWithInvalidTotals()
        {
            try
            {
                var orders = await _orderTotalService.GetOrdersWithInvalidTotalsAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca ordini con totali non validi");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }
}