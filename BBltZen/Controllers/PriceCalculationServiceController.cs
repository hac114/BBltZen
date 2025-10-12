using Database;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceCalculationServiceController : ControllerBase
    {
        private readonly IPriceCalculationServiceRepository _priceCalculationService;

        public PriceCalculationServiceController(IPriceCalculationServiceRepository priceCalculationService)
        {
            _priceCalculationService = priceCalculationService;
        }

        [HttpPost("bevanda-standard/{bevandaStandardId}")]
        public async Task<ActionResult<decimal>> CalculateBevandaStandard(int bevandaStandardId)
        {
            try
            {
                var prezzo = await _priceCalculationService.CalculateBevandaStandardPrice(bevandaStandardId);
                return Ok(prezzo);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo: {ex.Message}");
            }
        }

        [HttpPost("bevanda-custom/{personalizzazioneCustomId}")]
        public async Task<ActionResult<decimal>> CalculateBevandaCustom(int personalizzazioneCustomId)
        {
            try
            {
                var prezzo = await _priceCalculationService.CalculateBevandaCustomPrice(personalizzazioneCustomId);
                return Ok(prezzo);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo: {ex.Message}");
            }
        }

        [HttpPost("dolce/{dolceId}")]
        public async Task<ActionResult<decimal>> CalculateDolce(int dolceId)
        {
            try
            {
                var prezzo = await _priceCalculationService.CalculateDolcePrice(dolceId);
                return Ok(prezzo);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo: {ex.Message}");
            }
        }

        [HttpPost("order-item")]
        public async Task<ActionResult<PriceCalculationServiceDTO>> CalculateOrderItem([FromBody] OrderItemDTO orderItemDto)
        {
            try
            {
                // Converti OrderItemDTO in OrderItem (entity) per il servizio
                var orderItem = new OrderItem
                {
                    OrderItemId = orderItemDto.OrderItemId,
                    OrdineId = orderItemDto.OrdineId,
                    ArticoloId = orderItemDto.ArticoloId,
                    Quantita = orderItemDto.Quantita,
                    PrezzoUnitario = orderItemDto.PrezzoUnitario,
                    ScontoApplicato = orderItemDto.ScontoApplicato,
                    Imponibile = orderItemDto.Imponibile,
                    DataCreazione = orderItemDto.DataCreazione,
                    DataAggiornamento = orderItemDto.DataAggiornamento,
                    TipoArticolo = orderItemDto.TipoArticolo,
                    TotaleIvato = orderItemDto.TotaleIvato,
                    TaxRateId = orderItemDto.TaxRateId
                };

                var result = await _priceCalculationService.CalculateOrderItemPrice(orderItem);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo: {ex.Message}");
            }
        }

        [HttpPost("tax-amount")]
        public async Task<ActionResult<decimal>> CalculateTaxAmount([FromBody] TaxCalculationRequest request)
        {
            try
            {
                var taxAmount = await _priceCalculationService.CalculateTaxAmount(request.Imponibile, request.TaxRateId);
                return Ok(taxAmount);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo IVA: {ex.Message}");
            }
        }

        [HttpPost("imponibile")]
        public async Task<ActionResult<decimal>> CalculateImponibile([FromBody] ImponibileCalculationRequest request)
        {
            try
            {
                var imponibile = await _priceCalculationService.CalculateImponibile(request.Prezzo, request.Quantita, request.TaxRateId);
                return Ok(imponibile);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel calcolo imponibile: {ex.Message}");
            }
        }

        [HttpGet("tax-rate/{taxRateId}")]
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                var taxRate = await _priceCalculationService.GetTaxRate(taxRateId);
                return Ok(taxRate);
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel recupero aliquota: {ex.Message}");
            }
        }

        [HttpPost("clear-cache")]
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                await _priceCalculationService.ClearCache();
                return Ok("Cache del servizio di calcolo prezzi pulita con successo");
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nella pulizia cache: {ex.Message}");
            }
        }

        [HttpPost("preload-cache")]
        public async Task<ActionResult> PreloadCache()
        {
            try
            {
                await _priceCalculationService.PreloadCache();
                return Ok("Precaricamento cache completato con successo");
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Errore nel precaricamento cache: {ex.Message}");
            }
        }
    }

    // DTO per le richieste di calcolo
    public class TaxCalculationRequest
    {
        public decimal Imponibile { get; set; }
        public int TaxRateId { get; set; }
    }

    public class ImponibileCalculationRequest
    {
        public decimal Prezzo { get; set; }
        public int Quantita { get; set; }
        public int TaxRateId { get; set; }
    }
}