using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SistemaCacheController : ControllerBase
    {
        private readonly ISistemaCacheRepository _cacheRepository;
        private readonly ILogger<SistemaCacheController> _logger;

        public SistemaCacheController(
            ISistemaCacheRepository cacheRepository,
            ILogger<SistemaCacheController> logger)
        {
            _cacheRepository = cacheRepository;
            _logger = logger;
        }

        [HttpGet("get/{chiave}")]
        public async Task<ActionResult<object>> Get(string chiave)
        {
            try
            {
                var valore = await _cacheRepository.GetAsync<object>(chiave);
                if (valore == null)
                    return NotFound($"Chiave '{chiave}' non trovata in cache");

                return Ok(valore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero cache per chiave: {chiave}");
                return StatusCode(500, $"Errore interno: {ex.Message}");
            }
        }

        [HttpPost("set/{chiave}")]
        public async Task<ActionResult<CacheOperationResultDTO>> Set(string chiave, [FromBody] CacheSetRequest request)
        {
            try
            {
                var risultato = await _cacheRepository.SetAsync(chiave, request.Valore, request.Durata);

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'impostazione cache per chiave: {chiave}");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = chiave
                });
            }
        }

        [HttpDelete("remove/{chiave}")]
        public async Task<ActionResult<CacheOperationResultDTO>> Remove(string chiave)
        {
            try
            {
                var risultato = await _cacheRepository.RemoveAsync(chiave);

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella rimozione cache per chiave: {chiave}");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = chiave
                });
            }
        }

        [HttpGet("exists/{chiave}")]
        public async Task<ActionResult<bool>> Exists(string chiave)
        {
            try
            {
                var exists = await _cacheRepository.ExistsAsync(chiave);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel verificare esistenza cache per chiave: {chiave}");
                return StatusCode(500, false);
            }
        }

        [HttpPost("bulk/get")]
        public async Task<ActionResult<CacheBulkResultDTO>> GetBulk([FromBody] List<string> chiavi)
        {
            try
            {
                var risultato = await _cacheRepository.GetBulkAsync(chiavi);
                return Ok(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk get");
                return StatusCode(500, new CacheBulkResultDTO
                {
                    OperazioniCompletate = 0,
                    OperazioniFallite = chiavi.Count,
                    TempoEsecuzione = TimeSpan.Zero
                });
            }
        }

        [HttpPost("bulk/set")]
        public async Task<ActionResult<CacheBulkResultDTO>> SetBulk([FromBody] BulkSetRequest request)
        {
            try
            {
                var risultato = await _cacheRepository.SetBulkAsync(request.Valori, request.Durata);
                return Ok(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk set");
                return StatusCode(500, new CacheBulkResultDTO
                {
                    OperazioniCompletate = 0,
                    OperazioniFallite = request.Valori.Count,
                    TempoEsecuzione = TimeSpan.Zero
                });
            }
        }

        [HttpPost("bulk/remove")]
        public async Task<ActionResult<CacheBulkResultDTO>> RemoveBulk([FromBody] List<string> chiavi)
        {
            try
            {
                var risultato = await _cacheRepository.RemoveBulkAsync(chiavi);
                return Ok(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk remove");
                return StatusCode(500, new CacheBulkResultDTO
                {
                    OperazioniCompletate = 0,
                    OperazioniFallite = chiavi.Count,
                    TempoEsecuzione = TimeSpan.Zero
                });
            }
        }

        [HttpGet("info")]
        public async Task<ActionResult<CacheInfoDTO>> GetCacheInfo()
        {
            try
            {
                var info = await _cacheRepository.GetCacheInfoAsync();
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero informazioni cache");
                return StatusCode(500, new CacheInfoDTO());
            }
        }

        [HttpPost("cleanup")]
        public async Task<ActionResult<CacheCleanupDTO>> CleanupExpired()
        {
            try
            {
                var risultato = await _cacheRepository.CleanupExpiredAsync();
                return Ok(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia cache");
                return StatusCode(500, new CacheCleanupDTO());
            }
        }

        [HttpPost("refresh/{chiave}")]
        public async Task<ActionResult<bool>> Refresh(string chiave, [FromBody] TimeSpan? nuovaDurata = null)
        {
            try
            {
                var successo = await _cacheRepository.RefreshAsync(chiave, nuovaDurata);
                return Ok(successo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel refresh cache per chiave: {chiave}");
                return StatusCode(500, false);
            }
        }

        // Cache per dati specifici del dominio Bubble Tea
        [HttpPost("preload/menu")]
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadMenu()
        {
            try
            {
                var risultato = await _cacheRepository.CacheMenuAsync();

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento menu in cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = "MENU"
                });
            }
        }

        [HttpPost("preload/statistiche")]
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadStatistiche()
        {
            try
            {
                var risultato = await _cacheRepository.CacheStatisticheAsync();

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento statistiche in cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = "STATISTICHE"
                });
            }
        }

        [HttpPost("preload/prezzi")]
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadPrezzi()
        {
            try
            {
                var risultato = await _cacheRepository.CachePrezziAsync();

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento prezzi in cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = "PREZZI"
                });
            }
        }

        [HttpPost("preload/configurazioni")]
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadConfigurazioni()
        {
            try
            {
                var risultato = await _cacheRepository.CacheConfigurazioniAsync();

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento configurazioni in cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = "CONFIGURAZIONI"
                });
            }
        }

        [HttpPost("preload/all")]
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadAll()
        {
            try
            {
                var risultato = await _cacheRepository.PreloadCommonDataAsync();

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return BadRequest(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento dati comuni in cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}",
                    Chiave = "PRELOAD_ALL"
                });
            }
        }

        [HttpGet("performance")]
        public async Task<ActionResult<CachePerformanceDTO>> GetPerformanceStats()
        {
            try
            {
                var stats = await _cacheRepository.GetPerformanceStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche performance cache");
                return StatusCode(500, new CachePerformanceDTO());
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<List<CacheStatisticheDTO>>> GetCacheStatistics()
        {
            try
            {
                var statistiche = await _cacheRepository.GetCacheStatisticsAsync();
                return Ok(statistiche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche cache");
                return StatusCode(500, new List<CacheStatisticheDTO>());
            }
        }

        [HttpPost("statistics/reset")]
        public async Task<ActionResult> ResetStatistics()
        {
            try
            {
                await _cacheRepository.ResetStatisticsAsync();
                return Ok("Statistiche cache resetate con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel reset statistiche cache");
                return StatusCode(500, "Errore nel reset statistiche");
            }
        }

        [HttpPost("compact")]
        public async Task<ActionResult<CacheOperationResultDTO>> CompactCache()
        {
            try
            {
                var risultato = await _cacheRepository.CompactCacheAsync();
                return Ok(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella compattazione cache");
                return StatusCode(500, new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore interno: {ex.Message}"
                });
            }
        }

        [HttpPost("clear")]
        public async Task<ActionResult> ClearAll()
        {
            try
            {
                await _cacheRepository.ClearAllAsync();
                return Ok("Cache pulita completamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia completa cache");
                return StatusCode(500, "Errore nella pulizia cache");
            }
        }

        [HttpGet("validate/{tipoCache}")]
        public async Task<ActionResult<bool>> ValidateCache(string tipoCache)
        {
            try
            {
                var isValid = await _cacheRepository.IsCacheValidAsync(tipoCache);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella validazione cache per tipo: {tipoCache}");
                return StatusCode(500, false);
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult<object>> HealthCheck()
        {
            try
            {
                var performance = await _cacheRepository.GetPerformanceStatsAsync();
                var info = await _cacheRepository.GetCacheInfoAsync();

                return Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.Now,
                    Performance = new
                    {
                        HitRate = $"{performance.HitRate}%",
                        TotalHits = info.HitsTotali,
                        TotalMisses = info.MissesTotali
                    },
                    Memory = new
                    {
                        ActiveEntries = info.TotaleEntry,
                        LastCleanup = info.UltimaPulizia
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel health check cache");
                return StatusCode(500, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.Now,
                    Error = ex.Message
                });
            }
        }
    }

    // DTO per le richieste
    public class CacheSetRequest
    {
        public object Valore { get; set; } = new();
        public TimeSpan? Durata { get; set; }
    }

    public class BulkSetRequest
    {
        public Dictionary<string, object> Valori { get; set; } = new();
        public TimeSpan? Durata { get; set; }
    }
}