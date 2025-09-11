using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                .Select(s => new StatoOrdineDTO
                {
                    StatoOrdineId = s.StatoOrdineId,
                    StatoOrdine1 = s.StatoOrdine1, // Note the different property name
                    Terminale = s.Terminale
                    // Map all other properties from StatoOrdine entity to StatoOrdineDTO
                })
                .ToListAsync();
        }

        public async Task<StatoOrdineDTO> GetByIdAsync(int statoOrdineId)
        {
            var statoOrdine = await _context.StatoOrdine.FindAsync(statoOrdineId);
            if (statoOrdine == null) return null;

            return new StatoOrdineDTO
            {
                StatoOrdineId = statoOrdine.StatoOrdineId,
                StatoOrdine1 = statoOrdine.StatoOrdine1, // Note the different property name
                Terminale = statoOrdine.Terminale
                // Map all other properties
            };
        }

        public async Task<StatoOrdineDTO> GetByNomeAsync(string nomeStatoOrdine)
        {
            var statoOrdine = await _context.StatoOrdine
                .FirstOrDefaultAsync(s => s.StatoOrdine1 == nomeStatoOrdine);

            if (statoOrdine == null) return null;

            return new StatoOrdineDTO
            {
                StatoOrdineId = statoOrdine.StatoOrdineId,
                StatoOrdine1 = statoOrdine.StatoOrdine1, // Note the different property name
                Terminale = statoOrdine.Terminale
                // Map all other properties
            };
        }

        public async Task<IEnumerable<StatoOrdineDTO>> GetStatiTerminaliAsync()
        {
            return await _context.StatoOrdine
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
                .Where(s => !s.Terminale)
                .Select(s => new StatoOrdineDTO
                {
                    StatoOrdineId = s.StatoOrdineId,
                    StatoOrdine1 = s.StatoOrdine1,
                    Terminale = s.Terminale
                })
                .ToListAsync();
        }

        public async Task AddAsync(StatoOrdineDTO statoOrdineDto)
        {
            var statoOrdine = new StatoOrdine
            {
                StatoOrdine1 = statoOrdineDto.StatoOrdine1, // Note the different property name
                Terminale = statoOrdineDto.Terminale
                // Map all other properties from DTO to entity
            };

            await _context.StatoOrdine.AddAsync(statoOrdine);
            await _context.SaveChangesAsync();

            // Return the generated ID to the DTO
            statoOrdineDto.StatoOrdineId = statoOrdine.StatoOrdineId;
        }

        public async Task UpdateAsync(StatoOrdineDTO statoOrdineDto)
        {
            var statoOrdine = await _context.StatoOrdine.FindAsync(statoOrdineDto.StatoOrdineId);
            if (statoOrdine == null)
                throw new ArgumentException("Stato ordine not found");

            statoOrdine.StatoOrdine1 = statoOrdineDto.StatoOrdine1; // Note the different property name
            statoOrdine.Terminale = statoOrdineDto.Terminale;
            // Update all other properties

            _context.StatoOrdine.Update(statoOrdine);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int statoOrdineId)
        {
            var statoOrdine = await _context.StatoOrdine.FindAsync(statoOrdineId);
            if (statoOrdine != null)
            {
                _context.StatoOrdine.Remove(statoOrdine);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int statoOrdineId)
        {
            return await _context.StatoOrdine.AnyAsync(s => s.StatoOrdineId == statoOrdineId);
        }
    }
}
