using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedPriceCalculationServiceController : ControllerBase
    {
        private readonly IAdvancedPriceCalculationServiceRepository _priceCalculationService;

        public AdvancedPriceCalculationServiceController(IAdvancedPriceCalculationServiceRepository priceCalculationService)
        {
            _priceCalculationService = priceCalculationService;
        }

        [HttpGet("bevanda-standard/{articoloId}")]
        public async Task<ActionResult<decimal>> CalculateBevandaStandardPrice(int articoloId)
        {
            try
            {
                var result = await _priceCalculationService.CalculateBevandaStandardPriceAsync(articoloId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("bevanda-custom/{personalizzazioneCustomId}")]
        public async Task<ActionResult<decimal>> CalculateBevandaCustomPrice(int personalizzazioneCustomId)
        {
            try
            {
                var result = await _priceCalculationService.CalculateBevandaCustomPriceAsync(personalizzazioneCustomId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("dolce/{articoloId}")]
        public async Task<ActionResult<decimal>> CalculateDolcePrice(int articoloId)
        {
            try
            {
                var result = await _priceCalculationService.CalculateDolcePriceAsync(articoloId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("calcolo-completo")]
        public async Task<ActionResult<PriceCalculationResultDTO>> CalculateCompletePrice([FromBody] PriceCalculationRequestDTO request)
        {
            try
            {
                var result = await _priceCalculationService.CalculateCompletePriceAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("bevanda-custom-dettagliata/{personalizzazioneCustomId}")]
        public async Task<ActionResult<CustomBeverageCalculationDTO>> CalculateDetailedCustomBeveragePrice(int personalizzazioneCustomId)
        {
            try
            {
                var result = await _priceCalculationService.CalculateDetailedCustomBeveragePriceAsync(personalizzazioneCustomId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("ordine/{ordineId}")]
        public async Task<ActionResult<OrderCalculationSummaryDTO>> CalculateCompleteOrder(int ordineId)
        {
            try
            {
                var result = await _priceCalculationService.CalculateCompleteOrderAsync(ordineId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("calcolo-batch")]
        public async Task<ActionResult<List<PriceCalculationResultDTO>>> CalculateBatchPrices([FromBody] List<PriceCalculationRequestDTO> requests)
        {
            try
            {
                var results = await _priceCalculationService.CalculateBatchPricesAsync(requests);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("sconto")]
        public async Task<ActionResult<decimal>> ApplyDiscount([FromBody] DiscountRequestDTO request)
        {
            try
            {
                var result = await _priceCalculationService.ApplyDiscountAsync(request.Prezzo, request.PercentualeSconto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpGet("iva/{taxRateId}")]
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                var result = await _priceCalculationService.GetTaxRateAsync(taxRateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("validazione-prezzo")]
        public async Task<ActionResult<bool>> ValidatePriceCalculation([FromBody] ValidationRequestDTO request)
        {
            try
            {
                var result = await _priceCalculationService.ValidatePriceCalculationAsync(request.ArticoloId, request.TipoArticolo, request.PrezzoCalcolato);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        [HttpPost("precarica-cache")]
        public async Task<ActionResult> PreloadCache()
        {
            try
            {
                await _priceCalculationService.PreloadCalculationCacheAsync();
                return Ok("Cache precaricata con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore nel precaricamento cache: {ex.Message}");
            }
        }

        [HttpDelete("pulisci-cache")]
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                await _priceCalculationService.ClearCalculationCacheAsync();
                return Ok("Cache pulita con successo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore nella pulizia cache: {ex.Message}");
            }
        }

        [HttpGet("cache-valida")]
        public async Task<ActionResult<bool>> IsCacheValid()
        {
            try
            {
                var result = await _priceCalculationService.IsCacheValidAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore nella verifica cache: {ex.Message}");
            }
        }
    }

    // DTO aggiuntivi per le richieste specifiche del controller
    public class DiscountRequestDTO
    {
        public decimal Prezzo { get; set; }
        public decimal PercentualeSconto { get; set; }
    }

    public class ValidationRequestDTO
    {
        public int ArticoloId { get; set; }
        public string TipoArticolo { get; set; } = string.Empty;
        public decimal PrezzoCalcolato { get; set; }
    }
}