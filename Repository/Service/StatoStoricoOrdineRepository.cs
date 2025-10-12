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
    public class StatoStoricoOrdineRepository : IStatoStoricoOrdineRepository
    {
        private readonly BubbleTeaContext _context;

        public StatoStoricoOrdineRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetAllAsync()
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Select(s => new StatoStoricoOrdineDTO
                {
                    StatoStoricoOrdineId = s.StatoStoricoOrdineId,
                    OrdineId = s.OrdineId,
                    StatoOrdineId = s.StatoOrdineId,
                    Inizio = s.Inizio,
                    Fine = s.Fine
                })
                .ToListAsync();
        }

        public async Task<StatoStoricoOrdineDTO?> GetByIdAsync(int statoStoricoOrdineId)
        {
            var statoStoricoOrdine = await _context.StatoStoricoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineId);

            if (statoStoricoOrdine == null) return null;

            return new StatoStoricoOrdineDTO
            {
                StatoStoricoOrdineId = statoStoricoOrdine.StatoStoricoOrdineId,
                OrdineId = statoStoricoOrdine.OrdineId,
                StatoOrdineId = statoStoricoOrdine.StatoOrdineId,
                Inizio = statoStoricoOrdine.Inizio,
                Fine = statoStoricoOrdine.Fine
            };
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetByOrdineIdAsync(int ordineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId)
                .OrderBy(s => s.Inizio)
                .Select(s => new StatoStoricoOrdineDTO
                {
                    StatoStoricoOrdineId = s.StatoStoricoOrdineId,
                    OrdineId = s.OrdineId,
                    StatoOrdineId = s.StatoOrdineId,
                    Inizio = s.Inizio,
                    Fine = s.Fine
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.StatoOrdineId == statoOrdineId)
                .OrderBy(s => s.Inizio)
                .Select(s => new StatoStoricoOrdineDTO
                {
                    StatoStoricoOrdineId = s.StatoStoricoOrdineId,
                    OrdineId = s.OrdineId,
                    StatoOrdineId = s.StatoOrdineId,
                    Inizio = s.Inizio,
                    Fine = s.Fine
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoCompletoOrdineAsync(int ordineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId)
                .OrderBy(s => s.Inizio)
                .Select(s => new StatoStoricoOrdineDTO
                {
                    StatoStoricoOrdineId = s.StatoStoricoOrdineId,
                    OrdineId = s.OrdineId,
                    StatoOrdineId = s.StatoOrdineId,
                    Inizio = s.Inizio,
                    Fine = s.Fine
                })
                .ToListAsync();
        }

        public async Task<StatoStoricoOrdineDTO?> GetStatoAttualeOrdineAsync(int ordineId)
        {
            var statoAttuale = await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId && s.Fine == null)
                .OrderByDescending(s => s.Inizio)
                .FirstOrDefaultAsync();

            if (statoAttuale == null) return null;

            return new StatoStoricoOrdineDTO
            {
                StatoStoricoOrdineId = statoAttuale.StatoStoricoOrdineId,
                OrdineId = statoAttuale.OrdineId,
                StatoOrdineId = statoAttuale.StatoOrdineId,
                Inizio = statoAttuale.Inizio,
                Fine = statoAttuale.Fine
            };
        }

        public async Task AddAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            var statoStoricoOrdine = new StatoStoricoOrdine
            {
                OrdineId = statoStoricoOrdineDto.OrdineId,
                StatoOrdineId = statoStoricoOrdineDto.StatoOrdineId,
                Inizio = statoStoricoOrdineDto.Inizio,
                Fine = statoStoricoOrdineDto.Fine
            };

            _context.StatoStoricoOrdine.Add(statoStoricoOrdine);
            await _context.SaveChangesAsync();

            statoStoricoOrdineDto.StatoStoricoOrdineId = statoStoricoOrdine.StatoStoricoOrdineId;
        }

        public async Task UpdateAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            var statoStoricoOrdine = await _context.StatoStoricoOrdine
                .FirstOrDefaultAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineDto.StatoStoricoOrdineId);

            if (statoStoricoOrdine == null)
                throw new ArgumentException($"Stato storico ordine con ID {statoStoricoOrdineDto.StatoStoricoOrdineId} non trovato");

            statoStoricoOrdine.OrdineId = statoStoricoOrdineDto.OrdineId;
            statoStoricoOrdine.StatoOrdineId = statoStoricoOrdineDto.StatoOrdineId;
            statoStoricoOrdine.Inizio = statoStoricoOrdineDto.Inizio;
            statoStoricoOrdine.Fine = statoStoricoOrdineDto.Fine;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int statoStoricoOrdineId)
        {
            var statoStoricoOrdine = await _context.StatoStoricoOrdine
                .FirstOrDefaultAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineId);

            if (statoStoricoOrdine != null)
            {
                _context.StatoStoricoOrdine.Remove(statoStoricoOrdine);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int statoStoricoOrdineId)
        {
            return await _context.StatoStoricoOrdine
                .AnyAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineId);
        }

        public async Task<bool> ChiudiStatoAttualeAsync(int ordineId, DateTime fine)
        {
            var statoAttuale = await _context.StatoStoricoOrdine
                .Where(s => s.OrdineId == ordineId && s.Fine == null)
                .OrderByDescending(s => s.Inizio)
                .FirstOrDefaultAsync();

            if (statoAttuale == null) return false;

            statoAttuale.Fine = fine;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
