using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class BeverageAvailabilityServiceController : SecureBaseController
    {
        private readonly IBeverageAvailabilityServiceRepository _repository;

        public BeverageAvailabilityServiceController(
            IBeverageAvailabilityServiceRepository repository,
            IWebHostEnvironment environment,
            ILogger<BeverageAvailabilityServiceController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet("check/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BeverageAvailabilityDTO>> CheckBeverageAvailability(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<BeverageAvailabilityDTO>("ID articolo non valido");

                var result = await _repository.CheckBeverageAvailabilityAsync(articoloId);

                LogAuditTrail("CHECK_BEVERAGE_AVAILABILITY", "BeverageAvailability", articoloId.ToString());

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Articolo non trovato: {ArticoloId}", articoloId);
                return SafeNotFound<BeverageAvailabilityDTO>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica disponibilità bevanda: {ArticoloId}", articoloId);
                return SafeInternalError<BeverageAvailabilityDTO>("Errore durante la verifica disponibilità bevanda");
            }
        }

        [HttpPost("check-multiple")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> CheckMultipleBeveragesAvailability([FromBody] List<int> articoliIds)
        {
            try
            {
                if (articoliIds == null || articoliIds.Count == 0)
                    return SafeBadRequest<List<BeverageAvailabilityDTO>>("Lista articoli non valida");

                var results = await _repository.CheckMultipleBeveragesAvailabilityAsync(articoliIds);

                LogAuditTrail("CHECK_MULTIPLE_BEVERAGES", "BeverageAvailability", $"Count:{articoliIds.Count}");

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica disponibilità multipla per {Count} bevande", articoliIds?.Count);
                return SafeInternalError<List<BeverageAvailabilityDTO>>("Errore durante la verifica disponibilità multipla");
            }
        }

        [HttpGet("is-available/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> IsBeverageAvailable(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<bool>("ID articolo non valido");

                var isAvailable = await _repository.IsBeverageAvailableAsync(articoloId);
                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica rapida disponibilità bevanda: {ArticoloId}", articoloId);
                return SafeInternalError<bool>("Errore durante la verifica rapida disponibilità");
            }
        }

        [HttpPost("update/{articoloId}")]
        //[Authorize(Roles = "admin,system")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult<AvailabilityUpdateDTO>> UpdateBeverageAvailability(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<AvailabilityUpdateDTO>("ID articolo non valido");

                var result = await _repository.UpdateBeverageAvailabilityAsync(articoloId);

                LogAuditTrail("UPDATE_BEVERAGE_AVAILABILITY", "BeverageAvailability", articoloId.ToString());
                LogSecurityEvent("BeverageAvailabilityUpdated", new
                {
                    ArticoloId = articoloId,
                    NuovoStato = result.NuovoStatoDisponibilita,
                    Motivo = result.Motivo,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Articolo non trovato per aggiornamento: {ArticoloId}", articoloId);
                return SafeNotFound<AvailabilityUpdateDTO>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento disponibilità bevanda: {ArticoloId}", articoloId);
                return SafeInternalError<AvailabilityUpdateDTO>("Errore durante l'aggiornamento disponibilità");
            }
        }

        [HttpPost("update-all")]
        //[Authorize(Roles = "admin,system")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult<List<AvailabilityUpdateDTO>>> UpdateAllBeveragesAvailability()
        {
            try
            {
                var results = await _repository.UpdateAllBeveragesAvailabilityAsync();

                LogAuditTrail("UPDATE_ALL_BEVERAGES", "BeverageAvailability", "All");
                LogSecurityEvent("AllBeveragesAvailabilityUpdated", new
                {
                    Count = results.Count,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento massa disponibilità bevande");
                return SafeInternalError<List<AvailabilityUpdateDTO>>("Errore durante l'aggiornamento massa disponibilità");
            }
        }

        [HttpPost("force-availability/{articoloId}")]
        //[Authorize(Roles = "admin")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult> ForceBeverageAvailability(int articoloId, [FromBody] ForceAvailabilityRequest request)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest("ID articolo non valido");

                if (!IsModelValid(request))
                    return SafeBadRequest("Dati forzatura disponibilità non validi");

                await _repository.ForceBeverageAvailabilityAsync(articoloId, request.Disponibile, request.Motivo);

                LogAuditTrail("FORCE_BEVERAGE_AVAILABILITY", "BeverageAvailability", articoloId.ToString());
                LogSecurityEvent("BeverageAvailabilityForced", new
                {
                    ArticoloId = articoloId,
                    ForzatoA = request.Disponibile,
                    Motivo = request.Motivo,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                if (_environment.IsDevelopment())
                    return Ok("Disponibilità forzata con successo");
                else
                    return Ok("Operazione completata");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Articolo non trovato per forzatura: {ArticoloId}", articoloId);
                return SafeNotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore forzatura disponibilità bevanda: {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante la forzatura disponibilità");
            }
        }

        [HttpGet("menu-status")]
        [AllowAnonymous]
        public async Task<ActionResult<MenuAvailabilityDTO>> GetMenuAvailabilityStatus()
        {
            try
            {
                var status = await _repository.GetMenuAvailabilityStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero stato disponibilità menu");
                return SafeInternalError<MenuAvailabilityDTO>("Errore durante il recupero stato menu");
            }
        }

        [HttpGet("primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> GetAvailableBeveragesForPrimoPiano([FromQuery] int numeroElementi = 6)
        {
            try
            {
                if (numeroElementi <= 0 || numeroElementi > 20)
                    return SafeBadRequest<List<BeverageAvailabilityDTO>>("Numero elementi non valido (1-20)");

                var bevande = await _repository.GetAvailableBeveragesForPrimoPianoAsync(numeroElementi);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero bevande per primo piano");
                return SafeInternalError<List<BeverageAvailabilityDTO>>("Errore durante il recupero primo piano");
            }
        }

        [HttpGet("sostituti-primo-piano")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> FindSostitutiPrimoPiano([FromQuery] int numeroRichieste = 3)
        {
            try
            {
                if (numeroRichieste <= 0 || numeroRichieste > 10)
                    return SafeBadRequest<List<BeverageAvailabilityDTO>>("Numero richieste non valido (1-10)");

                var sostituti = await _repository.FindSostitutiPrimoPianoAsync(numeroRichieste);
                return Ok(sostituti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca sostituti primo piano");
                return SafeInternalError<List<BeverageAvailabilityDTO>>("Errore durante la ricerca sostituti");
            }
        }

        [HttpGet("ingredienti-critici")]
        //[Authorize(Roles = "admin,barista")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult<List<IngredienteMancanteDTO>>> GetIngredientiCritici()
        {
            try
            {
                var ingredientiCritici = await _repository.GetIngredientiCriticiAsync();

                LogAuditTrail("GET_CRITICAL_INGREDIENTS", "BeverageAvailability", $"Count:{ingredientiCritici.Count}");

                return Ok(ingredientiCritici);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero ingredienti critici");
                return SafeInternalError<List<IngredienteMancanteDTO>>("Errore durante il recupero ingredienti critici");
            }
        }

        [HttpGet("low-stock-count")]
        //[Authorize(Roles = "admin,barista")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult<int>> GetCountBeveragesWithLowStock()
        {
            try
            {
                var count = await _repository.GetCountBeveragesWithLowStockAsync();

                LogAuditTrail("GET_LOW_STOCK_COUNT", "BeverageAvailability", $"Count:{count}");

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio bevande con stock basso");
                return SafeInternalError<int>("Errore durante il conteggio stock basso");
            }
        }

        [HttpGet("can-be-primo-piano/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CanBeverageBeInPrimoPiano(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<bool>("ID articolo non valido");

                var canBe = await _repository.CanBeverageBeInPrimoPianoAsync(articoloId);
                return Ok(canBe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore verifica bevanda per primo piano: {ArticoloId}", articoloId);
                return SafeInternalError<bool>("Errore durante la verifica primo piano");
            }
        }

        [HttpGet("affected-by-ingredient/{ingredienteId}")]
        //[Authorize(Roles = "admin,barista")]
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PER TEST
        public async Task<ActionResult<List<int>>> GetBeveragesAffectedByIngredient(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SafeBadRequest<List<int>>("ID ingrediente non valido");

                var bevandeAffected = await _repository.GetBeveragesAffectedByIngredientAsync(ingredienteId);

                LogAuditTrail("GET_AFFECTED_BEVERAGES", "BeverageAvailability", $"Ingrediente:{ingredienteId}");

                return Ok(bevandeAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero bevande affette da ingrediente: {IngredienteId}", ingredienteId);
                return SafeInternalError<List<int>>("Errore durante il recupero bevande affette");
            }
        }
    }
}