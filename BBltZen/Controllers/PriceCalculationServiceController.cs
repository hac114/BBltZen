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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
                    OrderItemId = orderItemDto.OrderItemId,
                    TipoArticolo = orderItemDto.TipoArticolo,
                    Quantita = orderItemDto.Quantita,
                    PrezzoFinale = result.TotaleIvato,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return Ok(result);
            }
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
                    User = User.Identity?.Name,
                    Timestamp = System.DateTime.UtcNow
                });

                if (_environment.IsDevelopment())
                    return Ok("Cache del servizio di calcolo prezzi pulita con successo");
                else
                    return Ok("Operazione completata");
            }
            catch (System.Exception ex)
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
                    User = User.Identity?.Name,
                    Timestamp = System.DateTime.UtcNow
                });

                if (_environment.IsDevelopment())
                    return Ok("Precaricamento cache completato con successo");
                else
                    return Ok("Operazione completata");
            }
            catch (System.Exception ex)
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

                var response = new BatchCalculationResponseDTO();

                // Calcola prezzi bevande standard
                foreach (var bevandaId in request.BevandeStandardIds)
                {
                    if (bevandaId > 0)
                    {
                        try
                        {
                            var prezzo = await _repository.CalculateBevandaStandardPrice(bevandaId);
                            response.BevandeStandardPrezzi.Add(bevandaId, prezzo);
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogWarning(ex, "Errore nel calcolo batch bevanda standard {BevandaId}", bevandaId);
                            response.Errori.Add($"Bevanda standard {bevandaId}: {ex.Message}");
                        }
                    }
                }

                // Calcola prezzi bevande custom
                foreach (var customId in request.BevandeCustomIds)
                {
                    if (customId > 0)
                    {
                        try
                        {
                            var prezzo = await _repository.CalculateBevandaCustomPrice(customId);
                            response.BevandeCustomPrezzi.Add(customId, prezzo);
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogWarning(ex, "Errore nel calcolo batch bevanda custom {CustomId}", customId);
                            response.Errori.Add($"Bevanda custom {customId}: {ex.Message}");
                        }
                    }
                }

                // Calcola prezzi dolci
                foreach (var dolceId in request.DolciIds)
                {
                    if (dolceId > 0)
                    {
                        try
                        {
                            var prezzo = await _repository.CalculateDolcePrice(dolceId);
                            response.DolciPrezzi.Add(dolceId, prezzo);
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogWarning(ex, "Errore nel calcolo batch dolce {DolceId}", dolceId);
                            response.Errori.Add($"Dolce {dolceId}: {ex.Message}");
                        }
                    }
                }

                // ✅ Log per audit
                LogAuditTrail("BATCH_PRICE_CALCULATION", "PriceCalculation",
                    $"Standard:{request.BevandeStandardIds.Count}, Custom:{request.BevandeCustomIds.Count}, Dolci:{request.DolciIds.Count}");

                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo batch prezzi");
                return SafeInternalError("Errore nel calcolo batch prezzi");
            }
        }
    }
}