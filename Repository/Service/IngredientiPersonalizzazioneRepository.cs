using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class IngredientiPersonalizzazioneRepository(BubbleTeaContext context, ILogger<IngredientiPersonalizzazioneRepository> logger) : IIngredientiPersonalizzazioneRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger _logger = logger;

        private IngredientiPersonalizzazioneDTO MapToDTOWithJoin(IngredientiPersonalizzazione ingredientePers)
        {
            return new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = ingredientePers.IngredientePersId,
                PersCustomId = ingredientePers.PersCustomId,
                IngredienteId = ingredientePers.IngredienteId,
                DataCreazione = ingredientePers.DataCreazione,
                NomePersonalizzazione = _context.PersonalizzazioneCustom
                    .Where(pc => pc.PersCustomId == ingredientePers.PersCustomId)
                    .Select(pc => pc.Nome)
                    .FirstOrDefault() ?? "",
                NomeIngrediente = _context.Ingrediente
                    .Where(i => i.IngredienteId == ingredientePers.IngredienteId)
                    .Select(i => i.Ingrediente1)
                    .FirstOrDefault() ?? ""
            };
        }

        public async Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .OrderByDescending(ip => ip.DataCreazione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(ip => new IngredientiPersonalizzazioneDTO
                    {
                        IngredientePersId = ip.IngredientePersId,
                        PersCustomId = ip.PersCustomId,
                        IngredienteId = ip.IngredienteId,
                        DataCreazione = ip.DataCreazione,
                        NomePersonalizzazione = _context.PersonalizzazioneCustom
                            .Where(pc => pc.PersCustomId == ip.PersCustomId)
                            .Select(pc => pc.Nome)
                            .FirstOrDefault() ?? "",
                        NomeIngrediente = _context.Ingrediente
                            .Where(i => i.IngredienteId == ip.IngredienteId)
                            .Select(i => i.Ingrediente1)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessuna ingredienti personalizzazione trovata",
                        1 => "Trovato 1 ingredienti personalizzazione",
                        _ => $"Trovate {totalCount} ingredienti personalizzazioni"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero di ingredienti personalizzazioni"
                };
            }
        }

        public async Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> GetByIdAsync(int ingredientePersId)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse("ID ingredienti personalizzazione non valido");

                var dto = await _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .Where(ip => ip.IngredientePersId == ingredientePersId)
                    .Select(ip => new IngredientiPersonalizzazioneDTO
                    {
                        IngredientePersId = ip.IngredientePersId,
                        PersCustomId = ip.PersCustomId,
                        IngredienteId = ip.IngredienteId,
                        DataCreazione = ip.DataCreazione,
                        NomePersonalizzazione = _context.PersonalizzazioneCustom
                            .Where(pc => pc.PersCustomId == ip.PersCustomId)
                            .Select(pc => pc.Nome)
                            .FirstOrDefault() ?? "",
                        NomeIngrediente = _context.Ingrediente
                            .Where(i => i.IngredienteId == ip.IngredienteId)
                            .Select(i => i.Ingrediente1)
                            .FirstOrDefault() ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.NotFoundResponse(
                        $"Ingredienti personalizzazione con ID {ingredientePersId} non trovata");

                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.SuccessResponse(
                    dto,
                    $"Ingredienti personalizzazione con ID {ingredientePersId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per ID: {IngredientePersId}", ingredientePersId);
                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse(
                    "Errore interno nel recupero di ingredienti personalizzazione");
            }
        }

        public async Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (persCustomId <= 0)
                {
                    return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'persCustomId' non è valido"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .Where(ip => ip.PersCustomId == persCustomId)
                    .OrderByDescending(ip => ip.DataCreazione);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(ip => ip.PersCustomId)
                    .Select(ip => MapToDTOWithJoin(ip))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna ingredienti personalizzazione trovata per parametro persCustomId '{persCustomId}'",
                    1 => $"Trovata 1 ingredienti personalizzazione per parametro persCustomId '{persCustomId}'",
                    _ => $"Trovate {totalCount} ingredienti personalizzazione per parametro persCustomId '{persCustomId}'"
                };

                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPersCustomIdAsync per parametro persCustomId: {PersCustomId}", persCustomId);
                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero di ingredienti personalizzazione per parametro persCustomId"
                };
            }
        }

        public async Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (ingredienteId <= 0)
                {
                    return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'ingredienteId' non è valido"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .Where(ip => ip.IngredienteId == ingredienteId)
                    .OrderByDescending(ip => ip.DataCreazione);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(ip => ip.PersCustomId)
                    .Select(ip => MapToDTOWithJoin(ip))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna ingredienti personalizzazione trovata per parametro ingredienteId '{ingredienteId}'",
                    1 => $"Trovata 1 ingredienti personalizzazione per parametro ingredienteId '{ingredienteId}'",
                    _ => $"Trovate {totalCount} ingredienti personalizzazione per parametro ingredienteId '{ingredienteId}'"
                };

                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIngredienteIdAsync per parametro ingredienteId: {IngredienteId}", ingredienteId);
                return new PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero di ingredienti personalizzazione per parametro ingredienteId"
                };
            }
        }

        public async Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> GetByCombinazioneAsync(int persCustomId, int ingredienteId)
        {
            try
            {
                if (persCustomId <= 0 || ingredienteId <= 0)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse($"ingredienti personalizzazione non valida per parametri persCustomId: {persCustomId} e ingredienteId: {ingredienteId}");

                var dto = await _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .Where(ip => ip.PersCustomId == persCustomId && ip.IngredienteId == ingredienteId)
                    .Select(ip => new IngredientiPersonalizzazioneDTO
                    {
                        IngredientePersId = ip.IngredientePersId,
                        PersCustomId = ip.PersCustomId,
                        IngredienteId = ip.IngredienteId,
                        DataCreazione = ip.DataCreazione,
                        NomePersonalizzazione = _context.PersonalizzazioneCustom
                            .Where(pc => pc.PersCustomId == ip.PersCustomId)
                            .Select(pc => pc.Nome)
                            .FirstOrDefault() ?? "",
                        NomeIngrediente = _context.Ingrediente
                            .Where(i => i.IngredienteId == ip.IngredienteId)
                            .Select(i => i.Ingrediente1)
                            .FirstOrDefault() ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.NotFoundResponse(
                        $"Ingredienti personalizzazione con parametri persCustomId: {persCustomId} e ingredienteId: {ingredienteId} non trovata");

                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.SuccessResponse(
                    dto,
                    $"Ingredienti personalizzazione con parametri persCustomId: {persCustomId} e ingredienteId: {ingredienteId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per persCustomId: {PersCustomId} e ingredienteId: {IngredienteId}", persCustomId, ingredienteId);
                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse(
                    "Errore interno nel recupero di ingredienti personalizzazione");
            }
        }

        private async Task<bool> IngredienteEsisteInPersonalizzazioneCustomAsync(int persCustomId, int ingredienteId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .AnyAsync(ip => ip.PersCustomId == persCustomId &&
                                ip.IngredienteId == ingredienteId);
        }

        public async Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(ingredientiPersDto);

                if (ingredientiPersDto.PersCustomId <= 0)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse("Il parametro PersCustomId è obbligatorio");

                if (ingredientiPersDto.IngredienteId <= 0)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse("Il parametro IngredienteId è obbligatorio");

                if (await IngredienteEsisteInPersonalizzazioneCustomAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse(
                        $"Esiste già ingrediente incluso nella personalizzazione custom con persCustoId: {ingredientiPersDto.PersCustomId} e ingredienteId '{ingredientiPersDto.IngredienteId}'");

                var ingredientiPersonalizzazione = new IngredientiPersonalizzazione
                {
                    PersCustomId = ingredientiPersDto.PersCustomId,
                    IngredienteId = ingredientiPersDto.IngredienteId,
                    DataCreazione = DateTime.Now
                };

                await _context.IngredientiPersonalizzazione.AddAsync(ingredientiPersonalizzazione);
                await _context.SaveChangesAsync();

                // ✅ Recupera il DTO con nomePersonalizazione e nomeIngrediente usando l'ID dell'entità creata
                var resultDto = await _context.IngredientiPersonalizzazione
                    .Where(ip => ip.IngredientePersId == ingredientiPersonalizzazione.IngredientePersId)
                    .Select(ip => new IngredientiPersonalizzazioneDTO
                    {
                        IngredientePersId = ip.IngredientePersId,
                        PersCustomId = ip.PersCustomId,
                        IngredienteId = ip.IngredienteId,
                        DataCreazione = ip.DataCreazione,
                        NomePersonalizzazione = _context.PersonalizzazioneCustom
                            .Where(pc => pc.PersCustomId == ip.PersCustomId)
                            .Select(pc => pc.Nome)
                            .FirstOrDefault() ?? "",
                        NomeIngrediente = _context.Ingrediente
                            .Where(i => i.IngredienteId == ip.IngredienteId)
                            .Select(i => i.Ingrediente1)
                            .FirstOrDefault() ?? ""
                    })
                    .FirstOrDefaultAsync();

                if (resultDto == null)
                    return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse("Errore nel recupero di ingredienti personalizzazione creata");

                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.SuccessResponse(resultDto,
                    $"Ingredienti personalizzazione creata con successo (ID: {resultDto.IngredientePersId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per PersCustomId: {PersCustomId} e IngredienteId: {IngredienteId}",
                    ingredientiPersDto?.PersCustomId, ingredientiPersDto?.IngredienteId);
                return SingleResponseDTO<IngredientiPersonalizzazioneDTO>.ErrorResponse("Errore interno durante la creazione di ingredienti personalizzazione");
            }
        }

        private async Task<bool> IngredienteEsisteInPersonalizzazioneCustomAsync(int persCustomId, int ingredienteId, int excludeId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .AnyAsync(ip => ip.PersCustomId == persCustomId &&
                                ip.IngredienteId == ingredienteId &&
                                ip.IngredientePersId != excludeId);
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(ingredientiPersDto);

                if (ingredientiPersDto.IngredientePersId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID ingredienti personalizzazione obbligatorio");

                if (ingredientiPersDto.PersCustomId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il parametro PersCustomId è obbligatorio");

                if (ingredientiPersDto.IngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il parametro IngredienteId è obbligatorio");

                if (await IngredienteEsisteInPersonalizzazioneCustomAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId, ingredientiPersDto.IngredientePersId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già ingrediente incluso nella personalizzazione custom con persCustoId: {ingredientiPersDto.PersCustomId} e ingredienteId '{ingredientiPersDto.IngredienteId}'");

                var ingredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                    .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientiPersDto.IngredientePersId);

                if (ingredientiPersonalizzazione == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Ingredienti personalizzazione con ID {ingredientiPersDto.IngredientePersId} non trovata");

                bool hasChanges = false;

                if (ingredientiPersonalizzazione.PersCustomId != ingredientiPersDto.PersCustomId)
                {
                    ingredientiPersonalizzazione.PersCustomId = ingredientiPersDto.PersCustomId;
                    hasChanges = true;
                }

                if (ingredientiPersonalizzazione.IngredienteId != ingredientiPersDto.IngredienteId)
                {
                    ingredientiPersonalizzazione.IngredienteId = ingredientiPersDto.IngredienteId;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(true,
                        $"Ingredienti personalizzazione ID: {ingredientiPersonalizzazione.IngredientePersId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(false,
                        $"Nessuna modifica necessaria per ingredienti personalizzazione con ID: {ingredientiPersonalizzazione.IngredientePersId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ID: {IngredientePersId}",
                    ingredientiPersDto?.IngredientePersId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento di ingredienti personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int ingredientePersId)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione custom non valido");

                var ingredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                    .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientePersId);

                if (ingredientiPersonalizzazione == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Ingredienti personalizzazione con ID: {ingredientePersId} non trovato");

                _context.IngredientiPersonalizzazione.Remove(ingredientiPersonalizzazione);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(true,
                    $"Ingredienti personalizzazione con (ID: {ingredientePersId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per ingredientePersId: {IngredientePersId}", ingredientePersId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione di ingredienti personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int ingredientePersId)
        {
            try
            {
                if (ingredientePersId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .AnyAsync(ip => ip.IngredientePersId == ingredientePersId);

                string message = exists
                    ? $"Ingredienti personalizzazione con ID {ingredientePersId} esiste"
                    : $"Ingredienti personalizzazione con ID {ingredientePersId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per ingredientePersId: {IngredientePersId}", ingredientePersId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza di ingredienti personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int persCustomId, int ingredienteId)
        {
            try
            {
                if (persCustomId <= 0 || ingredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.IngredientiPersonalizzazione
                    .AsNoTracking()
                    .AnyAsync(ip => ip.PersCustomId == persCustomId && ip.IngredienteId == ingredienteId);

                string message = exists
                    ? $"Ingredienti personalizzazione con persCustomId: {persCustomId} e con ingredienteId: {ingredienteId} esiste"
                    : $"Ingredienti personalizzazione con persCustomId: {persCustomId} e con ingredienteId: {ingredienteId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per persCustomId: {PersCustomId} e per ingredienteId: {IngredienteId}", persCustomId, ingredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza di ingredienti personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var totalCount = await _context.IngredientiPersonalizzazione.AsNoTracking().CountAsync();
                string message = totalCount switch
                {
                    0 => $"Ingrediente personalizzazione non trovata",
                    1 => $"Trovata 1 ingredienti personalizzazione",
                    _ => $"Trovate {totalCount} ingredienti personalizzazioni"

                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio di ingredienti personalizzazioni");
            }
        }
    }
}