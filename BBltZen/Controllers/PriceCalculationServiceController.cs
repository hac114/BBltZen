// BBltZen/Controllers/PriceCalculationServiceController.cs
using Database;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class PriceCalculationServiceController : SecureBaseController
    {
        private readonly IPriceCalculationServiceRepository _repository;

        public PriceCalculationServiceController(
            IPriceCalculationServiceRepository repository,
            IWebHostEnvironment environment,
            ILogger<PriceCalculationServiceController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpPost("bevanda-standard/{bevandaStandardId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateBevandaStandard(int bevandaStandardId)
        {
            try
            {
                if (bevandaStandardId <= 0)
                    return SafeBadRequest<decimal>("ID bevanda standard non valido");

                var prezzo = await _repository.CalculateBevandaStandardPrice(bevandaStandardId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_BEVANDA_STANDARD_PRICE", "PriceCalculation", bevandaStandardId.ToString());

                return Ok(prezzo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda standard {BevandaStandardId}", bevandaStandardId);
                return SafeBadRequest<decimal>("Errore nel calcolo del prezzo");
            }
        }

        [HttpPost("bevanda-custom/{personalizzazioneCustomId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateBevandaCustom(int personalizzazioneCustomId)
        {
            try
            {
                if (personalizzazioneCustomId <= 0)
                    return SafeBadRequest<decimal>("ID personalizzazione custom non valido");

                var prezzo = await _repository.CalculateBevandaCustomPrice(personalizzazioneCustomId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_BEVANDA_CUSTOM_PRICE", "PriceCalculation", personalizzazioneCustomId.ToString());

                return Ok(prezzo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda custom {PersonalizzazioneCustomId}", personalizzazioneCustomId);
                return SafeBadRequest<decimal>("Errore nel calcolo del prezzo");
            }
        }

        [HttpPost("dolce/{dolceId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateDolce(int dolceId)
        {
            try
            {
                if (dolceId <= 0)
                    return SafeBadRequest<decimal>("ID dolce non valido");

                var prezzo = await _repository.CalculateDolcePrice(dolceId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_DOLCE_PRICE", "PriceCalculation", dolceId.ToString());

                return Ok(prezzo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo dolce {DolceId}", dolceId);
                return SafeBadRequest<decimal>("Errore nel calcolo del prezzo");
            }
        }

        [HttpPost("order-item")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<PriceCalculationServiceDTO>> CalculateOrderItem([FromBody] OrderItemDTO orderItemDto)
        {
            try
            {
                if (!IsModelValid(orderItemDto))
                    return SafeBadRequest<PriceCalculationServiceDTO>("Dati order item non validi");

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

                var result = await _repository.CalculateOrderItemPrice(orderItem);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_ORDER_ITEM_PRICE", "PriceCalculation", orderItemDto.OrderItemId.ToString());
                LogSecurityEvent("OrderItemPriceCalculated", new
                {
                    orderItemDto.OrderItemId,           // ✅ NOME MEMBRO SEMPLIFICATO
                    orderItemDto.TipoArticolo,          // ✅ NOME MEMBRO SEMPLIFICATO
                    orderItemDto.Quantita,              // ✅ NOME MEMBRO SEMPLIFICATO
                    PrezzoFinale = result.TotaleIvato,  // ✅ MANTIENI NOME ESPLICITO
                    User = GetCurrentUserIdOrDefault()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo order item {OrderItemId}", orderItemDto?.OrderItemId);
                return SafeBadRequest<PriceCalculationServiceDTO>("Errore nel calcolo del prezzo");
            }
        }

        [HttpPost("tax-amount")]
        [AllowAnonymous] // ✅ CALCOLO FISCALE PUBBLICO
        public async Task<ActionResult<decimal>> CalculateTaxAmount([FromBody] TaxCalculationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<decimal>("Dati calcolo IVA non validi");

                if (request.Imponibile < 0)
                    return SafeBadRequest<decimal>("Imponibile non valido");

                if (request.TaxRateId <= 0)
                    return SafeBadRequest<decimal>("ID aliquota IVA non valido");

                var taxAmount = await _repository.CalculateTaxAmount(request.Imponibile, request.TaxRateId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_TAX_AMOUNT", "PriceCalculation", $"{request.TaxRateId}_{request.Imponibile}");

                return Ok(taxAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo IVA per imponibile {Imponibile} e tax rate {TaxRateId}", request.Imponibile, request.TaxRateId);
                return SafeBadRequest<decimal>("Errore nel calcolo IVA");
            }
        }

        [HttpPost("imponibile")]
        [AllowAnonymous] // ✅ CALCOLO FISCALE PUBBLICO
        public async Task<ActionResult<decimal>> CalculateImponibile([FromBody] ImponibileCalculationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<decimal>("Dati calcolo imponibile non validi");

                if (request.Prezzo < 0)
                    return SafeBadRequest<decimal>("Prezzo non valido");

                if (request.Quantita <= 0)
                    return SafeBadRequest<decimal>("Quantità non valida");

                if (request.TaxRateId <= 0)
                    return SafeBadRequest<decimal>("ID aliquota IVA non valido");

                var imponibile = await _repository.CalculateImponibile(request.Prezzo, request.Quantita, request.TaxRateId);

                // ✅ Log per audit
                LogAuditTrail("CALCULATE_IMPONIBILE", "PriceCalculation", $"{request.TaxRateId}_{request.Prezzo}x{request.Quantita}");

                return Ok(imponibile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo imponibile per prezzo {Prezzo}, quantità {Quantita} e tax rate {TaxRateId}",
                    request.Prezzo, request.Quantita, request.TaxRateId);
                return SafeBadRequest<decimal>("Errore nel calcolo imponibile");
            }
        }

        [HttpGet("tax-rate/{taxRateId}")]
        [AllowAnonymous] // ✅ INFORMAZIONI IVA PUBBLICHE
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SafeBadRequest<decimal>("ID aliquota IVA non valido");

                var taxRate = await _repository.GetTaxRate(taxRateId);
                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero aliquota IVA {TaxRateId}", taxRateId);
                return SafeBadRequest<decimal>("Errore nel recupero aliquota IVA");
            }
        }

        [HttpPost("clear-cache")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                await _repository.ClearCache();

                // ✅ Log per audit
                LogAuditTrail("CLEAR_PRICE_CACHE", "PriceCalculation", "All");
                LogSecurityEvent("PriceCacheCleared", new
                {
                    User = GetCurrentUserIdOrDefault(),
                    Timestamp = DateTime.UtcNow
                });

                return Ok(_environment.IsDevelopment()
                    ? "Cache del servizio di calcolo prezzi pulita con successo"
                    : "Operazione completata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia cache prezzi");
                return SafeInternalError("Errore nella pulizia cache");
            }
        }

        [HttpPost("preload-cache")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> PreloadCache()
        {
            try
            {
                await _repository.PreloadCache();

                // ✅ Log per audit
                LogAuditTrail("PRELOAD_PRICE_CACHE", "PriceCalculation", "All");
                LogSecurityEvent("PriceCachePreloaded", new
                {
                    User = GetCurrentUserIdOrDefault(),
                    Timestamp = DateTime.UtcNow
                });

                return Ok(_environment.IsDevelopment()
                    ? "Precaricamento cache completato con successo"
                    : "Operazione completata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento cache prezzi");
                return SafeInternalError("Errore nel precaricamento cache");
            }
        }

        // ✅ Endpoint aggiuntivo per calcolo batch
        [HttpPost("batch-calculation")]
        [AllowAnonymous] // ✅ CALCOLO BATCH PUBBLICO
        public async Task<ActionResult<BatchCalculationResponseDTO>> BatchCalculation([FromBody] BatchCalculationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<BatchCalculationResponseDTO>("Dati calcolo batch non validi");

                // ✅ CORREZIONE: USA IL METODO DEL REPOSITORY INVECE DI LOGICA NEL CONTROLLER
                var response = await _repository.CalculateBatchPricesAsync(request);

                // ✅ Log per audit
                LogAuditTrail("BATCH_PRICE_CALCULATION", "PriceCalculation",
                    $"Standard:{request.BevandeStandardIds.Count}, Custom:{request.BevandeCustomIds.Count}, Dolci:{request.DolciIds.Count}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo batch prezzi");
                return SafeInternalError<BatchCalculationResponseDTO>("Errore nel calcolo batch prezzi");
            }
        }

        [HttpGet("validate-tax-rate/{taxRateId}")]
        [AllowAnonymous] // ✅ VALIDAZIONE PUBBLICA
        public async Task<ActionResult<bool>> ValidateTaxRate(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SafeBadRequest<bool>("ID aliquota IVA non valido");

                var isValid = await _repository.ValidateTaxRate(taxRateId);

                LogAuditTrail("VALIDATE_TAX_RATE", "PriceCalculation", taxRateId.ToString());

                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella validazione tax rate {TaxRateId}", taxRateId);
                return SafeBadRequest<bool>("Errore nella validazione aliquota IVA");
            }
        }

        [HttpGet("health")]
        [AllowAnonymous] // ✅ HEALTH CHECK PUBBLICO
        public async Task<ActionResult<object>> HealthCheck()
        {
            try
            {
                // ✅ TESTA FUNZIONALITÀ CRITICHE
                var taxRate = await _repository.GetTaxRate(1); // Aliquota standard
                var isValid = await _repository.ValidateTaxRate(1);

                var health = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    TaxRateService = taxRate > 0 ? "Operational" : "Degraded",
                    ValidationService = isValid ? "Operational" : "Degraded",
                    CacheStatus = "Enabled"
                };

                LogAuditTrail("PRICE_CALCULATION_HEALTH_CHECK", "PriceCalculation", "OK");

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check fallito per PriceCalculationService");

                return Ok(new
                {
                    Status = "Unhealthy",
                    Error = _environment.IsDevelopment() ? ex.Message : "Service unavailable",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}