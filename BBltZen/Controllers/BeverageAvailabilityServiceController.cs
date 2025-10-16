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
    public class BeverageAvailabilityServiceController : ControllerBase
    {
        private readonly IBeverageAvailabilityServiceRepository _availabilityService;
        private readonly ILogger<BeverageAvailabilityServiceController> _logger;

        public BeverageAvailabilityServiceController(
            IBeverageAvailabilityServiceRepository availabilityService,
            ILogger<BeverageAvailabilityServiceController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        [HttpGet("check/{articoloId}")]
        public async Task<ActionResult<BeverageAvailabilityDTO>> CheckBeverageAvailability(int articoloId)
        {
            try
            {
                _logger.LogInformation($"Verifica disponibilità bevanda: {articoloId}");
                var result = await _availabilityService.CheckBeverageAvailabilityAsync(articoloId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Articolo non trovato: {articoloId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica disponibilità bevanda: {articoloId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("check-multiple")]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> CheckMultipleBeveragesAvailability([FromBody] List<int> articoliIds)
        {
            try
            {
                _logger.LogInformation($"Verifica disponibilità multipla per {articoliIds.Count} bevande");
                var results = await _availabilityService.CheckMultipleBeveragesAvailabilityAsync(articoliIds);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica disponibilità multipla per {articoliIds.Count} bevande");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("is-available/{articoloId}")]
        public async Task<ActionResult<bool>> IsBeverageAvailable(int articoloId)
        {
            try
            {
                var isAvailable = await _availabilityService.IsBeverageAvailableAsync(articoloId);
                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica rapida disponibilità bevanda: {articoloId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("update/{articoloId}")]
        public async Task<ActionResult<AvailabilityUpdateDTO>> UpdateBeverageAvailability(int articoloId)
        {
            try
            {
                _logger.LogInformation($"Aggiornamento disponibilità bevanda: {articoloId}");
                var result = await _availabilityService.UpdateBeverageAvailabilityAsync(articoloId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Articolo non trovato per aggiornamento: {articoloId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore aggiornamento disponibilità bevanda: {articoloId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("update-all")]
        public async Task<ActionResult<List<AvailabilityUpdateDTO>>> UpdateAllBeveragesAvailability()
        {
            try
            {
                _logger.LogInformation("Aggiornamento disponibilità TUTTE le bevande");
                var results = await _availabilityService.UpdateAllBeveragesAvailabilityAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore aggiornamento massa disponibilità bevande");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpPost("force-availability/{articoloId}")]
        public async Task<ActionResult> ForceBeverageAvailability(int articoloId, [FromBody] ForceAvailabilityRequest request)
        {
            try
            {
                _logger.LogInformation($"Forzatura disponibilità bevanda {articoloId} a: {request.Disponibile}");
                await _availabilityService.ForceBeverageAvailabilityAsync(articoloId, request.Disponibile, request.Motivo);
                return Ok(new { message = "Disponibilità forzata con successo" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Articolo non trovato per forzatura: {articoloId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore forzatura disponibilità bevanda: {articoloId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("menu-status")]
        public async Task<ActionResult<MenuAvailabilityDTO>> GetMenuAvailabilityStatus()
        {
            try
            {
                _logger.LogInformation("Recupero stato disponibilità menu completo");
                var status = await _availabilityService.GetMenuAvailabilityStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero stato disponibilità menu");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("primo-piano")]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> GetAvailableBeveragesForPrimoPiano([FromQuery] int numeroElementi = 6)
        {
            try
            {
                var bevande = await _availabilityService.GetAvailableBeveragesForPrimoPianoAsync(numeroElementi);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero bevande per primo piano");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("sostituti-primo-piano")]
        public async Task<ActionResult<List<BeverageAvailabilityDTO>>> FindSostitutiPrimoPiano([FromQuery] int numeroRichieste = 3)
        {
            try
            {
                var sostituti = await _availabilityService.FindSostitutiPrimoPianoAsync(numeroRichieste);
                return Ok(sostituti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca sostituti primo piano");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("ingredienti-critici")]
        public async Task<ActionResult<List<IngredienteMancanteDTO>>> GetIngredientiCritici()
        {
            try
            {
                var ingredientiCritici = await _availabilityService.GetIngredientiCriticiAsync();
                return Ok(ingredientiCritici);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore recupero ingredienti critici");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("low-stock-count")]
        public async Task<ActionResult<int>> GetCountBeveragesWithLowStock()
        {
            try
            {
                var count = await _availabilityService.GetCountBeveragesWithLowStockAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore conteggio bevande con stock basso");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("can-be-primo-piano/{articoloId}")]
        public async Task<ActionResult<bool>> CanBeverageBeInPrimoPiano(int articoloId)
        {
            try
            {
                var canBe = await _availabilityService.CanBeverageBeInPrimoPianoAsync(articoloId);
                return Ok(canBe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore verifica bevanda per primo piano: {articoloId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }

        [HttpGet("affected-by-ingredient/{ingredienteId}")]
        public async Task<ActionResult<List<int>>> GetBeveragesAffectedByIngredient(int ingredienteId)
        {
            try
            {
                var bevandeAffected = await _availabilityService.GetBeveragesAffectedByIngredientAsync(ingredienteId);
                return Ok(bevandeAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore recupero bevande affette da ingrediente: {ingredienteId}");
                return StatusCode(500, new { message = "Errore interno del server" });
            }
        }
    }

    public class ForceAvailabilityRequest
    {
        public bool Disponibile { get; set; }
        public string? Motivo { get; set; }
    }
}