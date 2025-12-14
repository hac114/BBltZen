using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Text.Json;

namespace Repository.Service
{
    public class StatisticheCacheRepository : IStatisticheCacheRepository
    {
        private readonly BubbleTeaContext _context;

        public StatisticheCacheRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private StatisticheCacheDTO MapToDTO(StatisticheCache statisticheCache)
        {
            return new StatisticheCacheDTO
            {
                Id = statisticheCache.Id,
                TipoStatistica = statisticheCache.TipoStatistica,
                Periodo = statisticheCache.Periodo,
                Metriche = statisticheCache.Metriche,
                DataAggiornamento = statisticheCache.DataAggiornamento
            };
        }

        public async Task<IEnumerable<StatisticheCacheDTO>> GetAllAsync()
        {
            return await _context.StatisticheCache
                .AsNoTracking()
                .OrderByDescending(s => s.DataAggiornamento)
                .ThenBy(s => s.TipoStatistica)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<StatisticheCacheDTO?> GetByIdAsync(int id)
        {
            var statisticheCache = await _context.StatisticheCache
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            return statisticheCache == null ? null : MapToDTO(statisticheCache);
        }

        public async Task<StatisticheCacheDTO?> GetByTipoAndPeriodoAsync(string tipoStatistica, string periodo)
        {
            var statisticheCache = await _context.StatisticheCache
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TipoStatistica == tipoStatistica && s.Periodo == periodo);

            return statisticheCache == null ? null : MapToDTO(statisticheCache);
        }

        public async Task<IEnumerable<StatisticheCacheDTO>> GetByTipoAsync(string tipoStatistica)
        {
            return await _context.StatisticheCache
                .AsNoTracking()
                .Where(s => s.TipoStatistica == tipoStatistica)
                .OrderByDescending(s => s.DataAggiornamento)
                .ThenBy(s => s.Periodo)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<StatisticheCacheDTO> AddAsync(StatisticheCacheDTO statisticheCacheDto)
        {
            if (statisticheCacheDto == null)
                throw new ArgumentNullException(nameof(statisticheCacheDto));

            // ✅ VERIFICA UNICITÀ COMBINAZIONE
            var cacheEsistente = await GetByTipoAndPeriodoAsync(statisticheCacheDto.TipoStatistica, statisticheCacheDto.Periodo);
            if (cacheEsistente != null)
            {
                throw new ArgumentException($"Esiste già una cache per questa combinazione tipo/periodo");
            }

            var statisticheCache = new StatisticheCache
            {
                // ✅ NON impostare Id - sarà generato automaticamente
                TipoStatistica = statisticheCacheDto.TipoStatistica,
                Periodo = statisticheCacheDto.Periodo,
                Metriche = statisticheCacheDto.Metriche,
                DataAggiornamento = DateTime.UtcNow // ✅ UTC per consistenza
            };

            _context.StatisticheCache.Add(statisticheCache);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            statisticheCacheDto.Id = statisticheCache.Id;
            statisticheCacheDto.DataAggiornamento = statisticheCache.DataAggiornamento;
            return statisticheCacheDto;
        }

        public async Task UpdateAsync(StatisticheCacheDTO statisticheCacheDto)
        {
            var statisticheCache = await _context.StatisticheCache
                .FirstOrDefaultAsync(s => s.Id == statisticheCacheDto.Id);

            if (statisticheCache == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ COMBINAZIONE (escludendo il record corrente)
            var cacheDuplicata = await _context.StatisticheCache
                .AnyAsync(s => s.TipoStatistica == statisticheCacheDto.TipoStatistica &&
                             s.Periodo == statisticheCacheDto.Periodo &&
                             s.Id != statisticheCacheDto.Id);

            if (cacheDuplicata)
            {
                throw new ArgumentException($"Esiste già un'altra cache per questa combinazione tipo/periodo");
            }

            statisticheCache.TipoStatistica = statisticheCacheDto.TipoStatistica;
            statisticheCache.Periodo = statisticheCacheDto.Periodo;
            statisticheCache.Metriche = statisticheCacheDto.Metriche;
            statisticheCache.DataAggiornamento = DateTime.UtcNow; // ✅ UTC

            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DATA NEL DTO
            statisticheCacheDto.DataAggiornamento = statisticheCache.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var statisticheCache = await _context.StatisticheCache
                .FirstOrDefaultAsync(s => s.Id == id);

            if (statisticheCache != null)
            {
                _context.StatisticheCache.Remove(statisticheCache);
                await _context.SaveChangesAsync();
            }
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.StatisticheCache
                .AnyAsync(s => s.Id == id);
        }

        public async Task AggiornaCacheAsync(string tipoStatistica, string periodo, string metriche)
        {
            var cacheEsistente = await _context.StatisticheCache
                .FirstOrDefaultAsync(s => s.TipoStatistica == tipoStatistica && s.Periodo == periodo);

            if (cacheEsistente != null)
            {
                // Aggiorna cache esistente
                cacheEsistente.Metriche = metriche;
                cacheEsistente.DataAggiornamento = DateTime.UtcNow; // ✅ UTC
            }
            else
            {
                // Crea nuova cache
                var nuovaCache = new StatisticheCache
                {
                    TipoStatistica = tipoStatistica,
                    Periodo = periodo,
                    Metriche = metriche,
                    DataAggiornamento = DateTime.UtcNow // ✅ UTC
                };
                _context.StatisticheCache.Add(nuovaCache);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsCacheValidaAsync(string tipoStatistica, string periodo, TimeSpan validita)
        {
            var cache = await _context.StatisticheCache
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TipoStatistica == tipoStatistica && s.Periodo == periodo);

            if (cache == null)
                return false;

            var tempoTrascorso = DateTime.UtcNow - cache.DataAggiornamento; // ✅ UTC
            return tempoTrascorso <= validita;
        }

        // Metodi specifici per statistiche carrello - CACHE PERSISTENTE
        public async Task<StatisticheCarrelloDTO?> GetStatisticheCarrelloByPeriodoAsync(string periodo)
        {
            var cache = await GetByTipoAndPeriodoAsync("CarrelloComprehensive", periodo);
            return cache == null ? null : JsonSerializer.Deserialize<StatisticheCarrelloDTO>(cache.Metriche);
        }

        public async Task SalvaStatisticheCarrelloAsync(string periodo, StatisticheCarrelloDTO statistiche)
        {
            var metricheJson = JsonSerializer.Serialize(statistiche);
            await AggiornaCacheAsync("CarrelloComprehensive", periodo, metricheJson);
        }

        public async Task<bool> IsStatisticheCarrelloValideAsync(string periodo, TimeSpan validita)
        {
            return await IsCacheValidaAsync("CarrelloComprehensive", periodo, validita);
        }

        public async Task<IEnumerable<string>> GetPeriodiDisponibiliCarrelloAsync()
        {
            return await _context.StatisticheCache
                .Where(s => s.TipoStatistica == "CarrelloComprehensive")
                .Select(s => s.Periodo)
                .OrderByDescending(p => p)
                .ToListAsync();
        }
    }
}
