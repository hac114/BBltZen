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
    public class TavoloRepository : ITavoloRepository
    {
        private readonly BubbleTeaContext _context;
        public TavoloRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TavoloDTO>> GetAllAsync()
        {
            return await _context.Tavolo
                .Select(t => new TavoloDTO
                {
                    TavoloId = t.TavoloId,
                    Numero = t.Numero,
                    Zona = t.Zona,
                    Disponibile = t.Disponibile,
                })
                .ToListAsync();
        }

        public async Task<TavoloDTO> GetByIdAsync(int tavoloId)
        {
            var tavolo = await _context.Tavolo.FindAsync(tavoloId);
            if (tavolo == null) return null;

            return new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = tavolo.Numero,
                Zona = tavolo.Zona,
                Disponibile = tavolo.Disponibile,
            };
        }

        public async Task<TavoloDTO> GetByNumeroAsync(int numero)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.Numero == numero);

            if (tavolo == null) return null;

            return new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = tavolo.Numero,
                Zona = tavolo.Zona,
                Disponibile = tavolo.Disponibile,
            };
        }

        public async Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync()
        {
            return await _context.Tavolo
                .Where(t => t.Disponibile)
                .Select(t => new TavoloDTO
                {
                    TavoloId = t.TavoloId,
                    Numero = t.Numero,
                    Zona = t.Zona,
                    Disponibile = t.Disponibile,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona)
        {
            return await _context.Tavolo
                .Where(t => t.Zona == zona)
                .Select(t => new TavoloDTO
                {
                    TavoloId = t.TavoloId,
                    Numero = t.Numero,
                    Zona = t.Zona,
                    Disponibile = t.Disponibile,
                })
                .ToListAsync();
        }

        public async Task AddAsync(TavoloDTO tavoloDto)
        {
            var tavolo = new Tavolo
            {
                Numero = tavoloDto.Numero,
                Zona = tavoloDto.Zona,
                Disponibile = tavoloDto.Disponibile,
            };

            await _context.Tavolo.AddAsync(tavolo);
            await _context.SaveChangesAsync();

            tavoloDto.TavoloId = tavolo.TavoloId;
        }

        public async Task UpdateAsync(TavoloDTO tavoloDto)
        {
            var tavolo = await _context.Tavolo.FindAsync(tavoloDto.TavoloId);
            if (tavolo == null)
                throw new ArgumentException("Tavolo not found");

            tavolo.Numero = tavoloDto.Numero;
            tavolo.Zona = tavoloDto.Zona;
            tavolo.Disponibile = tavoloDto.Disponibile;

            _context.Tavolo.Update(tavolo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int tavoloId)
        {
            var tavolo = await _context.Tavolo.FindAsync(tavoloId);
            if (tavolo != null)
            {
                _context.Tavolo.Remove(tavolo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int tavoloId)
        {
            return await _context.Tavolo.AnyAsync(t => t.TavoloId == tavoloId);
        }

        public async Task<bool> NumeroExistsAsync(int numero, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.Tavolo
                    .AnyAsync(t => t.Numero == numero && t.TavoloId != excludeId.Value);
            }

            return await _context.Tavolo.AnyAsync(t => t.Numero == numero);
        }
    }
}