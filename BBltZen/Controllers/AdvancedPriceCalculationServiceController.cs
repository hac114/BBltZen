// BBltZen/Controllers/AdvancedPriceCalculationServiceController.cs
using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class AdvancedPriceCalculationServiceController : SecureBaseController
    {
        private readonly IAdvancedPriceCalculationServiceRepository _repository;

        public AdvancedPriceCalculationServiceController(
            IAdvancedPriceCalculationServiceRepository repository,
            IWebHostEnvironment environment,
            ILogger<AdvancedPriceCalculationServiceController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet("bevanda-standard/{articoloId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateBevandaStandardPrice(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<decimal>("ID articolo non valido");

                var result = await _repository.CalculateBevandaStandardPriceAsync(articoloId);

                LogAuditTrail("CALCULATE_BEVANDA_STANDARD", "AdvancedPriceCalculation", articoloId.ToString());
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Articolo non trovato: {ArticoloId}", articoloId);
                return SafeNotFound<decimal>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda standard {ArticoloId}", articoloId);
                return SafeInternalError("Errore nel calcolo del prezzo");
            }
        }

        [HttpGet("bevanda-custom/{personalizzazioneCustomId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateBevandaCustomPrice(int personalizzazioneCustomId)
        {
            try
            {
                if (personalizzazioneCustomId <= 0)
                    return SafeBadRequest<decimal>("ID personalizzazione non valido");

                var result = await _repository.CalculateBevandaCustomPriceAsync(personalizzazioneCustomId);

                LogAuditTrail("CALCULATE_BEVANDA_CUSTOM", "AdvancedPriceCalculation", personalizzazioneCustomId.ToString());
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Personalizzazione non trovata: {PersonalizzazioneCustomId}", personalizzazioneCustomId);
                return SafeNotFound<decimal>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo bevanda custom {PersonalizzazioneCustomId}", personalizzazioneCustomId);
                return SafeInternalError("Errore nel calcolo del prezzo");
            }
        }

        [HttpGet("dolce/{articoloId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<decimal>> CalculateDolcePrice(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<decimal>("ID articolo non valido");

                var result = await _repository.CalculateDolcePriceAsync(articoloId);

                LogAuditTrail("CALCULATE_DOLCE", "AdvancedPriceCalculation", articoloId.ToString());
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dolce non trovato: {ArticoloId}", articoloId);
                return SafeNotFound<decimal>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo prezzo dolce {ArticoloId}", articoloId);
                return SafeInternalError("Errore nel calcolo del prezzo");
            }
        }

        [HttpPost("calcolo-completo")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<PriceCalculationResultDTO>> CalculateCompletePrice([FromBody] PriceCalculationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<PriceCalculationResultDTO>("Dati calcolo non validi");

                var result = await _repository.CalculateCompletePriceAsync(request);

                LogAuditTrail("CALCULATE_COMPLETE_PRICE", "AdvancedPriceCalculation", $"{request.TipoArticolo}_{request.ArticoloId}");
                LogSecurityEvent("CompletePriceCalculated", new
                {
                    ArticoloId = request.ArticoloId,
                    TipoArticolo = request.TipoArticolo,
                    Quantita = request.Quantita,
                    PrezzoFinale = result.TotaleIvato,
                    User = User.Identity?.Name ?? "Anonymous"
                });

                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Dati calcolo non validi per articolo {ArticoloId}", request.ArticoloId);
                return SafeBadRequest<PriceCalculationResultDTO>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo completo prezzo per articolo {ArticoloId}", request.ArticoloId);
                return SafeInternalError("Errore nel calcolo del prezzo");
            }
        }

        [HttpGet("bevanda-custom-dettagliata/{personalizzazioneCustomId}")]
        [AllowAnonymous] // ✅ CALCOLO PREZZI PUBBLICO
        public async Task<ActionResult<CustomBeverageCalculationDTO>> CalculateDetailedCustomBeveragePrice(int personalizzazioneCustomId)
        {
            try
            {
                if (personalizzazioneCustomId <= 0)
                    return SafeBadRequest<CustomBeverageCalculationDTO>("ID personalizzazione non valido");

                var result = await _repository.CalculateDetailedCustomBeveragePriceAsync(personalizzazioneCustomId);

                LogAuditTrail("CALCULATE_DETAILED_CUSTOM_BEVERAGE", "AdvancedPriceCalculation", personalizzazioneCustomId.ToString());
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Personalizzazione non trovata: {PersonalizzazioneCustomId}", personalizzazioneCustomId);
                return SafeNotFound<CustomBeverageCalculationDTO>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo dettagliato bevanda custom {PersonalizzazioneCustomId}", personalizzazioneCustomId);
                return SafeInternalError("Errore nel calcolo del prezzo");
            }
        }

        [HttpGet("ordine/{ordineId}")]
        //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<OrderCalculationSummaryDTO>> CalculateCompleteOrder(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<OrderCalculationSummaryDTO>("ID ordine non valido");

                var result = await _repository.CalculateCompleteOrderAsync(ordineId);

                LogAuditTrail("CALCULATE_COMPLETE_ORDER", "AdvancedPriceCalculation", ordineId.ToString());
                LogSecurityEvent("CompleteOrderCalculated", new
                {
                    OrdineId = ordineId,
                    TotaleOrdine = result.TotaleOrdine,
                    User = User.Identity?.Name
                });

                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ordine non trovato: {OrdineId}", ordineId);
                return SafeNotFound<OrderCalculationSummaryDTO>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo completo ordine {OrdineId}", ordineId);
                return SafeInternalError("Errore nel calcolo dell'ordine");
            }
        }

        [HttpPost("calcolo-batch")]
        [AllowAnonymous] // ✅ CALCOLO BATCH PUBBLICO
        public async Task<ActionResult<List<PriceCalculationResultDTO>>> CalculateBatchPrices([FromBody] List<PriceCalculationRequestDTO> requests)
        {
            try
            {
                if (requests == null || requests.Count == 0)
                    return SafeBadRequest<List<PriceCalculationResultDTO>>("Nessuna richiesta di calcolo"); // ✅ CORRETTO

                var results = await _repository.CalculateBatchPricesAsync(requests);

                LogAuditTrail("CALCULATE_BATCH_PRICES", "AdvancedPriceCalculation", $"Count:{requests.Count}");
                return Ok(results);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel calcolo batch per {Count} richieste", requests?.Count);
                return SafeInternalError("Errore nel calcolo batch");
            }
        }

        [HttpPost("sconto")]
        [AllowAnonymous] // ✅ CALCOLO SCONTO PUBBLICO
        public async Task<ActionResult<decimal>> ApplyDiscount([FromBody] DiscountRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<decimal>("Dati sconto non validi");

                var result = await _repository.ApplyDiscountAsync(request.Prezzo, request.PercentualeSconto);

                LogAuditTrail("APPLY_DISCOUNT", "AdvancedPriceCalculation", $"{request.PercentualeSconto}%");
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Percentuale sconto non valida: {Percentuale}", request.PercentualeSconto);
                return SafeBadRequest<decimal>(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'applicazione sconto {Percentuale}%", request.PercentualeSconto);
                return SafeInternalError("Errore nell'applicazione sconto");
            }
        }

        [HttpGet("iva/{taxRateId}")]
        [AllowAnonymous] // ✅ INFORMAZIONI IVA PUBBLICHE
        public async Task<ActionResult<decimal>> GetTaxRate(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SafeBadRequest<decimal>("ID aliquota IVA non valido");

                var result = await _repository.GetTaxRateAsync(taxRateId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero aliquota IVA {TaxRateId}", taxRateId);
                return SafeInternalError("Errore nel recupero aliquota IVA");
            }
        }

        [HttpPost("validazione-prezzo")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<bool>> ValidatePriceCalculation([FromBody] ValidationRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<bool>("Dati validazione non validi");

                var result = await _repository.ValidatePriceCalculationAsync(request.ArticoloId, request.TipoArticolo, request.PrezzoCalcolato);

                LogAuditTrail("VALIDATE_PRICE_CALCULATION", "AdvancedPriceCalculation", $"{request.TipoArticolo}_{request.ArticoloId}");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella validazione prezzo per articolo {ArticoloId}", request.ArticoloId);
                return SafeInternalError("Errore nella validazione prezzo");
            }
        }

        [HttpPost("precarica-cache")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> PreloadCache()
        {
            try
            {
                await _repository.PreloadCalculationCacheAsync();

                LogAuditTrail("PRELOAD_CALCULATION_CACHE", "AdvancedPriceCalculation", "All");
                LogSecurityEvent("CalculationCachePreloaded", new
                {
                    User = User.Identity?.Name,
                    Timestamp = System.DateTime.UtcNow
                });

                if (_environment.IsDevelopment())
                    return Ok("Cache precaricata con successo");
                else
                    return Ok("Operazione completata");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento cache calcoli");
                return SafeInternalError("Errore nel precaricamento cache");
            }
        }

        [HttpDelete("pulisci-cache")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> ClearCache()
        {
            try
            {
                await _repository.ClearCalculationCacheAsync();

                LogAuditTrail("CLEAR_CALCULATION_CACHE", "AdvancedPriceCalculation", "All");
                LogSecurityEvent("CalculationCacheCleared", new
                {
                    User = User.Identity?.Name,
                    Timestamp = System.DateTime.UtcNow
                });

                if (_environment.IsDevelopment())
                    return Ok("Cache pulita con successo");
                else
                    return Ok("Operazione completata");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia cache calcoli");
                return SafeInternalError("Errore nella pulizia cache");
            }
        }

        [HttpGet("cache-valida")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<bool>> IsCacheValid()
        {
            try
            {
                var result = await _repository.IsCacheValidAsync();
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella verifica validità cache");
                return SafeInternalError("Errore nella verifica cache");
            }
        }
    }
}