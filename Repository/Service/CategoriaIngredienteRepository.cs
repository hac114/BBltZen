using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Repository.Service.Helper;

namespace Repository.Service
{
    public class CategoriaIngredienteRepository(BubbleTeaContext context, ILogger<CategoriaIngredienteRepository> logger) : ICategoriaIngredienteRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<CategoriaIngredienteRepository> _logger = logger;

        private static CategoriaIngredienteDTO MapToDTO(CategoriaIngrediente categoria)
        {
            return new CategoriaIngredienteDTO
            {
                CategoriaId = categoria.CategoriaId,
                Categoria = categoria.Categoria
            };
        }

        private static CategoriaIngredienteFrontendDTO MapToFrontendDTO(CategoriaIngrediente categoria)
        {
            return new CategoriaIngredienteFrontendDTO
            {
                Categoria = categoria.Categoria
            };
        }

        // ✅ METODO CRUD: GetAllAsync (paginato)
        public async Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            var query = _context.CategoriaIngrediente.AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Categoria)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(c => MapToDTO(c))
                .ToListAsync();

            return new PaginatedResponseDTO<CategoriaIngredienteDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO CRUD: GetByIdAsync
        public async Task<CategoriaIngredienteDTO?> GetByIdAsync(int id)
        {
            var categoria = await _context.CategoriaIngrediente
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            return categoria == null ? null : MapToDTO(categoria);
        }

        // ✅ METODO CRUD: AddAsync
        public async Task<CategoriaIngredienteDTO> AddAsync(CategoriaIngredienteDTO categoriaDto)
        {
            if (categoriaDto == null)
                throw new ArgumentNullException(nameof(categoriaDto));

            // ✅ VALIDAZIONE INPUT CON SecurityHelper
            if (!SecurityHelper.IsValidInput(categoriaDto.Categoria, maxLength: 50))
                throw new ArgumentException("Nome categoria non valido");

            // ✅ CONTROLLO UNIVOCITÀ CON StringHelper (case-insensitive)
            var exists = await _context.CategoriaIngrediente
                .AnyAsync(c => StringHelper.EqualsCaseInsensitive(c.Categoria, categoriaDto.Categoria));

            if (exists)
                throw new InvalidOperationException($"Esiste già una categoria con nome '{categoriaDto.Categoria}'");

            var categoria = new CategoriaIngrediente
            {
                Categoria = StringHelper.NormalizeSearchTerm(categoriaDto.Categoria)
            };

            _context.CategoriaIngrediente.Add(categoria);
            await _context.SaveChangesAsync();

            categoriaDto.CategoriaId = categoria.CategoriaId;
            return categoriaDto;
        }

        // ✅ METODO CRUD: UpdateAsync
        public async Task UpdateAsync(CategoriaIngredienteDTO categoriaDto)
        {
            // ✅ VALIDAZIONE INPUT CON SecurityHelper
            if (!SecurityHelper.IsValidInput(categoriaDto.Categoria, maxLength: 50))
                throw new ArgumentException("Nome categoria non valido");

            var categoria = await _context.CategoriaIngrediente
                .FirstOrDefaultAsync(c => c.CategoriaId == categoriaDto.CategoriaId);

            if (categoria == null)
                throw new KeyNotFoundException($"Categoria con ID {categoriaDto.CategoriaId} non trovata");

            // ✅ CONTROLLO UNIVOCITÀ CON StringHelper (case-insensitive)
            var existsOther = await _context.CategoriaIngrediente
                .AnyAsync(c => c.CategoriaId != categoriaDto.CategoriaId &&
                              StringHelper.EqualsCaseInsensitive(c.Categoria, categoriaDto.Categoria));

            if (existsOther)
                throw new InvalidOperationException($"Esiste già un'altra categoria con nome '{categoriaDto.Categoria}'");

            categoria.Categoria = StringHelper.NormalizeSearchTerm(categoriaDto.Categoria);
            await _context.SaveChangesAsync();
        }

        // ✅ METODO CRUD: DeleteAsync
        public async Task DeleteAsync(int id)
        {
            // ✅ CONTROLLO DIPENDENZE
            if (await HasDependenciesAsync(id))
                throw new InvalidOperationException("Impossibile eliminare: esistono dipendenze (ingredienti collegati)");

            var categoria = await _context.CategoriaIngrediente
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria == null)
                throw new KeyNotFoundException($"Categoria con ID {id} non trovata");

            _context.CategoriaIngrediente.Remove(categoria);
            await _context.SaveChangesAsync();
        }

        // ✅ METODO CRUD: ExistsAsync
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CategoriaIngrediente
                .AnyAsync(c => c.CategoriaId == id);
        }

        // ✅ METODO BUSINESS: GetByNomeAsync (ricerca "inizia con" - parametro opzionale)
        public async Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetByNomeAsync(string? categoria = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            // ✅ VALIDAZIONE INPUT CON SecurityHelper
            if (!SecurityHelper.IsValidInput(categoria, maxLength: 50))
                return new PaginatedResponseDTO<CategoriaIngredienteDTO> { Message = "Input non valido" };

            var query = _context.CategoriaIngrediente.AsNoTracking().AsQueryable();

            // ✅ RICERCA "INIZIA CON" USANDO StringHelper (se categoria non è null)
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                var normalizedCategoria = SecurityHelper.NormalizeSafe(categoria);
                query = query.Where(c => c.Categoria != null &&
                       StringHelper.StartsWithCaseInsensitive(c.Categoria, categoria));
            }

            // ✅ PAGINAZIONE
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Categoria)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(c => MapToDTO(c))
                .ToListAsync();

            return new PaginatedResponseDTO<CategoriaIngredienteDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO BUSINESS: GetByNomePerFrontendAsync (ricerca "inizia con" - parametro opzionale)
        public async Task<PaginatedResponseDTO<CategoriaIngredienteFrontendDTO>> GetByNomePerFrontendAsync(string? categoria = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            // ✅ VALIDAZIONE INPUT CON SecurityHelper
            if (!SecurityHelper.IsValidInput(categoria, maxLength: 50))
                return new PaginatedResponseDTO<CategoriaIngredienteFrontendDTO> { Message = "Input non valido" };

            var query = _context.CategoriaIngrediente.AsNoTracking().AsQueryable();

            // ✅ RICERCA "INIZIA CON" USANDO StringHelper (se categoria non è null)
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(c => c.Categoria != null &&
                       StringHelper.StartsWithCaseInsensitive(c.Categoria, categoria));
            }

            // ✅ PAGINAZIONE
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Categoria)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(c => MapToFrontendDTO(c))
                .ToListAsync();

            return new PaginatedResponseDTO<CategoriaIngredienteFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        // ✅ METODO UTILITY: ExistsByNomeAsync
        public async Task<bool> ExistsByNomeAsync(string categoria)
        {
            return await _context.CategoriaIngrediente
                .AnyAsync(c => StringHelper.EqualsCaseInsensitive(c.Categoria, categoria));
        }

        // ✅ METODO UTILITY: HasDependenciesAsync (per controllo DELETE)
        public async Task<bool> HasDependenciesAsync(int id)
        {
            return await _context.Ingrediente
                .AnyAsync(i => i.CategoriaId == id);
        }

        // ❌ METODI OBSOLETI (non più nell'interfaccia)
        // public async Task<IEnumerable<CategoriaIngredienteDTO>> GetAllAsync() // OBSOLETO
        // public async Task<IEnumerable<CategoriaIngredienteFrontendDTO>> GetAllPerFrontendAsync() // OBSOLETO
        // public async Task<CategoriaIngredienteFrontendDTO?> GetByNomePerFrontendAsync(string categoria) // OBSOLETO
    }
}