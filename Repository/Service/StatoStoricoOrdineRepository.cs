using Database.Models;
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

        private StatoStoricoOrdineDTO MapToDTO(StatoStoricoOrdine statoStoricoOrdine)
        {
            return new StatoStoricoOrdineDTO
            {
                StatoStoricoOrdineId = statoStoricoOrdine.StatoStoricoOrdineId,
                OrdineId = statoStoricoOrdine.OrdineId,
                StatoOrdineId = statoStoricoOrdine.StatoOrdineId,
                Inizio = statoStoricoOrdine.Inizio,
                Fine = statoStoricoOrdine.Fine
            };
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetAllAsync()
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .OrderByDescending(s => s.Inizio)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<StatoStoricoOrdineDTO?> GetByIdAsync(int statoStoricoOrdineId)
        {
            var statoStoricoOrdine = await _context.StatoStoricoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineId);

            return statoStoricoOrdine == null ? null : MapToDTO(statoStoricoOrdine);
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetByOrdineIdAsync(int ordineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId)
                .OrderBy(s => s.Inizio)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.StatoOrdineId == statoOrdineId)
                .OrderBy(s => s.Inizio)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoCompletoOrdineAsync(int ordineId)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId)
                .OrderBy(s => s.Inizio)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }

        public async Task<StatoStoricoOrdineDTO?> GetStatoAttualeOrdineAsync(int ordineId)
        {
            var statoAttuale = await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.OrdineId == ordineId && s.Fine == null)
                .OrderByDescending(s => s.Inizio)
                .FirstOrDefaultAsync();

            return statoAttuale == null ? null : MapToDTO(statoAttuale);
        }

        public async Task<StatoStoricoOrdineDTO> AddAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            if (statoStoricoOrdineDto == null)
                throw new ArgumentNullException(nameof(statoStoricoOrdineDto));

            // ✅ VALIDAZIONE BUSINESS LOGIC
            if (!statoStoricoOrdineDto.IsValid())
                throw new ArgumentException("La data di fine non può essere precedente alla data di inizio");

            var statoStoricoOrdine = new StatoStoricoOrdine
            {
                OrdineId = statoStoricoOrdineDto.OrdineId,
                StatoOrdineId = statoStoricoOrdineDto.StatoOrdineId,
                Inizio = statoStoricoOrdineDto.Inizio,
                Fine = statoStoricoOrdineDto.Fine
            };

            _context.StatoStoricoOrdine.Add(statoStoricoOrdine);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO
            statoStoricoOrdineDto.StatoStoricoOrdineId = statoStoricoOrdine.StatoStoricoOrdineId;

            return statoStoricoOrdineDto; // ✅ IMPORTANTE: ritorna DTO
        }

        public async Task UpdateAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto)
        {
            var statoStoricoOrdine = await _context.StatoStoricoOrdine
                .FirstOrDefaultAsync(s => s.StatoStoricoOrdineId == statoStoricoOrdineDto.StatoStoricoOrdineId);

            if (statoStoricoOrdine == null)
                return; // ✅ SILENT FAIL - Non lanciare eccezione

            // ✅ VALIDAZIONE BUSINESS LOGIC
            if (!statoStoricoOrdineDto.IsValid())
                throw new ArgumentException("La data di fine non può essere precedente alla data di inizio");

            // ✅ AGGIORNA SOLO SE ESISTE
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

        public async Task<bool> OrdineHasStatoAsync(int ordineId, int statoOrdineId)
        {
            return await _context.StatoStoricoOrdine
                .AnyAsync(s => s.OrdineId == ordineId && s.StatoOrdineId == statoOrdineId);
        }

        public async Task<DateTime?> GetDataInizioStatoAsync(int ordineId, int statoOrdineId)
        {
            var stato = await _context.StatoStoricoOrdine
                .Where(s => s.OrdineId == ordineId && s.StatoOrdineId == statoOrdineId)
                .OrderBy(s => s.Inizio)
                .FirstOrDefaultAsync();

            return stato?.Inizio;
        }

        public async Task<int> GetNumeroStatiByOrdineAsync(int ordineId)
        {
            return await _context.StatoStoricoOrdine
                .CountAsync(s => s.OrdineId == ordineId);
        }

        public async Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.StatoStoricoOrdine
                .AsNoTracking()
                .Where(s => s.Inizio >= dataInizio && (s.Fine == null || s.Fine <= dataFine))
                .OrderByDescending(s => s.Inizio)
                .Select(s => MapToDTO(s))
                .ToListAsync();
        }
    }
}
