// BBltZen/Controllers/StatisticheCacheController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StatisticheCacheController : SecureBaseController
    {
        private readonly IStatisticheCacheRepository _repository;

        public StatisticheCacheController(
            IStatisticheCacheRepository repository,
            IWebHostEnvironment environment,
            ILogger<StatisticheCacheController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        [HttpGet]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<StatisticheCacheDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                LogAuditTrail("GET_ALL_STATISTICHE_CACHE", "StatisticheCache", "All");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutte le cache statistiche");
                return SafeInternalError<IEnumerable<StatisticheCacheDTO>>("Errore nel recupero dati statistiche");
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<StatisticheCacheDTO>("ID cache statistiche non valido");

                var result = await _repository.GetByIdAsync(id);

                if (result == null)
                    return SafeNotFound<StatisticheCacheDTO>("Cache statistiche");

                LogAuditTrail("GET_STATISTICHE_CACHE_BY_ID", "StatisticheCache", id.ToString());
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache statistiche {Id}", id);
                return SafeInternalError<StatisticheCacheDTO>("Errore nel recupero cache");
            }
        }

        [HttpGet("tipo/{tipoStatistica}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<StatisticheCacheDTO>>> GetByTipo(string tipoStatistica)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica))
                    return SafeBadRequest<IEnumerable<StatisticheCacheDTO>>("Tipo statistica non valido");

                var result = await _repository.GetByTipoAsync(tipoStatistica);
                LogAuditTrail("GET_STATISTICHE_CACHE_BY_TIPO", "StatisticheCache", tipoStatistica);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache per tipo {TipoStatistica}", tipoStatistica);
                return SafeInternalError<IEnumerable<StatisticheCacheDTO>>("Errore nel recupero cache per tipo");
            }
        }

        [HttpGet("tipo/{tipoStatistica}/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> GetByTipoAndPeriodo(string tipoStatistica, string periodo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica) || string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest<StatisticheCacheDTO>("Tipo statistica o periodo non validi");

                var result = await _repository.GetByTipoAndPeriodoAsync(tipoStatistica, periodo);

                if (result == null)
                    return SafeNotFound<StatisticheCacheDTO>("Cache statistiche");

                LogAuditTrail("GET_STATISTICHE_CACHE_BY_TIPO_PERIODO", "StatisticheCache", $"{tipoStatistica}_{periodo}");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache per tipo {TipoStatistica} e periodo {Periodo}", tipoStatistica, periodo);
                return SafeInternalError<StatisticheCacheDTO>("Errore nel recupero cache specifica");
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> AggiornaCache([FromBody] AggiornaCacheRequestDTO request) // ✅ USA DTO
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest("Dati aggiornamento cache non validi");

                await _repository.AggiornaCacheAsync(request.TipoStatistica, request.Periodo, request.Metriche);

                LogAuditTrail("AGGIORNA_CACHE_STATISTICHE", "StatisticheCache", $"{request.TipoStatistica}_{request.Periodo}");
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento cache per tipo {TipoStatistica} e periodo {Periodo}",
                    request.TipoStatistica, request.Periodo);
                return SafeInternalError("Errore nell'aggiornamento cache");
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] StatisticheCacheDTO statisticheCacheDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID cache statistiche non valido");

                if (!IsModelValid(statisticheCacheDto))
                    return SafeBadRequest("Dati cache statistiche non validi");

                if (statisticheCacheDto.Id != id)
                    return SafeBadRequest("Identificativi non corrispondenti");

                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return SafeNotFound("Cache statistiche");

                await _repository.UpdateAsync(statisticheCacheDto);

                LogAuditTrail("UPDATE_STATISTICHE_CACHE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheUpdated", new
                {
                    CacheId = id,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento cache statistiche {Id}", id);
                return SafeInternalError("Errore nell'aggiornamento cache");
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID cache statistiche non valido");

                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return SafeNotFound("Cache statistiche");

                await _repository.DeleteAsync(id);

                LogAuditTrail("DELETE_STATISTICHE_CACHE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheDeleted", new
                {
                    CacheId = id,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione cache statistiche {Id}", id);
                return SafeInternalError("Errore nell'eliminazione cache");
            }
        }

        [HttpPost("aggiorna")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> AggiornaCacheStatistiche([FromBody] AggiornaCacheRequestDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest("Dati aggiornamento cache non validi");

                await _repository.AggiornaCacheAsync(request.TipoStatistica, request.Periodo, request.Metriche);

                LogAuditTrail("AGGIORNA_CACHE_STATISTICHE", "StatisticheCache", $"{request.TipoStatistica}_{request.Periodo}");
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento cache per tipo {TipoStatistica} e periodo {Periodo}",
                    request.TipoStatistica, request.Periodo);
                return SafeInternalError("Errore nell'aggiornamento cache");
            }
        }

        [HttpGet("valida/tipo/{tipoStatistica}/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> IsCacheValida(string tipoStatistica, string periodo, [FromQuery] int oreValidita = 24)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica) || string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest<bool>("Tipo statistica o periodo non validi");

                if (oreValidita <= 0)
                    return SafeBadRequest<bool>("Ore validità deve essere maggiore di 0");

                var validita = TimeSpan.FromHours(oreValidita);
                var result = await _repository.IsCacheValidaAsync(tipoStatistica, periodo, validita);

                LogAuditTrail("CHECK_CACHE_VALIDITY", "StatisticheCache", $"{tipoStatistica}_{periodo}_{oreValidita}h");
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo validità cache per tipo {TipoStatistica} e periodo {Periodo}",
                    tipoStatistica, periodo);
                return SafeInternalError<bool>("Errore nel controllo validità cache");
            }
        }
    }    
}