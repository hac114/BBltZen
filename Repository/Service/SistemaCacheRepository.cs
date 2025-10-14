using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class SistemaCacheRepository : ISistemaCacheRepository
    {
        private readonly BubbleTeaContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<SistemaCacheRepository> _logger;

        // Cache keys patterns
        private const string CACHE_STATS_KEY = "CacheStatistics";
        private const string MENU_CACHE_KEY = "MenuCompleto";
        private const string STATISTICHE_CACHE_KEY = "StatisticheGlobali";
        private const string PREZZI_CACHE_KEY = "PrezziCalcolati";
        private const string CONFIG_CACHE_KEY = "Configurazioni";

        // Statistiche
        private static long _totalHits = 0;
        private static long _totalMisses = 0;
        private static DateTime _lastCleanup = DateTime.Now;

        public SistemaCacheRepository(
            BubbleTeaContext context,
            IMemoryCache memoryCache,
            ILogger<SistemaCacheRepository> logger)
        {
            _context = context;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string chiave)
        {
            try
            {
                if (_memoryCache.TryGetValue(chiave, out T valore))
                {
                    _totalHits++;
                    _logger.LogDebug($"Cache HIT per chiave: {chiave}");
                    return valore;
                }
                else
                {
                    _totalMisses++;
                    _logger.LogDebug($"Cache MISS per chiave: {chiave}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel recupero cache per chiave: {chiave}");
                return default;
            }
        }

        public async Task<CacheOperationResultDTO> SetAsync<T>(string chiave, T valore, TimeSpan? durata = null)
        {
            try
            {
                var opzioni = new MemoryCacheEntryOptions
                {
                    Size = 1 // Dimensione base
                };

                if (durata.HasValue)
                {
                    opzioni.SetAbsoluteExpiration(durata.Value);
                }
                else
                {
                    opzioni.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                }

                opzioni.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _logger.LogInformation($"Entry cache rimossa: {key}, motivo: {reason}");
                });

                _memoryCache.Set(chiave, valore, opzioni);

                _logger.LogInformation($"Cache SET per chiave: {chiave}, durata: {durata ?? TimeSpan.FromMinutes(30)}");

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Cache impostata con successo",
                    Chiave = chiave,
                    Scadenza = DateTime.Now.Add(durata ?? TimeSpan.FromMinutes(30)),
                    DurataCache = durata ?? TimeSpan.FromMinutes(30),
                    DimensioneBytes = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'impostazione cache per chiave: {chiave}");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = chiave
                };
            }
        }

        public async Task<CacheOperationResultDTO> RemoveAsync(string chiave)
        {
            try
            {
                _memoryCache.Remove(chiave);
                _logger.LogInformation($"Cache REMOVE per chiave: {chiave}");

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Cache rimossa con successo",
                    Chiave = chiave
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nella rimozione cache per chiave: {chiave}");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = chiave
                };
            }
        }

        public async Task<bool> ExistsAsync(string chiave)
        {
            return _memoryCache.TryGetValue(chiave, out _);
        }

        public async Task<DateTime?> GetExpirationAsync(string chiave)
        {
            // IMemoryCache non espone direttamente le scadenze
            return await Task.FromResult<DateTime?>(null);
        }

        public async Task<CacheBulkResultDTO> GetBulkAsync(List<string> chiavi)
        {
            var risultato = new CacheBulkResultDTO();
            var startTime = DateTime.Now;

            try
            {
                foreach (var chiave in chiavi)
                {
                    try
                    {
                        if (_memoryCache.TryGetValue(chiave, out var valore))
                        {
                            risultato.Risultati[chiave] = valore ?? "null";
                            risultato.OperazioniCompletate++;
                        }
                        else
                        {
                            risultato.Risultati[chiave] = "MISS";
                            risultato.OperazioniFallite++;
                        }
                        risultato.ChiaviProcessate.Add(chiave);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore nel bulk get per chiave: {chiave}");
                        risultato.Risultati[chiave] = $"ERROR: {ex.Message}";
                        risultato.OperazioniFallite++;
                    }
                }

                risultato.TempoEsecuzione = DateTime.Now - startTime;
                _logger.LogInformation($"Bulk GET completato: {risultato.OperazioniCompletate} successi, {risultato.OperazioniFallite} falliti");

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk get");
                risultato.TempoEsecuzione = DateTime.Now - startTime;
                return risultato;
            }
        }

        public async Task<CacheBulkResultDTO> SetBulkAsync<T>(Dictionary<string, T> valori, TimeSpan? durata = null)
        {
            var risultato = new CacheBulkResultDTO();
            var startTime = DateTime.Now;

            try
            {
                foreach (var (chiave, valore) in valori)
                {
                    try
                    {
                        await SetAsync(chiave, valore, durata);
                        risultato.OperazioniCompletate++;
                        risultato.ChiaviProcessate.Add(chiave);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore nel bulk set per chiave: {chiave}");
                        risultato.OperazioniFallite++;
                    }
                }

                risultato.TempoEsecuzione = DateTime.Now - startTime;
                _logger.LogInformation($"Bulk SET completato: {risultato.OperazioniCompletate} successi, {risultato.OperazioniFallite} falliti");

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk set");
                risultato.TempoEsecuzione = DateTime.Now - startTime;
                return risultato;
            }
        }

        public async Task<CacheBulkResultDTO> RemoveBulkAsync(List<string> chiavi)
        {
            var risultato = new CacheBulkResultDTO();
            var startTime = DateTime.Now;

            try
            {
                foreach (var chiave in chiavi)
                {
                    try
                    {
                        await RemoveAsync(chiave);
                        risultato.OperazioniCompletate++;
                        risultato.ChiaviProcessate.Add(chiave);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore nel bulk remove per chiave: {chiave}");
                        risultato.OperazioniFallite++;
                    }
                }

                risultato.TempoEsecuzione = DateTime.Now - startTime;
                _logger.LogInformation($"Bulk REMOVE completato: {risultato.OperazioniCompletate} successi, {risultato.OperazioniFallite} falliti");

                return risultato;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'operazione bulk remove");
                risultato.TempoEsecuzione = DateTime.Now - startTime;
                return risultato;
            }
        }

        public async Task<CacheInfoDTO> GetCacheInfoAsync()
        {
            var hitRate = (_totalHits + _totalMisses) > 0
                ? Math.Round((decimal)_totalHits / (_totalHits + _totalMisses) * 100, 2)
                : 0;

            return new CacheInfoDTO
            {
                TotaleEntry = -1, // Non disponibile con IMemoryCache
                DimensioneTotaleBytes = -1,
                EntryScadute = -1,
                HitsTotali = (int)_totalHits,
                MissesTotali = (int)_totalMisses,
                HitRatePercentuale = hitRate,
                UltimaPulizia = _lastCleanup,
                ChiaviAttive = new List<string>(),
                StatistichePerTipo = new Dictionary<string, int>()
            };
        }

        public async Task<CacheCleanupDTO> CleanupExpiredAsync()
        {
            var primaPulizia = _lastCleanup;
            _lastCleanup = DateTime.Now;

            _logger.LogInformation("Pulizia cache eseguita (gestita automaticamente da IMemoryCache)");

            return new CacheCleanupDTO
            {
                EntryRimosse = 0,
                BytesLiberati = 0,
                EntryScadute = 0,
                TempoEsecuzione = DateTime.Now - primaPulizia,
                DataPulizia = _lastCleanup
            };
        }

        public async Task<CacheOperationResultDTO> UpdateExpirationAsync(string chiave, TimeSpan durata)
        {
            try
            {
                if (_memoryCache.TryGetValue(chiave, out var valore))
                {
                    await RemoveAsync(chiave);
                    await SetAsync(chiave, valore, durata);

                    return new CacheOperationResultDTO
                    {
                        Successo = true,
                        Messaggio = "Scadenza aggiornata con successo",
                        Chiave = chiave,
                        Scadenza = DateTime.Now.Add(durata),
                        DurataCache = durata
                    };
                }

                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = "Chiave non trovata in cache",
                    Chiave = chiave
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento scadenza per chiave: {chiave}");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = chiave
                };
            }
        }

        public async Task<List<CacheEntryDTO>> GetAllEntriesAsync()
        {
            _logger.LogWarning("Enumerazione entries cache non supportata con IMemoryCache");
            return new List<CacheEntryDTO>();
        }

        public async Task<T> GetOrSetAsync<T>(string chiave, Func<Task<T>> factory, TimeSpan? durata = null)
        {
            var valore = await GetAsync<T>(chiave);
            if (valore != null && !valore.Equals(default(T)))
            {
                return valore;
            }

            var nuovoValore = await factory();
            await SetAsync(chiave, nuovoValore, durata);
            return nuovoValore;
        }

        public async Task<T> GetOrSetAsync<T>(string chiave, Func<T> factory, TimeSpan? durata = null)
        {
            var valore = await GetAsync<T>(chiave);
            if (valore != null && !valore.Equals(default(T)))
            {
                return valore;
            }

            var nuovoValore = factory();
            await SetAsync(chiave, nuovoValore, durata);
            return nuovoValore;
        }

        public async Task<bool> RefreshAsync(string chiave, TimeSpan? nuovaDurata = null)
        {
            try
            {
                if (_memoryCache.TryGetValue(chiave, out var valore))
                {
                    await RemoveAsync(chiave);
                    await SetAsync(chiave, valore, nuovaDurata);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel refresh cache per chiave: {chiave}");
                return false;
            }
        }

        public async Task<CacheOperationResultDTO> CacheMenuAsync()
        {
            try
            {
                // Carica menu completo dal database
                var bevandeStandard = await _context.BevandaStandard
                    .Where(bs => bs.Disponibile)
                    .Select(bs => new
                    {
                        bs.ArticoloId,
                        bs.Prezzo,
                        bs.DimensioneBicchiere.Descrizione
                    })
                    .ToListAsync();

                var dolci = await _context.Dolce
                    .Where(d => d.Disponibile)
                    .Select(d => new
                    {
                        d.ArticoloId,
                        d.Nome,
                        d.Prezzo
                    })
                    .ToListAsync();

                var menuData = new
                {
                    UltimoAggiornamento = DateTime.Now,
                    BevandeStandard = bevandeStandard,
                    Dolci = dolci,
                    TotaleVoci = bevandeStandard.Count + dolci.Count
                };

                await SetAsync(MENU_CACHE_KEY, menuData, TimeSpan.FromHours(1));

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Menu cached con successo",
                    Chiave = MENU_CACHE_KEY,
                    Scadenza = DateTime.Now.AddHours(1),
                    DurataCache = TimeSpan.FromHours(1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caching menu");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = MENU_CACHE_KEY
                };
            }
        }

        public async Task<CacheOperationResultDTO> CacheStatisticheAsync()
        {
            try
            {
                var statistiche = await _context.StatisticheCache
                    .OrderByDescending(s => s.DataAggiornamento)
                    .FirstOrDefaultAsync();

                await SetAsync(STATISTICHE_CACHE_KEY, statistiche, TimeSpan.FromMinutes(15));

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Statistiche cached con successo",
                    Chiave = STATISTICHE_CACHE_KEY,
                    Scadenza = DateTime.Now.AddMinutes(15),
                    DurataCache = TimeSpan.FromMinutes(15)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caching statistiche");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = STATISTICHE_CACHE_KEY
                };
            }
        }

        public async Task<CacheOperationResultDTO> CachePrezziAsync()
        {
            try
            {
                // Carica prezzi base e configurazioni
                var dimensioni = await _context.DimensioneBicchiere
                    .Select(db => new
                    {
                        db.DimensioneBicchiereId,
                        db.Descrizione,
                        db.PrezzoBase
                    })
                    .ToListAsync();

                var ingredienti = await _context.Ingrediente
                    .Where(i => i.Disponibile)
                    .Select(i => new
                    {
                        i.IngredienteId,
                        i.Ingrediente1,
                        i.PrezzoAggiunto
                    })
                    .ToListAsync();

                var prezziData = new
                {
                    Dimensioni = dimensioni,
                    Ingredienti = ingredienti,
                    UltimoRicalcolo = DateTime.Now
                };

                await SetAsync(PREZZI_CACHE_KEY, prezziData, TimeSpan.FromMinutes(30));

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Prezzi cached con successo",
                    Chiave = PREZZI_CACHE_KEY,
                    Scadenza = DateTime.Now.AddMinutes(30),
                    DurataCache = TimeSpan.FromMinutes(30)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caching prezzi");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = PREZZI_CACHE_KEY
                };
            }
        }

        public async Task<CacheOperationResultDTO> CacheConfigurazioniAsync()
        {
            try
            {
                var configurazioni = await _context.ConfigSoglieTempi
                    .ToListAsync(); // Rimosso .Where(c => c.Attivo)

                await SetAsync(CONFIG_CACHE_KEY, configurazioni, TimeSpan.FromHours(2));

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Configurazioni cached con successo",
                    Chiave = CONFIG_CACHE_KEY,
                    Scadenza = DateTime.Now.AddHours(2),
                    DurataCache = TimeSpan.FromHours(2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caching configurazioni");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = CONFIG_CACHE_KEY
                };
            }
        }

        public async Task<CachePerformanceDTO> GetPerformanceStatsAsync()
        {
            var hitRate = (_totalHits + _totalMisses) > 0
                ? Math.Round((decimal)_totalHits / (_totalHits + _totalMisses) * 100, 2)
                : 0;

            return new CachePerformanceDTO
            {
                HitRate = hitRate,
                MissRate = 100 - hitRate,
                MemoriaUtilizzataPercentuale = 0,
                MemoriaTotaleBytes = -1,
                MemoriaLiberaBytes = -1,
                EntryAttive = -1,
                TempoMedioAccesso = TimeSpan.Zero,
                DataRaccolta = DateTime.Now
            };
        }

        public async Task<List<CacheStatisticheDTO>> GetCacheStatisticsAsync()
        {
            try
            {
                var statistiche = await _context.StatisticheCache
                    .OrderByDescending(s => s.DataAggiornamento)
                    .Take(10)
                    .Select(s => new CacheStatisticheDTO
                    {
                        StatisticheCacheId = s.Id,
                        TipoStatistica = s.TipoStatistica,
                        DatiCache = s.Metriche, // Usa Metriche invece di DatiCache
                        DataAggiornamento = s.DataAggiornamento ?? DateTime.MinValue,
                        ScadenzaCache = DateTime.Now.AddHours(1), // Valore default poiché non presente nell'entità
                        DimensioneBytes = 0, // Valore default poiché non presente nell'entità
                        Hits = 0, // Valore default poiché non presente nell'entità
                        Misses = 0, // Valore default poiché non presente nell'entità
                        HitRate = 0 // Valore default poiché non presente nell'entità
                    })
                    .ToListAsync();

                return statistiche;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero statistiche cache");
                return new List<CacheStatisticheDTO>();
            }
        }

        public async Task ResetStatisticsAsync()
        {
            _totalHits = 0;
            _totalMisses = 0;
            _logger.LogInformation("Statistiche cache resetate");
        }

        public async Task<CacheOperationResultDTO> CompactCacheAsync()
        {
            _logger.LogInformation("Compattazione cache richiesta (gestita automaticamente)");

            return new CacheOperationResultDTO
            {
                Successo = true,
                Messaggio = "Compattazione completata",
                Chiave = "SYSTEM_COMPACT"
            };
        }

        public async Task<long> GetMemoryUsageAsync()
        {
            return -1; // Non disponibile con IMemoryCache
        }

        public async Task<bool> IsMemoryCriticalAsync()
        {
            return false; // Implementazione semplificata
        }

        public async Task<bool> IsCacheValidAsync(string tipoCache)
        {
            return tipoCache switch
            {
                "MENU" => await ExistsAsync(MENU_CACHE_KEY),
                "STATISTICHE" => await ExistsAsync(STATISTICHE_CACHE_KEY),
                "PREZZI" => await ExistsAsync(PREZZI_CACHE_KEY),
                "CONFIG" => await ExistsAsync(CONFIG_CACHE_KEY),
                _ => false
            };
        }

        public async Task<CacheOperationResultDTO> PreloadCommonDataAsync()
        {
            try
            {
                var tasks = new[]
                {
                    CacheMenuAsync(),
                    CacheStatisticheAsync(),
                    CachePrezziAsync(),
                    CacheConfigurazioniAsync()
                };

                await Task.WhenAll(tasks);

                return new CacheOperationResultDTO
                {
                    Successo = true,
                    Messaggio = "Dati comuni precaricati in cache",
                    Chiave = "PRELOAD_ALL"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel precaricamento dati comuni");
                return new CacheOperationResultDTO
                {
                    Successo = false,
                    Messaggio = $"Errore: {ex.Message}",
                    Chiave = "PRELOAD_ALL"
                };
            }
        }

        public async Task ClearAllAsync()
        {
            var chiaviConosciute = new[] { MENU_CACHE_KEY, STATISTICHE_CACHE_KEY, PREZZI_CACHE_KEY, CONFIG_CACHE_KEY };

            foreach (var chiave in chiaviConosciute)
            {
                await RemoveAsync(chiave);
            }

            await ResetStatisticsAsync();
            _logger.LogInformation("Cache pulita completamente");
        }
    }
}