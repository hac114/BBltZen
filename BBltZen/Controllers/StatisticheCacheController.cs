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
                LogAuditTrail("GET_ALL", "StatisticheCache", "All");
                return Ok(result);
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
                LogAuditTrail("GET_BY_TIPO", "StatisticheCache", tipoStatistica);
                return Ok(result);
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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

                if (id != statisticheCacheDto.Id)
                    return SafeBadRequest("Identificativi non corrispondenti");

                if (!IsModelValid(statisticheCacheDto))
                    return SafeBadRequest("Dati cache statistiche non validi");

                // ✅ VERIFICA ESISTENZA
                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Cache statistiche");

                await _repository.UpdateAsync(statisticheCacheDto);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("UPDATE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheUpdated", new
                {
                    CacheId = id,
                    statisticheCacheDto.TipoStatistica,
                    statisticheCacheDto.Periodo,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
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

                var cache = await _repository.GetByIdAsync(id);
                if (cache == null)
                    return SafeNotFound("Cache statistiche");

                await _repository.DeleteAsync(id);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("DELETE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheDeleted", new
                {
                    CacheId = id,
                    cache.TipoStatistica,
                    cache.Periodo,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (Exception ex)
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

                // ✅ AUDIT OTTIMIZZATO
                LogAuditTrail("AGGIORNA_CACHE", "StatisticheCache", $"{request.TipoStatistica}_{request.Periodo}");
                LogSecurityEvent("StatisticheCacheAggiornata", new
                {
                    request.TipoStatistica,
                    request.Periodo,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (Exception ex)
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

                LogAuditTrail("CHECK_VALIDITY", "StatisticheCache", $"{tipoStatistica}_{periodo}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo validità cache per tipo {TipoStatistica} e periodo {Periodo}",
                    tipoStatistica, periodo);
                return SafeInternalError<bool>("Errore nel controllo validità cache");
            }
        }

        [HttpPost("create")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> Create([FromBody] StatisticheCacheDTO statisticheCacheDto)
        {
            try
            {
                if (!IsModelValid(statisticheCacheDto))
                    return SafeBadRequest<StatisticheCacheDTO>("Dati cache statistiche non validi");

                // ✅ CORREZIONE: USA IL RISULTATO DI AddAsync (PATTERN STANDARD)
                var result = await _repository.AddAsync(statisticheCacheDto);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("CREATE", "StatisticheCache", result.Id.ToString());
                LogSecurityEvent("StatisticheCacheCreated", new
                {
                    result.Id,
                    result.TipoStatistica,
                    result.Periodo,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest<StatisticheCacheDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della cache statistiche");
                return SafeInternalError<StatisticheCacheDTO>("Errore durante la creazione");
            }
        }

        [HttpGet("carrello/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCarrelloDTO>> GetStatisticheCarrelloByPeriodo(string periodo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest<StatisticheCarrelloDTO>("Periodo non valido");

                var result = await _repository.GetStatisticheCarrelloByPeriodoAsync(periodo);

                if (result == null)
                    return SafeNotFound<StatisticheCarrelloDTO>("Statistiche carrello");

                LogAuditTrail("GET_STATISTICHE_CARRELLO_BY_PERIODO", "StatisticheCarrello", periodo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche carrello per periodo {Periodo}", periodo);
                return SafeInternalError<StatisticheCarrelloDTO>("Errore nel recupero statistiche carrello");
            }
        }

        [HttpPost("carrello/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> SalvaStatisticheCarrello(string periodo, [FromBody] StatisticheCarrelloDTO statistiche)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest("Periodo non valido");

                await _repository.SalvaStatisticheCarrelloAsync(periodo, statistiche);

                // ✅ AUDIT & SECURITY OTTIMIZZATO PER VS
                LogAuditTrail("SALVA_STATISTICHE_CARRELLO", "StatisticheCarrello", periodo);
                LogSecurityEvent("StatisticheCarrelloSalvate", new
                {
                    Periodo = periodo,
                    statistiche.TotaleOrdini,
                    statistiche.FatturatoTotale,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel salvataggio statistiche carrello per periodo {Periodo}", periodo);
                return SafeInternalError("Errore nel salvataggio statistiche carrello");
            }
        }

        [HttpGet("carrello/valida/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> IsStatisticheCarrelloValide(string periodo, [FromQuery] int oreValidita = 24)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest<bool>("Periodo non valido");

                if (oreValidita <= 0)
                    return SafeBadRequest<bool>("Ore validità deve essere maggiore di 0");

                var validita = TimeSpan.FromHours(oreValidita);
                var result = await _repository.IsStatisticheCarrelloValideAsync(periodo, validita);

                LogAuditTrail("CHECK_VALIDITY_CARRELLO", "StatisticheCarrello", periodo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo validità statistiche carrello per periodo {Periodo}", periodo);
                return SafeInternalError<bool>("Errore nel controllo validità statistiche carrello");
            }
        }

        [HttpGet("carrello/periodi")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<string>>> GetPeriodiDisponibiliCarrello()
        {
            try
            {
                var result = await _repository.GetPeriodiDisponibiliCarrelloAsync();
                LogAuditTrail("GET_PERIODI_CARRELLO", "StatisticheCarrello", "All");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero periodi disponibili carrello");
                return SafeInternalError<IEnumerable<string>>("Errore nel recupero periodi carrello");
            }
        }
    }    
}