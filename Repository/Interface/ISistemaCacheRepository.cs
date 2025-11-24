using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ISistemaCacheRepository
    {
        // ✅ CORRETTO: Pattern già allineato per cache operations

        // Operazioni Base Cache
        Task<T?> GetAsync<T>(string chiave);
        Task<CacheOperationResultDTO> SetAsync<T>(string chiave, T valore, TimeSpan? durata = null);
        Task<CacheOperationResultDTO> RemoveAsync(string chiave);
        Task<bool> ExistsAsync(string chiave);
        Task<DateTime?> GetExpirationAsync(string chiave);

        // Operazioni Bulk
        Task<CacheBulkResultDTO> GetBulkAsync(List<string> chiavi);
        Task<CacheBulkResultDTO> SetBulkAsync<T>(Dictionary<string, T> valori, TimeSpan? durata = null);
        Task<CacheBulkResultDTO> RemoveBulkAsync(List<string> chiavi);

        // Gestione Cache Avanzata
        Task<CacheInfoDTO> GetCacheInfoAsync();
        Task<CacheCleanupDTO> CleanupExpiredAsync();
        Task<CacheOperationResultDTO> UpdateExpirationAsync(string chiave, TimeSpan durata);
        Task<List<CacheEntryDTO>> GetAllEntriesAsync();

        // Cache Pattern Specializzati
        Task<T> GetOrSetAsync<T>(string chiave, Func<Task<T>> factory, TimeSpan? durata = null);
        Task<T> GetOrSetAsync<T>(string chiave, Func<T> factory, TimeSpan? durata = null);
        Task<bool> RefreshAsync(string chiave, TimeSpan? nuovaDurata = null);

        // Cache per Dati Specifici
        Task<CacheOperationResultDTO> CacheMenuAsync();
        Task<CacheOperationResultDTO> CacheStatisticheAsync();
        Task<CacheOperationResultDTO> CachePrezziAsync();
        Task<CacheOperationResultDTO> CacheConfigurazioniAsync();

        // ✅ NUOVI METODI PER STATISTICHE CARRELLO - CACHE MEMORIA
        Task<StatisticheCarrelloDTO> GetStatisticheCarrelloRealtimeAsync();
        Task<StatisticheCarrelloDTO> GetStatisticheCarrelloUltimoPeriodoAsync();
        Task<CacheOperationResultDTO> CacheStatisticheCarrelloAsync();
        Task<bool> IsStatisticheCarrelloRealtimeValideAsync();
        Task<CacheOperationResultDTO> RefreshStatisticheCarrelloAsync();

        // Statistiche e Monitoring
        Task<CachePerformanceDTO> GetPerformanceStatsAsync();
        Task<List<CacheStatisticheDTO>> GetCacheStatisticsAsync();
        Task ResetStatisticsAsync();

        // Gestione Memoria
        Task<CacheOperationResultDTO> CompactCacheAsync();
        Task<long> GetMemoryUsageAsync();
        Task<bool> IsMemoryCriticalAsync();

        // Utility
        Task<bool> IsCacheValidAsync(string tipoCache);
        Task<CacheOperationResultDTO> PreloadCommonDataAsync();
        Task ClearAllAsync();
    }
}