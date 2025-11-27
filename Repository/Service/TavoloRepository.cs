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
    public class TavoloRepository(BubbleTeaContext context) : ITavoloRepository
    {
        private readonly BubbleTeaContext _context = context;

        private static TavoloDTO MapToDTO(Tavolo tavolo)
        {
            return new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = tavolo.Numero,
                Zona = tavolo.Zona,
                Disponibile = tavolo.Disponibile
            };
        }

        private static TavoloFrontendDTO MapToFrontendDTO(Tavolo tavolo)
        {
            return new TavoloFrontendDTO
            {
                Numero = tavolo.Numero,
                Disponibile = GetDisponibileText(tavolo.Disponibile),
                Zona = FormatZona(tavolo.Zona ?? string.Empty)
            };
        }

        private static string GetDisponibileText(bool disponibile) => disponibile ? "SI" : "NO";

        private static string FormatZona(string zona) => string.IsNullOrEmpty(zona) ? "" : zona.ToUpper();

        // ✅ METODI CRUD ESISTENTI
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

            return tavolo == null ? null : MapToDTO(tavolo);
        }

        public async Task<TavoloDTO?> GetByNumeroAsync(int numero)
        {
            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero);

            return tavolo == null ? null : MapToDTO(tavolo);
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

            if (await NumeroExistsAsync(tavoloDto.Numero))
                throw new ArgumentException($"Esiste già un tavolo con numero {tavoloDto.Numero}");

            var tavolo = new Tavolo
            {
                Numero = tavoloDto.Numero,
                Zona = tavoloDto.Zona,
                Disponibile = tavoloDto.Disponibile
            };

            _context.Tavolo.Add(tavolo);
            await _context.SaveChangesAsync();

            tavoloDto.TavoloId = tavolo.TavoloId;
            return tavoloDto;
        }

        public async Task UpdateAsync(TavoloDTO tavoloDto)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloDto.TavoloId);

            if (tavolo == null)
                return;

            if (await NumeroExistsAsync(tavoloDto.Numero, tavoloDto.TavoloId))
                throw new ArgumentException($"Esiste già un altro tavolo con numero {tavoloDto.Numero}");

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
                return await _context.Tavolo.AnyAsync(t => t.Numero == numero && t.TavoloId != excludeId.Value);

            return await _context.Tavolo.AnyAsync(t => t.Numero == numero);
        }

        // ✅ NUOVI METODI PER FRONTEND
        public async Task<IEnumerable<TavoloFrontendDTO>> GetAllPerFrontendAsync()
        {
            return await _context.Tavolo
                .AsNoTracking()
                .OrderBy(t => t.Numero)
                .Select(t => MapToFrontendDTO(t))
                .ToListAsync();
        }

        public async Task<IEnumerable<TavoloFrontendDTO>> GetDisponibiliPerFrontendAsync()
        {
            return await _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Disponibile)
                .OrderBy(t => t.Numero)
                .Select(t => MapToFrontendDTO(t))
                .ToListAsync();
        }

        public async Task<IEnumerable<TavoloFrontendDTO>> GetByZonaPerFrontendAsync(string zona)
        {
            var zonaFormattata = FormatZona(zona);

            return await _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Zona != null && t.Zona.ToUpper() == zonaFormattata)
                .OrderBy(t => t.Numero)
                .Select(t => MapToFrontendDTO(t))
                .ToListAsync();
        }

        public async Task<TavoloFrontendDTO?> GetByNumeroPerFrontendAsync(int numero)
        {
            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero);

            return tavolo == null ? null : MapToFrontendDTO(tavolo);
        }        

        public async Task<bool> ToggleDisponibilitaAsync(int tavoloId)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloId);

            if (tavolo == null)
                return false;

            // ✅ CORREZIONE: inverti la disponibilità
            tavolo.Disponibile = !tavolo.Disponibile;
            await _context.SaveChangesAsync();

            // ✅ RESTITUISCI IL NUOVO VALORE (dopo il toggle)
            return tavolo.Disponibile;
        }

        public async Task<bool> ToggleDisponibilitaByNumeroAsync(int numero)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.Numero == numero);

            if (tavolo == null)
                return false;

            // ✅ CORREZIONE: inverti la disponibilità
            tavolo.Disponibile = !tavolo.Disponibile;
            await _context.SaveChangesAsync();

            // ✅ RESTITUISCI IL NUOVO VALORE (dopo il toggle)
            return tavolo.Disponibile;
        }
    }
}