using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service.Helper;
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

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.Tavolo
                .AsNoTracking()
                .OrderBy(t => t.Numero);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TavoloDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TavoloDTO?> GetByIdAsync(int? tavoloId = null)
        {
            // ✅ SE NULL, RESTITUISCE NULL (il controller gestirà il caso)
            if (!tavoloId.HasValue)
                return null;

            // ✅ VALIDAZIONE SICUREZZA
            if (tavoloId <= 0)
                return null;

            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloId.Value);

            return tavolo == null ? null : MapToDTO(tavolo);
        }

        public async Task<TavoloDTO?> GetByNumeroAsync(int? numero = null)
        {
            // ✅ SE NULL, RESTITUISCE NULL (il controller gestirà il caso)
            if (!numero.HasValue)
                return null;

            // ✅ VALIDAZIONE SICUREZZA
            if (numero <= 0)
                return null;

            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero.Value);

            return tavolo == null ? null : MapToDTO(tavolo);
        }

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Disponibile)
                .OrderBy(t => t.Numero);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TavoloDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetByZonaAsync(string? zona = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            // ✅ VALIDAZIONE SICUREZZA INPUT
            if (!SecurityHelper.IsValidInput(zona))
                return new PaginatedResponseDTO<TavoloDTO> { Message = "Input non valido" };

            var query = _context.Tavolo.AsQueryable();

            // ✅ FILTRA SOLO SE ZONA SPECIFICATA - USA STRINGHELPER
            if (!string.IsNullOrWhiteSpace(zona))
            {
                var normalizedZona = StringHelper.NormalizeSearchTerm(zona);
                query = query.Where(t => t.Zona != null &&
                       StringHelper.StartsWithCaseInsensitive(t.Zona, normalizedZona));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Numero)
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TavoloDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TavoloDTO> AddAsync(TavoloDTO tavoloDto)
        {
            ArgumentNullException.ThrowIfNull(tavoloDto);

            // ✅ VALIDAZIONE SICUREZZA
            if (tavoloDto.Numero <= 0)
            {
                throw new ArgumentException("Numero tavolo non valido");
            }

            if (!SecurityHelper.IsValidInput(tavoloDto.Zona))
            {
                throw new ArgumentException("Zona non valida");
            }

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

            tavoloDto.TavoloId = tavolo.TavoloId;
            return tavoloDto;
        }

        public async Task UpdateAsync(TavoloDTO tavoloDto)
        {
            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloDto.TavoloId);

            if (tavolo == null)
            {
                return;
            }

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
                return await _context.Tavolo.AnyAsync(t => t.Numero == numero && t.TavoloId != excludeId.Value);
            }

            return await _context.Tavolo.AnyAsync(t => t.Numero == numero);
        }

        // ✅ METODI FRONTEND PAGINATI - CORRETTI PER INTERFACCIA
        //public async Task<PaginatedResponseDTO<TavoloFrontendDTO>> GetAllPerFrontendAsync(int page = 1, int pageSize = 10)
        //{
        //    var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
        //    var skip = (safePage - 1) * safePageSize;

        //    var query = _context.Tavolo
        //        .AsNoTracking()
        //        .OrderBy(t => t.Numero);

        //    var totalCount = await query.CountAsync();
        //    var items = await query
        //        .Skip(skip)
        //        .Take(safePageSize)
        //        .Select(t => MapToFrontendDTO(t))
        //        .ToListAsync();

        //    return new PaginatedResponseDTO<TavoloFrontendDTO>
        //    {
        //        Data = items,
        //        Page = safePage,
        //        PageSize = safePageSize,
        //        TotalCount = totalCount
        //    };
        //}

        // ✅ METODO PAGINATO - CORRETTO PER INTERFACCIA
        public async Task<PaginatedResponseDTO<TavoloFrontendDTO>> GetDisponibiliPerFrontendAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Disponibile)
                .OrderBy(t => t.Numero);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToFrontendDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TavoloFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloFrontendDTO>> GetByZonaPerFrontendAsync(string? zona = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            if (!SecurityHelper.IsValidInput(zona))
                return new PaginatedResponseDTO<TavoloFrontendDTO> { Message = "Input non valido" };

            var query = _context.Tavolo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(zona))
            {
                var normalizedZona = StringHelper.NormalizeSearchTerm(zona);
                query = query.Where(t => t.Zona != null &&
                       StringHelper.StartsWithCaseInsensitive(t.Zona, normalizedZona));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Numero)
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToFrontendDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TavoloFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TavoloFrontendDTO?> GetByNumeroPerFrontendAsync(int? numero = null)
        {
            // ✅ SE NULL, RESTITUISCE NULL (il controller gestirà il caso)
            if (!numero.HasValue)
                return null;

            // ✅ VALIDAZIONE SICUREZZA
            if (numero <= 0)
                return null;

            var tavolo = await _context.Tavolo
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Numero == numero.Value);

            return tavolo == null ? null : MapToFrontendDTO(tavolo);
        }

        public async Task<bool> ToggleDisponibilitaAsync(int tavoloId)
        {
            // ✅ VALIDAZIONE SICUREZZA
            if (tavoloId <= 0) return false;

            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.TavoloId == tavoloId);

            if (tavolo == null)
                return false;

            tavolo.Disponibile = !tavolo.Disponibile;
            await _context.SaveChangesAsync();

            return tavolo.Disponibile;
        }

        public async Task<bool> ToggleDisponibilitaByNumeroAsync(int numero)
        {
            // ✅ VALIDAZIONE SICUREZZA
            if (numero <= 0) return false;

            var tavolo = await _context.Tavolo
                .FirstOrDefaultAsync(t => t.Numero == numero);

            if (tavolo == null)
                return false;

            tavolo.Disponibile = !tavolo.Disponibile;
            await _context.SaveChangesAsync();

            return tavolo.Disponibile;
        }

        // ❌ METODO DA ELIMINARE - RIDONDANTE CON GetByZonaAsync
        /*
        public async Task<IEnumerable<TavoloDTO>> GetAllByZonaAsync(string? zona = null)
        {
            var query = _context.Tavolo.AsQueryable();

            if (!string.IsNullOrWhiteSpace(zona))
            {
                query = query.Where(t => t.Zona != null &&
                                       t.Zona.StartsWith(zona, StringComparison.InvariantCultureIgnoreCase));
            }

            return await query
                .AsNoTracking()
                .OrderBy(t => t.Numero)
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }
        */
    }
}