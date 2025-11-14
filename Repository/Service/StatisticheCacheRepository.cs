using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class StatisticheCacheRepository : IStatisticheCacheRepository
    {
        private readonly BubbleTeaContext _context;

        public StatisticheCacheRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StatisticheCacheDTO>> GetAllAsync()
        {
            return await _context.StatisticheCache
                .AsNoTracking()
                .Select(s => new StatisticheCacheDTO
                {
                    Id = s.Id,
                    TipoStatistica = s.TipoStatistica,
                    Periodo = s.Periodo,
                    Metriche = s.Metriche,
                    DataAggiornamento = s.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<StatisticheCacheDTO?> GetByIdAsync(int id)
        {
            var statisticheCache = await _context.StatisticheCache
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (statisticheCache == null) return null;

            return new StatisticheCacheDTO
            {
                Id = statisticheCache.Id,
                TipoStatistica = statisticheCache.TipoStatistica,
                Periodo = statisticheCache.Periodo,
                Metriche = statisticheCache.Metriche,
                DataAggiornamento = statisticheCache.DataAggiornamento
            };
        }

        public async Task<StatisticheCacheDTO?> GetByTipoAndPeriodoAsync(string tipoStatistica, string periodo)
        {
            var statisticheCache = await _context.StatisticheCache
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TipoStatistica == tipoStatistica && s.Periodo == periodo);

            if (statisticheCache == null) return null;

            return new StatisticheCacheDTO
            {
                Id = statisticheCache.Id,
                TipoStatistica = statisticheCache.TipoStatistica,
                Periodo = statisticheCache.Periodo,
                Metriche = statisticheCache.Metriche,
                DataAggiornamento = statisticheCache.DataAggiornamento
            };
        }

        public async Task<IEnumerable<StatisticheCacheDTO>> GetByTipoAsync(string tipoStatistica)
        {
            return await _context.StatisticheCache
                .AsNoTracking()
                .Where(s => s.TipoStatistica == tipoStatistica)
                .Select(s => new StatisticheCacheDTO
                {
                    Id = s.Id,
                    TipoStatistica = s.TipoStatistica,
                    Periodo = s.Periodo,
                    Metriche = s.Metriche,
                    DataAggiornamento = s.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task AddAsync(StatisticheCacheDTO statisticheCacheDto)
        {
            var statisticheCache = new StatisticheCache
            {
                TipoStatistica = statisticheCacheDto.TipoStatistica,
                Periodo = statisticheCacheDto.Periodo,
                Metriche = statisticheCacheDto.Metriche,
                DataAggiornamento = DateTime.Now
            };

            _context.StatisticheCache.Add(statisticheCache);
            await _context.SaveChangesAsync();

            statisticheCacheDto.Id = statisticheCache.Id;
            statisticheCacheDto.DataAggiornamento = statisticheCache.DataAggiornamento;
        }

        public async Task UpdateAsync(StatisticheCacheDTO statisticheCacheDto)
        {
            var statisticheCache = await _context.StatisticheCache
                .FirstOrDefaultAsync(s => s.Id == statisticheCacheDto.Id);

            if (statisticheCache == null)
                throw new ArgumentException($"Statistiche cache con ID {statisticheCacheDto.Id} non trovato");

            statisticheCache.TipoStatistica = statisticheCacheDto.TipoStatistica;
            statisticheCache.Periodo = statisticheCacheDto.Periodo;
            statisticheCache.Metriche = statisticheCacheDto.Metriche;
            statisticheCache.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

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
                cacheEsistente.DataAggiornamento = DateTime.Now;
            }
            else
            {
                // Crea nuova cache
                var nuovaCache = new StatisticheCache
                {
                    TipoStatistica = tipoStatistica,
                    Periodo = periodo,
                    Metriche = metriche,
                    DataAggiornamento = DateTime.Now
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
                        
            var tempoTrascorso = DateTime.Now - cache.DataAggiornamento; //rigo 177 corretto
            return tempoTrascorso <= validita;
        }
    }
}
