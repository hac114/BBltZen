// BBltZen/Controllers/SistemaCacheController.cs
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
    public class SistemaCacheController : SecureBaseController
    {
        private readonly ISistemaCacheRepository _cacheRepository;

        public SistemaCacheController(
            ISistemaCacheRepository cacheRepository,
            IWebHostEnvironment environment,
            ILogger<SistemaCacheController> logger)
            : base(environment, logger)
        {
            _cacheRepository = cacheRepository;
        }

        [HttpGet("get/{chiave}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<object>> Get(string chiave)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chiave))
                    return SafeBadRequest<object>("Chiave cache non valida");

                var valore = await _cacheRepository.GetAsync<object>(chiave);
                if (valore == null)
                    return SafeNotFound<object>("Cache");

                LogAuditTrail("CACHE_GET", "SistemaCache", chiave);
                return Ok(valore);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero cache per chiave: {Chiave}", chiave);
                return SafeInternalError<object>("Errore nel recupero cache");
            }
        }

        [HttpPost("set/{chiave}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> Set(string chiave, [FromBody] CacheSetRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chiave))
                    return SafeBadRequest<CacheOperationResultDTO>("Chiave cache non valida");

                if (!IsModelValid(request))
                    return SafeBadRequest<CacheOperationResultDTO>("Dati richiesta cache non validi");

                var risultato = await _cacheRepository.SetAsync(chiave, request.Valore, request.Durata);

                LogAuditTrail("CACHE_SET", "SistemaCache", chiave);
                LogSecurityEvent("CacheSet", new
                {
                    Chiave = chiave,
                    Durata = request.Durata?.ToString() ?? "default",
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'impostazione cache per chiave: {Chiave}", chiave);
                return SafeInternalError<CacheOperationResultDTO>("Errore nell'impostazione cache");
            }
        }

        [HttpDelete("remove/{chiave}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> Remove(string chiave)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chiave))
                    return SafeBadRequest<CacheOperationResultDTO>("Chiave cache non valida");

                var risultato = await _cacheRepository.RemoveAsync(chiave);

                LogAuditTrail("CACHE_REMOVE", "SistemaCache", chiave);
                LogSecurityEvent("CacheRemoved", new
                {
                    Chiave = chiave,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella rimozione cache per chiave: {Chiave}", chiave);
                return SafeInternalError<CacheOperationResultDTO>("Errore nella rimozione cache");
            }
        }

        [HttpGet("exists/{chiave}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> Exists(string chiave)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chiave))
                    return SafeBadRequest<bool>("Chiave cache non valida");

                var exists = await _cacheRepository.ExistsAsync(chiave);
                LogAuditTrail("CACHE_EXISTS", "SistemaCache", chiave);
                return Ok(exists);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel verificare esistenza cache per chiave: {Chiave}", chiave);
                return SafeInternalError<bool>("Errore nel controllo esistenza cache");
            }
        }

        [HttpPost("bulk/get")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheBulkResultDTO>> GetBulk([FromBody] List<string> chiavi)
        {
            try
            {
                if (chiavi == null || chiavi.Count == 0)
                    return SafeBadRequest<CacheBulkResultDTO>("Lista chiavi non valida");

                var risultato = await _cacheRepository.GetBulkAsync(chiavi);
                LogAuditTrail("CACHE_BULK_GET", "SistemaCache", $"Chiavi: {chiavi.Count}");
                return Ok(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk get");
                return SafeInternalError<CacheBulkResultDTO>("Errore nell'operazione bulk get");
            }
        }

        [HttpPost("bulk/set")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheBulkResultDTO>> SetBulk([FromBody] CacheBulkOperationDTO request)
        {
            try
            {
                if (!IsModelValid(request))
                    return SafeBadRequest<CacheBulkResultDTO>("Dati richiesta bulk set non validi");

                if (request.Valori == null || request.Valori.Count == 0)
                    return SafeBadRequest<CacheBulkResultDTO>("Dati valori non validi");

                var risultato = await _cacheRepository.SetBulkAsync(request.Valori, request.Durata);

                LogAuditTrail("CACHE_BULK_SET", "SistemaCache", $"Chiavi: {request.Valori.Count}");
                LogSecurityEvent("CacheBulkSet", new
                {
                    NumeroChiavi = request.Valori.Count,
                    Durata = request.Durata?.ToString() ?? "default",
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk set");
                return SafeInternalError<CacheBulkResultDTO>("Errore nell'operazione bulk set");
            }
        }

        [HttpPost("bulk/remove")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheBulkResultDTO>> RemoveBulk([FromBody] List<string> chiavi)
        {
            try
            {
                if (chiavi == null || chiavi.Count == 0)
                    return SafeBadRequest<CacheBulkResultDTO>("Lista chiavi non valida");

                var risultato = await _cacheRepository.RemoveBulkAsync(chiavi);

                LogAuditTrail("CACHE_BULK_REMOVE", "SistemaCache", $"Chiavi: {chiavi.Count}");
                LogSecurityEvent("CacheBulkRemoved", new
                {
                    NumeroChiavi = chiavi.Count,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk remove");
                return SafeInternalError<CacheBulkResultDTO>("Errore nell'operazione bulk remove");
            }
        }

        [HttpGet("info")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheInfoDTO>> GetCacheInfo()
        {
            try
            {
                var info = await _cacheRepository.GetCacheInfoAsync();
                LogAuditTrail("CACHE_INFO", "SistemaCache", $"HitRate: {info.HitRatePercentuale}%");
                return Ok(info);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero informazioni cache");
                return SafeInternalError<CacheInfoDTO>("Errore nel recupero informazioni cache");
            }
        }

        [HttpPost("cleanup")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheCleanupDTO>> CleanupExpired()
        {
            try
            {
                var risultato = await _cacheRepository.CleanupExpiredAsync();

                LogAuditTrail("CACHE_CLEANUP", "SistemaCache", $"Rimossi: {risultato.EntryRimosse}");
                LogSecurityEvent("CacheCleanup", new
                {
                    EntryRimosse = risultato.EntryRimosse,
                    BytesLiberati = risultato.BytesLiberati,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia cache");
                return SafeInternalError<CacheCleanupDTO>("Errore nella pulizia cache");
            }
        }

        [HttpPost("refresh/{chiave}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> Refresh(string chiave, [FromBody] TimeSpan? nuovaDurata = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(chiave))
                    return SafeBadRequest<bool>("Chiave cache non valida");

                var successo = await _cacheRepository.RefreshAsync(chiave, nuovaDurata);
                LogAuditTrail("CACHE_REFRESH", "SistemaCache", chiave);
                return Ok(successo);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel refresh cache per chiave: {Chiave}", chiave);
                return SafeInternalError<bool>("Errore nel refresh cache");
            }
        }

        [HttpPost("preload/menu")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadMenu()
        {
            try
            {
                var risultato = await _cacheRepository.CacheMenuAsync();

                LogAuditTrail("CACHE_PRELOAD_MENU", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("MenuCached", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento menu in cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nel precaricamento menu");
            }
        }

        [HttpPost("preload/statistiche")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadStatistiche()
        {
            try
            {
                var risultato = await _cacheRepository.CacheStatisticheAsync();

                LogAuditTrail("CACHE_PRELOAD_STATISTICHE", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("StatisticheCached", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento statistiche in cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nel precaricamento statistiche");
            }
        }

        [HttpPost("preload/prezzi")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadPrezzi()
        {
            try
            {
                var risultato = await _cacheRepository.CachePrezziAsync();

                LogAuditTrail("CACHE_PRELOAD_PREZZI", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("PrezziCached", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento prezzi in cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nel precaricamento prezzi");
            }
        }

        [HttpPost("preload/configurazioni")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadConfigurazioni()
        {
            try
            {
                var risultato = await _cacheRepository.CacheConfigurazioniAsync();

                LogAuditTrail("CACHE_PRELOAD_CONFIG", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("ConfigCached", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento configurazioni in cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nel precaricamento configurazioni");
            }
        }

        [HttpPost("preload/all")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> PreloadAll()
        {
            try
            {
                var risultato = await _cacheRepository.PreloadCommonDataAsync();

                LogAuditTrail("CACHE_PRELOAD_ALL", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("AllDataCached", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                if (risultato.Successo)
                    return Ok(risultato);
                else
                    return SafeBadRequest(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento dati comuni in cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nel precaricamento dati comuni");
            }
        }

        [HttpGet("performance")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CachePerformanceDTO>> GetPerformanceStats()
        {
            try
            {
                var stats = await _cacheRepository.GetPerformanceStatsAsync();
                LogAuditTrail("CACHE_PERFORMANCE", "SistemaCache", $"HitRate: {stats.HitRate}%");
                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche performance cache");
                return SafeInternalError<CachePerformanceDTO>("Errore nel recupero statistiche performance");
            }
        }

        [HttpGet("statistics")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<List<CacheStatisticheDTO>>> GetCacheStatistics()
        {
            try
            {
                var statistiche = await _cacheRepository.GetCacheStatisticsAsync();
                LogAuditTrail("CACHE_STATISTICS", "SistemaCache", $"Count: {statistiche.Count}");
                return Ok(statistiche);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche cache");
                return SafeInternalError<List<CacheStatisticheDTO>>("Errore nel recupero statistiche cache");
            }
        }

        [HttpPost("statistics/reset")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> ResetStatistics()
        {
            try
            {
                await _cacheRepository.ResetStatisticsAsync();

                LogAuditTrail("CACHE_STATS_RESET", "SistemaCache", "OK");
                LogSecurityEvent("CacheStatsReset", new
                {
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok("Operazione completata");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel reset statistiche cache");
                return SafeInternalError("Errore nel reset statistiche");
            }
        }

        [HttpPost("compact")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<CacheOperationResultDTO>> CompactCache()
        {
            try
            {
                var risultato = await _cacheRepository.CompactCacheAsync();

                LogAuditTrail("CACHE_COMPACT", "SistemaCache", risultato.Successo.ToString());
                LogSecurityEvent("CacheCompacted", new
                {
                    Successo = risultato.Successo,
                    Messaggio = risultato.Messaggio,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok(risultato);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella compattazione cache");
                return SafeInternalError<CacheOperationResultDTO>("Errore nella compattazione cache");
            }
        }

        [HttpPost("clear")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> ClearAll()
        {
            try
            {
                await _cacheRepository.ClearAllAsync();

                LogAuditTrail("CACHE_CLEAR_ALL", "SistemaCache", "OK");
                LogSecurityEvent("CacheCleared", new
                {
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow
                });

                return Ok("Operazione completata");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella pulizia completa cache");
                return SafeInternalError("Errore nella pulizia cache");
            }
        }

        [HttpGet("validate/{tipoCache}")]
        //[Authorize(Roles = "admin,system")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<bool>> ValidateCache(string tipoCache)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoCache))
                    return SafeBadRequest<bool>("Tipo cache non valido");

                var isValid = await _cacheRepository.IsCacheValidAsync(tipoCache);
                LogAuditTrail("CACHE_VALIDATE", "SistemaCache", $"{tipoCache}: {isValid}");
                return Ok(isValid);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nella validazione cache per tipo: {TipoCache}", tipoCache);
                return SafeInternalError<bool>("Errore nella validazione cache");
            }
        }

        [HttpGet("health")]
        [AllowAnonymous] // ✅ HEALTH CHECK PUBBLICO
        public async Task<ActionResult<object>> HealthCheck()
        {
            try
            {
                var performance = await _cacheRepository.GetPerformanceStatsAsync();
                var info = await _cacheRepository.GetCacheInfoAsync();

                var health = new
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
                };

                LogAuditTrail("CACHE_HEALTH_CHECK", "SistemaCache", "OK");
                return Ok(health);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore nel health check cache");
                LogAuditTrail("CACHE_HEALTH_CHECK_FAILED", "SistemaCache", ex.Message);
                return SafeInternalError("Health check fallito");
            }
        }
    }
}