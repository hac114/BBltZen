using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

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

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_STATISTICHE_CACHE", "StatisticheCache", "All");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero di tutte le cache statistiche");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero cache statistiche: {ex.Message}"
                        : "Errore interno nel recupero dati statistiche"
                );
            }
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID cache statistiche non valido: deve essere maggiore di 0"
                            : "ID non valido"
                    );

                var result = await _repository.GetByIdAsync(id);

                if (result == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Cache statistiche con ID {id} non trovata"
                            : "Cache statistiche non trovata"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_STATISTICHE_CACHE_BY_ID", "StatisticheCache", id.ToString());

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache statistiche {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero cache statistiche {id}: {ex.Message}"
                        : "Errore interno nel recupero cache"
                );
            }
        }

        [HttpGet("tipo/{tipoStatistica}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<IEnumerable<StatisticheCacheDTO>>> GetByTipo(string tipoStatistica)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo statistica non valido: non può essere vuoto"
                            : "Tipo statistica non valido"
                    );

                var result = await _repository.GetByTipoAsync(tipoStatistica);

                // ✅ Log per audit
                LogAuditTrail("GET_STATISTICHE_CACHE_BY_TIPO", "StatisticheCache", tipoStatistica);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache per tipo {TipoStatistica}", tipoStatistica);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero cache per tipo {tipoStatistica}: {ex.Message}"
                        : "Errore interno nel recupero cache per tipo"
                );
            }
        }

        [HttpGet("tipo/{tipoStatistica}/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> GetByTipoAndPeriodo(string tipoStatistica, string periodo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica) || string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo statistica o periodo non validi: non possono essere vuoti"
                            : "Parametri di ricerca non validi"
                    );

                var result = await _repository.GetByTipoAndPeriodoAsync(tipoStatistica, periodo);

                if (result == null)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Cache statistiche non trovata per tipo {tipoStatistica} e periodo {periodo}"
                            : "Cache statistiche non trovata"
                    );

                // ✅ Log per audit
                LogAuditTrail("GET_STATISTICHE_CACHE_BY_TIPO_PERIODO", "StatisticheCache", $"{tipoStatistica}_{periodo}");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache per tipo {TipoStatistica} e periodo {Periodo}", tipoStatistica, periodo);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel recupero cache per tipo {tipoStatistica} e periodo {periodo}: {ex.Message}"
                        : "Errore interno nel recupero cache specifica"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatisticheCacheDTO>> Create([FromBody] StatisticheCacheDTO statisticheCacheDto)
        {
            try
            {
                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(statisticheCacheDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati cache statistiche non validi: modello di binding fallito"
                            : "Dati inseriti non validi"
                    );

                await _repository.AddAsync(statisticheCacheDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("CREATE_STATISTICHE_CACHE", "StatisticheCache", statisticheCacheDto.Id.ToString());
                LogSecurityEvent("StatisticheCacheCreated", new
                {
                    CacheId = statisticheCacheDto.Id,
                    TipoStatistica = statisticheCacheDto.TipoStatistica,
                    Periodo = statisticheCacheDto.Periodo,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return CreatedAtAction(nameof(GetById), new { id = statisticheCacheDto.Id }, statisticheCacheDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione cache statistiche");
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nella creazione cache statistiche: {ex.Message}"
                        : "Errore interno nella creazione cache"
                );
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] StatisticheCacheDTO statisticheCacheDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID cache statistiche non valido: deve essere maggiore di 0"
                            : "ID non valido"
                    );

                // ✅ La validazione dei campi è gestita automaticamente dai Data Annotations del DTO
                if (!IsModelValid(statisticheCacheDto))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati cache statistiche non validi: modello di binding fallito"
                            : "Dati aggiornamento non validi"
                    );

                if (statisticheCacheDto.Id != id)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? $"ID nel corpo ({statisticheCacheDto.Id}) non corrisponde all'ID nell'URL ({id})"
                            : "Identificativi non corrispondenti"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Cache statistiche con ID {id} non trovata per l'aggiornamento"
                            : "Cache statistiche non trovata"
                    );

                await _repository.UpdateAsync(statisticheCacheDto);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("UPDATE_STATISTICHE_CACHE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheUpdated", new
                {
                    CacheId = id,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento cache statistiche {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'aggiornamento cache statistiche {id}: {ex.Message}"
                        : "Errore interno nell'aggiornamento cache"
                );
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "ID cache statistiche non valido: deve essere maggiore di 0"
                            : "ID non valido"
                    );

                // Verifica esistenza
                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return SafeNotFound(
                        _environment.IsDevelopment()
                            ? $"Cache statistiche con ID {id} non trovata per l'eliminazione"
                            : "Cache statistiche non trovata"
                    );

                await _repository.DeleteAsync(id);

                // ✅ Log per audit e sicurezza
                LogAuditTrail("DELETE_STATISTICHE_CACHE", "StatisticheCache", id.ToString());
                LogSecurityEvent("StatisticheCacheDeleted", new
                {
                    CacheId = id,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione cache statistiche {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'eliminazione cache statistiche {id}: {ex.Message}"
                        : "Errore interno nell'eliminazione cache"
                );
            }
        }

        [HttpPost("aggiorna")]
        //[Authorize(Roles = "admin,manager")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> AggiornaCache([FromBody] AggiornaCacheRequest request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Dati aggiornamento cache non validi: modello di binding fallito"
                            : "Dati aggiornamento non validi"
                    );

                await _repository.AggiornaCacheAsync(request.TipoStatistica, request.Periodo, request.Metriche);

                // ✅ Log per audit
                LogAuditTrail("AGGIORNA_CACHE_STATISTICHE", "StatisticheCache", $"{request.TipoStatistica}_{request.Periodo}");

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento cache per tipo {TipoStatistica} e periodo {Periodo}",
                    request.TipoStatistica, request.Periodo);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nell'aggiornamento cache per tipo {request.TipoStatistica} e periodo {request.Periodo}: {ex.Message}"
                        : "Errore interno nell'aggiornamento cache"
                );
            }
        }

        [HttpGet("valida/tipo/{tipoStatistica}/periodo/{periodo}")]
        //[Authorize(Roles = "admin,manager,user")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> IsCacheValida(string tipoStatistica, string periodo, [FromQuery] int oreValidita = 24)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoStatistica) || string.IsNullOrWhiteSpace(periodo))
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Tipo statistica o periodo non validi: non possono essere vuoti"
                            : "Parametri di ricerca non validi"
                    );

                if (oreValidita <= 0)
                    return SafeBadRequest(
                        _environment.IsDevelopment()
                            ? "Ore validità deve essere maggiore di 0"
                            : "Durata validità non valida"
                    );

                var validita = TimeSpan.FromHours(oreValidita);
                var result = await _repository.IsCacheValidaAsync(tipoStatistica, periodo, validita);

                // ✅ Log per audit
                LogAuditTrail("CHECK_CACHE_VALIDITY", "StatisticheCache", $"{tipoStatistica}_{periodo}_{oreValidita}h");

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel controllo validità cache per tipo {TipoStatistica} e periodo {Periodo}",
                    tipoStatistica, periodo);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore nel controllo validità cache per tipo {tipoStatistica} e periodo {periodo}: {ex.Message}"
                        : "Errore interno nel controllo validità cache"
                );
            }
        }
    }

    public class AggiornaCacheRequest
    {
        public string TipoStatistica { get; set; } = null!;
        public string Periodo { get; set; } = null!;
        public string Metriche { get; set; } = null!;
    }
}