using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Repository.Service.Helper;

namespace Repository.Service
{
    public class UnitaDiMisuraRepository(BubbleTeaContext context, ILogger<UnitaDiMisuraRepository> logger) : IUnitaDiMisuraRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<UnitaDiMisuraRepository> _logger = logger;

        private UnitaDiMisuraDTO MapToDTO(UnitaDiMisura unita)
        {
            return new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita.UnitaMisuraId,
                Sigla = unita.Sigla,
                Descrizione = unita.Descrizione
            };
        }

        private static UnitaDiMisuraFrontendDTO MapToFrontendDTO(UnitaDiMisura unita)
        {
            return new UnitaDiMisuraFrontendDTO
            {
                Sigla = unita.Sigla,
                Descrizione = unita.Descrizione
            };
        }

        // ✅ METODO MODIFICATO: GetBySiglaAsync con parametro opzionale e paginazione
        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetBySiglaAsync(string? sigla = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (!SecurityHelper.IsValidInput(sigla, maxLength: 2))
                return new PaginatedResponseDTO<UnitaDiMisuraDTO> { Message = "Input non valido" };

            var query = _context.UnitaDiMisura.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(sigla))
            {
                var normalizedSigla = SecurityHelper.NormalizeSafe(sigla);
                query = query.Where(u => u.Sigla != null && u.Sigla.ToUpper().StartsWith(normalizedSigla));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Sigla)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => MapToDTO(u))
                .ToListAsync();

            return new PaginatedResponseDTO<UnitaDiMisuraDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ NUOVO METODO: GetByDescrizioneAsync con parametro opzionale e paginazione
        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetByDescrizioneAsync(string? descrizione = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                return new PaginatedResponseDTO<UnitaDiMisuraDTO> { Message = "Input non valido" };

            var query = _context.UnitaDiMisura.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(descrizione))
            {
                var normalizedDesc = SecurityHelper.NormalizeSafe(descrizione);
                query = query.Where(u => u.Descrizione != null && u.Descrizione.ToUpper().StartsWith(normalizedDesc));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Descrizione)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => MapToDTO(u))
                .ToListAsync();

            return new PaginatedResponseDTO<UnitaDiMisuraDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO FRONTEND: GetBySiglaPerFrontendAsync
        public async Task<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>> GetBySiglaPerFrontendAsync(string? sigla = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (!SecurityHelper.IsValidInput(sigla, maxLength: 2))
                return new PaginatedResponseDTO<UnitaDiMisuraFrontendDTO> { Message = "Input non valido" };

            var query = _context.UnitaDiMisura.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(sigla))
            {
                var normalizedSigla = SecurityHelper.NormalizeSafe(sigla);
                query = query.Where(u => u.Sigla != null && u.Sigla.ToUpper().StartsWith(normalizedSigla));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Sigla)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => MapToFrontendDTO(u))
                .ToListAsync();

            return new PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ NUOVO METODO FRONTEND: GetByDescrizionePerFrontendAsync
        public async Task<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>> GetByDescrizionePerFrontendAsync(string? descrizione = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                return new PaginatedResponseDTO<UnitaDiMisuraFrontendDTO> { Message = "Input non valido" };

            var query = _context.UnitaDiMisura.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(descrizione))
            {
                var normalizedDesc = SecurityHelper.NormalizeSafe(descrizione);
                query = query.Where(u => u.Descrizione != null && u.Descrizione.ToUpper().StartsWith(normalizedDesc));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Descrizione)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => MapToFrontendDTO(u))
                .ToListAsync();

            return new PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO MODIFICATO: GetAllAsync con paginazione
        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            var query = _context.UnitaDiMisura.AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Sigla)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => MapToDTO(u))
                .ToListAsync();

            return new PaginatedResponseDTO<UnitaDiMisuraDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UnitaDiMisuraDTO?> GetByIdAsync(int id)
        {
            var unita = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId == id);

            return unita == null ? null : MapToDTO(unita);
        }

        public async Task<UnitaDiMisuraDTO> AddAsync(UnitaDiMisuraDTO unitaDto)
        {
            // ✅ CONTROLLO DUPICATI SIGLA
            var existingBySigla = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.Sigla.ToUpper() == unitaDto.Sigla.ToUpper());

            if (existingBySigla != null)
                throw new InvalidOperationException($"Esiste già un'unità di misura con la sigla '{unitaDto.Sigla}'");

            // ✅ CONTROLLO DUPICATI DESCRIZIONE
            var existingByDesc = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.Descrizione.ToUpper() == unitaDto.Descrizione.ToUpper());

            if (existingByDesc != null)
                throw new InvalidOperationException($"Esiste già un'unità di misura con la descrizione '{unitaDto.Descrizione}'");

            var unita = new UnitaDiMisura
            {
                Sigla = unitaDto.Sigla.Trim().ToUpper(),
                Descrizione = unitaDto.Descrizione.Trim()
            };

            _context.UnitaDiMisura.Add(unita);
            await _context.SaveChangesAsync();

            unitaDto.UnitaMisuraId = unita.UnitaMisuraId;
            return unitaDto;
        }

        public async Task UpdateAsync(UnitaDiMisuraDTO unitaDto)
        {
            // ✅ CONTROLLO DUPICATI SIGLA (escludendo se stesso)
            var existingBySigla = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId != unitaDto.UnitaMisuraId &&
                                         u.Sigla.ToUpper() == unitaDto.Sigla.ToUpper());

            if (existingBySigla != null)
                throw new InvalidOperationException($"Esiste già un'unità di misura con la sigla '{unitaDto.Sigla}'");

            // ✅ CONTROLLO DUPICATI DESCRIZIONE (escludendo se stesso)
            var existingByDesc = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId != unitaDto.UnitaMisuraId &&
                                         u.Descrizione.ToUpper() == unitaDto.Descrizione.ToUpper());

            if (existingByDesc != null)
                throw new InvalidOperationException($"Esiste già un'unità di misura con la descrizione '{unitaDto.Descrizione}'");

            var unita = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId == unitaDto.UnitaMisuraId) ?? throw new KeyNotFoundException($"Unità di misura con ID {unitaDto.UnitaMisuraId} non trovata");
            unita.Sigla = unitaDto.Sigla.Trim().ToUpper();
            unita.Descrizione = unitaDto.Descrizione.Trim();

            _context.UnitaDiMisura.Update(unita);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // ✅ CONTROLLO DIPENDENZE PRIMA DI ELIMINARE
            if (await HasDependenciesAsync(id))
                throw new InvalidOperationException("Impossibile eliminare: esistono dipendenze (ingredienti o personalizzazioni collegati)");

            var unita = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId == id);

            if (unita == null)
                throw new KeyNotFoundException($"Unità di misura con ID {id} non trovata");

            _context.UnitaDiMisura.Remove(unita);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.UnitaMisuraId == id);
        }

        public async Task<bool> SiglaExistsAsync(string sigla)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.Sigla.ToUpper() == sigla.ToUpper());
        }

        public async Task<bool> SiglaExistsForOtherAsync(int id, string sigla)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.UnitaMisuraId != id && u.Sigla.ToUpper() == sigla.ToUpper());
        }

        public async Task<bool> DescrizioneExistsAsync(string descrizione)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.Descrizione.ToUpper() == descrizione.ToUpper());
        }

        public async Task<bool> DescrizioneExistsForOtherAsync(int id, string descrizione)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.UnitaMisuraId != id && u.Descrizione.ToUpper() == descrizione.ToUpper());
        }

        public async Task<bool> HasDependenciesAsync(int id)
        {
            // Controlla se ci sono DimensioneBicchiere collegati
            bool hasDimensioneBicchiere = await _context.DimensioneBicchiere
                .AnyAsync(d => d.UnitaMisuraId == id);

            // Controlla se ci sono PersonalizzazioneIngrediente collegati
            bool hasPersonalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.UnitaMisuraId == id);

            return hasDimensioneBicchiere || hasPersonalizzazioneIngrediente;
        }
    }
}