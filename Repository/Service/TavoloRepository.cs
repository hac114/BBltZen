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

        private TavoloDTO MapToDTO(Tavolo tavolo)
        {
            return new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = tavolo.Numero,
                Zona = tavolo.Zona,
                Disponibile = tavolo.Disponibile
            };
        }

        public async Task<IEnumerable<TavoloDTO>> GetAllAsync()
        {
            return await _context.Tavolo
                .AsNoTracking()
                .OrderBy(t => t.Numero)
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }

        public async Task<TavoloDTO?> GetByIdAsync(int tavoloId)
        {
            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloId);

            if (tavolo == null)
                return null;

            return MapToDTO(tavolo);
        }

        public async Task<TavoloDTO?> GetByNumeroAsync(int numero)
        {
            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero);

            if (tavolo == null)
                return null;

            return MapToDTO(tavolo);
        }

        public async Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync()
        {
            return await _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Disponibile)
                .OrderBy(t => t.Numero)
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }

        public async Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona)
        {
            return await _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Zona == zona)
                .OrderBy(t => t.Numero)
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }

        public async Task<TavoloDTO> AddAsync(TavoloDTO tavoloDto)
        {
            if (tavoloDto == null)
                throw new ArgumentNullException(nameof(tavoloDto));

            // ✅ VERIFICA UNICITÀ NUMERO TAVOLO
            if (await NumeroExistsAsync(tavoloDto.Numero))
            {
                throw new ArgumentException($"Esiste già un tavolo con numero {tavoloDto.Numero}");
            }

            var tavolo = new Tavolo
            {
                Numero = tavoloDto.Numero,
                Zona = tavoloDto.Zona,
                Disponibile = tavoloDto.Disponibile
            };

            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            tavoloDto.TavoloId = tavolo.TavoloId;
            return tavoloDto;
        }

        public async Task UpdateAsync(TavoloDTO tavoloDto)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloDto.TavoloId);

            if (tavolo == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ NUMERO TAVOLO (escludendo il record corrente)
            if (await NumeroExistsAsync(tavoloDto.Numero, tavoloDto.TavoloId))
            {
                throw new ArgumentException($"Esiste già un altro tavolo con numero {tavoloDto.Numero}");
            }

            tavolo.Numero = tavoloDto.Numero;
            tavolo.Zona = tavoloDto.Zona;
            tavolo.Disponibile = tavoloDto.Disponibile;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int tavoloId)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloId);

            if (tavolo != null)
            {
                _context.Tavolo.Remove(tavolo);
                await _context.SaveChangesAsync();
            }
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
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