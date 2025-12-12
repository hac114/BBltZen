using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;

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

        public async Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.CategoriaIngrediente
                    .AsNoTracking()
                    .OrderBy(c => c.Categoria);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(c => MapToDTO(c))
                    .ToListAsync();

                return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna categoria di ingrediente trovata"
                        : $"Trovate {totalCount} categorie di ingrediente"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle categorie di ingrediente"
                };
            }
        }

        public async Task<SingleResponseDTO<CategoriaIngredienteDTO>> GetByIdAsync(int categoriaId)
        {
            try
            {
                if (categoriaId <= 0)
                    return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse("ID categoria non valido");

                var categoria = await _context.CategoriaIngrediente
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoriaId == categoriaId);

                if (categoria == null)
                    return SingleResponseDTO<CategoriaIngredienteDTO>.NotFoundResponse(
                        $"Categoria ingrediente con ID {categoriaId} non trovata");

                return SingleResponseDTO<CategoriaIngredienteDTO>.SuccessResponse(
                    MapToDTO(categoria),
                    $"Categoria ingrediente con ID {categoriaId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per categoriaId: {CategoriaId}", categoriaId);
                return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse(
                    "Errore interno nel recupero della categoria");
            }
        }

        public async Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetByNomeAsync(string categoria, int page = 1, int pageSize = 10)
        {
            try
            {
                var searchTerm = StringHelper.NormalizeSearchTerm(categoria);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'categoria' è obbligatorio"
                    };
                }

                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 50))
                {
                    return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'categoria' contiene caratteri non validi"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.CategoriaIngrediente
                    .AsNoTracking()
                    .Where(c => c.Categoria != null &&
                               StringHelper.StartsWithCaseInsensitive(c.Categoria, searchTerm))
                    .OrderBy(c => c.Categoria);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(c => MapToDTO(c))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna categoria di ingrediente trovata con nome che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 categoria di ingrediente con nome che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} categorie di ingrediente con nome che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<CategoriaIngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle categorie di ingrediente"
                };
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int categoriaId)
        {
            try
            {
                if (categoriaId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID categoria non valido");

                var exists = await _context.CategoriaIngrediente
                    .AsNoTracking()
                    .AnyAsync(c => c.CategoriaId == categoriaId);

                string message = exists
                    ? $"Categoria ingrediente con ID {categoriaId} esiste"
                    : $"Categoria ingrediente con ID {categoriaId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per categoriaId: {CategoriaId}", categoriaId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della categoria");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string categoria)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome della categoria è obbligatorio");

                var searchTerm = StringHelper.NormalizeSearchTerm(categoria);

                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome della categoria contiene caratteri non validi");

                var exists = await _context.CategoriaIngrediente
                    .AsNoTracking()
                    .AnyAsync(c => StringHelper.EqualsCaseInsensitive(c.Categoria, searchTerm));

                string message = exists
                    ? $"Categoria ingrediente con nome '{searchTerm}' esiste"
                    : $"Categoria ingrediente con nome '{searchTerm}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByNomeAsync per categoria: {Categoria}", categoria);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della categoria per nome");
            }
        }        

        private async Task<bool> ExistsByNomeInternalAsync(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(categoria);
            return await _context.CategoriaIngrediente
                .AsNoTracking()
                .AnyAsync(c => StringHelper.EqualsCaseInsensitive(c.Categoria, searchTerm));
        }

        private async Task<bool> ExistsByNomeForOtherAsync(int excludeId, string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return false;

            return await _context.CategoriaIngrediente
                .AsNoTracking()
                .AnyAsync(c => c.CategoriaId != excludeId &&
                              StringHelper.EqualsCaseInsensitive(c.Categoria, categoria.Trim()));
        }

        private async Task<bool> ExistsByNomeForOtherInternalAsync(int excludeId, string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(categoria);
            return await _context.CategoriaIngrediente
                .AsNoTracking()
                .AnyAsync(c => c.CategoriaId != excludeId &&
                              StringHelper.EqualsCaseInsensitive(c.Categoria, searchTerm));
        }

        public async Task<SingleResponseDTO<CategoriaIngredienteDTO>> AddAsync(CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(categoriaDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(categoriaDto.Categoria))
                    return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse("Nome categoria obbligatorio");

                var searchTerm = StringHelper.NormalizeSearchTerm(categoriaDto.Categoria);

                if (!SecurityHelper.IsValidInput(searchTerm, 50))
                    return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse("Nome categoria non valido");

                // ✅ Controllo duplicati (usa metodo interno)
                if (await ExistsByNomeInternalAsync(searchTerm))
                    return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse(
                        $"Esiste già una categoria con nome '{searchTerm}'");

                var categoria = new CategoriaIngrediente
                {
                    Categoria = searchTerm
                };

                await _context.CategoriaIngrediente.AddAsync(categoria);
                await _context.SaveChangesAsync();

                categoriaDto.CategoriaId = categoria.CategoriaId;

                return SingleResponseDTO<CategoriaIngredienteDTO>.SuccessResponse(
                    categoriaDto,
                    $"Categoria '{searchTerm}' creata con successo (ID: {categoria.CategoriaId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per categoria: {Categoria}", categoriaDto?.Categoria);
                return SingleResponseDTO<CategoriaIngredienteDTO>.ErrorResponse("Errore interno durante la creazione della categoria");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(CategoriaIngredienteDTO categoriaDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(categoriaDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(categoriaDto.Categoria))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome categoria obbligatorio");

                var searchTerm = StringHelper.NormalizeSearchTerm(categoriaDto.Categoria);

                if (!SecurityHelper.IsValidInput(searchTerm, 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome categoria non valido");

                var categoria = await _context.CategoriaIngrediente
                    .FirstOrDefaultAsync(c => c.CategoriaId == categoriaDto.CategoriaId);

                if (categoria == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Categoria ingrediente con ID {categoriaDto.CategoriaId} non trovata");

                // ✅ Controllo duplicati ESCLUDENDO questa categoria (usa metodo interno)
                if (await ExistsByNomeForOtherInternalAsync(categoriaDto.CategoriaId, searchTerm))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra categoria con nome '{searchTerm}'");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                if (!StringHelper.EqualsCaseInsensitive(categoria.Categoria, searchTerm))
                {
                    categoria.Categoria = searchTerm;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Categoria con ID {categoriaDto.CategoriaId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per la categoria con ID {categoriaDto.CategoriaId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per categoriaId: {CategoriaId}", categoriaDto?.CategoriaId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della categoria");
            }
        }

        private async Task<bool> HasDependenciesAsync(int categoriaId)
        {
            return await _context.Ingrediente
                .AnyAsync(i => i.CategoriaId == categoriaId);
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int categoriaId)
        {
            try
            {
                // ✅ Validazione input
                if (categoriaId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID categoria non valido");

                // ✅ Ricerca della categoria
                var categoria = await _context.CategoriaIngrediente
                    .FirstOrDefaultAsync(c => c.CategoriaId == categoriaId);

                if (categoria == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Categoria ingrediente con ID {categoriaId} non trovata");

                // ✅ Controllo dipendenze
                if (await HasDependenciesAsync(categoriaId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare la categoria perché ci sono ingredienti collegati");

                // ✅ Eliminazione
                _context.CategoriaIngrediente.Remove(categoria);
                await _context.SaveChangesAsync();

                // ✅ Successo con messaggio
                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Categoria ingrediente '{categoria.Categoria}' (ID: {categoriaId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per categoriaId: {CategoriaId}", categoriaId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione della categoria");
            }
        }
    }
}