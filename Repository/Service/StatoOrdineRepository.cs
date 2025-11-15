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
    public class StatoOrdineRepository : IStatoOrdineRepository
    {
        private readonly BubbleTeaContext _context;

        public StatoOrdineRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StatoOrdineDTO>> GetAllAsync()
        {
            return await _context.StatoOrdine
                .AsNoTracking()
                .Select(s => new StatoOrdineDTO
                {
                    StatoOrdineId = s.StatoOrdineId,
                    StatoOrdine1 = s.StatoOrdine1,
                    Terminale = s.Terminale
                })
                .ToListAsync();
        }

        public async Task<StatoOrdineDTO?> GetByIdAsync(int statoOrdineId)
        {
            var statoOrdine = await _context.StatoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineId);

            if (statoOrdine == null) return null;

            return new StatoOrdineDTO
            {
                StatoOrdineId = statoOrdine.StatoOrdineId,
                StatoOrdine1 = statoOrdine.StatoOrdine1,
                Terminale = statoOrdine.Terminale
            };
        }

        public async Task<StatoOrdineDTO?> GetByNomeAsync(string nomeStatoOrdine)
        {
            var statoOrdine = await _context.StatoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StatoOrdine1 == nomeStatoOrdine);

            if (statoOrdine == null) return null;

            return new StatoOrdineDTO
            {
                StatoOrdineId = statoOrdine.StatoOrdineId,
                StatoOrdine1 = statoOrdine.StatoOrdine1,
                Terminale = statoOrdine.Terminale
            };
        }

        public async Task<IEnumerable<StatoOrdineDTO>> GetStatiTerminaliAsync()
        {
            return await _context.StatoOrdine
                .AsNoTracking()
                .Where(s => s.Terminale)
                .Select(s => new StatoOrdineDTO
                {
                    StatoOrdineId = s.StatoOrdineId,
                    StatoOrdine1 = s.StatoOrdine1,
                    Terminale = s.Terminale
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<StatoOrdineDTO>> GetStatiNonTerminaliAsync()
        {
            return await _context.StatoOrdine
                .AsNoTracking()
                .Where(s => !s.Terminale)
                .Select(s => new StatoOrdineDTO
                {
                    StatoOrdineId = s.StatoOrdineId,
                    StatoOrdine1 = s.StatoOrdine1,
                    Terminale = s.Terminale
                })
                .ToListAsync();
        }

        public async Task<StatoOrdineDTO> AddAsync(StatoOrdineDTO statoOrdineDto) // ✅ CORRETTO: Ritorna DTO
        {
            if (statoOrdineDto == null)
                throw new ArgumentNullException(nameof(statoOrdineDto));

            var statoOrdine = new StatoOrdine
            {
                StatoOrdine1 = statoOrdineDto.StatoOrdine1,
                Terminale = statoOrdineDto.Terminale
            };

            await _context.StatoOrdine.AddAsync(statoOrdine);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA IL DTO CON L'ID GENERATO
            statoOrdineDto.StatoOrdineId = statoOrdine.StatoOrdineId;
            return statoOrdineDto;
        }

        public async Task UpdateAsync(StatoOrdineDTO statoOrdineDto)
        {
            if (statoOrdineDto == null)
                throw new ArgumentNullException(nameof(statoOrdineDto));

            var statoOrdine = await _context.StatoOrdine
                .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineDto.StatoOrdineId);

            if (statoOrdine == null)
                throw new ArgumentException($"Stato ordine con ID {statoOrdineDto.StatoOrdineId} non trovato");

            statoOrdine.StatoOrdine1 = statoOrdineDto.StatoOrdine1;
            statoOrdine.Terminale = statoOrdineDto.Terminale;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int statoOrdineId)
        {
            var statoOrdine = await _context.StatoOrdine
                .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineId);

            if (statoOrdine != null)
            {
                _context.StatoOrdine.Remove(statoOrdine);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int statoOrdineId)
        {
            return await _context.StatoOrdine
                .AnyAsync(s => s.StatoOrdineId == statoOrdineId);
        }
    }
}