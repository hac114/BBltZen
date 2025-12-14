using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;

namespace Repository.Service
{
    public class UnitaDiMisuraRepository(BubbleTeaContext context, ILogger<UnitaDiMisuraRepository> logger) : IUnitaDiMisuraRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<UnitaDiMisuraRepository> _logger = logger;

        private static UnitaDiMisuraDTO MapToDTO(UnitaDiMisura unita)
        {
            return new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita.UnitaMisuraId,
                Sigla = unita.Sigla,
                Descrizione = unita.Descrizione
            };
        }

        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.UnitaDiMisura
                    .AsNoTracking()
                    .OrderBy(u => u.Sigla)
                    .ThenBy(u => u.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(u => MapToDTO(u))
                    .ToListAsync();
                
                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna unità di misura trovata"
                        : $"Trovate {totalCount} unità di misura"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle unità di misura"
                };
            }
        }

        public async Task<SingleResponseDTO<UnitaDiMisuraDTO>> GetByIdAsync(int unitaId)
        {
            try
            {
                if (unitaId <= 0)
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("ID unità di misura non valido");

                var unitaDiMisura = await _context.UnitaDiMisura
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId == unitaId);

                if (unitaDiMisura == null)
                    return SingleResponseDTO<UnitaDiMisuraDTO>.NotFoundResponse(
                        $"Unità di misura con ID {unitaId} non trovata");

                return SingleResponseDTO<UnitaDiMisuraDTO>.SuccessResponse(
                    MapToDTO(unitaDiMisura),
                    $"Unità di misura con ID {unitaId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per unitaId: {UnitaId}", unitaId);
                return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse(
                    "Errore interno nel recupero dell'unità di misura");
            }
        }

        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetBySiglaAsync(string sigla, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 10))
                {
                    return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'sigla' contiene caratteri non validi"
                    };
                }

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(sigla);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'sigla' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.UnitaDiMisura
                    .AsNoTracking()
                    .Where(u => u.Sigla != null &&
                               StringHelper.StartsWithCaseInsensitive(u.Sigla, searchTerm))
                    .OrderBy(u => u.Sigla)
                    .ThenBy(u => u.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(u => MapToDTO(u))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna unità di misura trovata con sigla che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 unità di misura con sigla che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} unità di misura con sigla che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
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
                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle unità di misura per sigla"
                };
            }
        }

        public async Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                {
                    return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' contiene caratteri non validi"
                    };
                }

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.UnitaDiMisura
                    .AsNoTracking()
                    .Where(u => u.Descrizione != null &&
                               StringHelper.StartsWithCaseInsensitive(u.Descrizione, searchTerm))
                    .OrderBy(u => u.Sigla)
                    .ThenBy(u => u.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(u => MapToDTO(u))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna unità di misura trovata con descrizione che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 unità di misura con descrizione che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} unità di misura con descrizione che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
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
                return new PaginatedResponseDTO<UnitaDiMisuraDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle unità di misura per descrizione"
                };
            }
        }

        // ✅ METODI PRIVATI per uso interno (restituiscono bool)
        private async Task<bool> SiglaExistsInternalAsync(string sigla)
        {
            if (string.IsNullOrWhiteSpace(sigla))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(sigla);
            return await _context.UnitaDiMisura
                .AsNoTracking()
                .AnyAsync(u => StringHelper.EqualsCaseInsensitive(u.Sigla, searchTerm));
        }

        private async Task<bool> DescrizioneExistsInternalAsync(string descrizione)
        {
            if (string.IsNullOrWhiteSpace(descrizione))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);
            return await _context.UnitaDiMisura
                .AsNoTracking()
                .AnyAsync(u => StringHelper.EqualsCaseInsensitive(u.Descrizione, searchTerm));
        }

        private async Task<bool> SiglaExistsForOtherInternalAsync(int unitaDiMisuraId, string sigla)
        {
            if (string.IsNullOrWhiteSpace(sigla))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(sigla);
            return await _context.UnitaDiMisura
                .AsNoTracking()
                .AnyAsync(u => u.UnitaMisuraId != unitaDiMisuraId &&
                              StringHelper.EqualsCaseInsensitive(u.Sigla, searchTerm));
        }

        private async Task<bool> DescrizioneExistsForOtherInternalAsync(int unitaDiMisuraId, string descrizione)
        {
            if (string.IsNullOrWhiteSpace(descrizione))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);
            return await _context.UnitaDiMisura
                .AsNoTracking()
                .AnyAsync(u => u.UnitaMisuraId != unitaDiMisuraId &&
                              StringHelper.EqualsCaseInsensitive(u.Descrizione, searchTerm));
        }

        public async Task<SingleResponseDTO<UnitaDiMisuraDTO>> AddAsync(UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(unitaDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(unitaDto.Sigla))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("Sigla obbligatoria");

                if (string.IsNullOrWhiteSpace(unitaDto.Descrizione))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("Descrizione obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(unitaDto.Sigla, 10))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("Sigla non valida");

                if (!SecurityHelper.IsValidInput(unitaDto.Descrizione, 50))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("Descrizione non valida");

                // ✅ SOLO DOPO la validazione, normalizza
                var sigla = StringHelper.NormalizeSearchTerm(unitaDto.Sigla);
                var descrizione = StringHelper.NormalizeSearchTerm(unitaDto.Descrizione);

                // ✅ Controllo duplicati (usa metodi interni)
                if (await SiglaExistsInternalAsync(sigla))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse(
                        $"Esiste già un'unità di misura con sigla '{sigla}'");

                if (await DescrizioneExistsInternalAsync(descrizione))
                    return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse(
                        $"Esiste già un'unità di misura con descrizione '{descrizione}'");

                var unitaDiMisura = new UnitaDiMisura
                {
                    Sigla = sigla.ToUpperInvariant(),
                    Descrizione = descrizione
                };

                await _context.UnitaDiMisura.AddAsync(unitaDiMisura);
                await _context.SaveChangesAsync();

                unitaDto.UnitaMisuraId = unitaDiMisura.UnitaMisuraId;

                return SingleResponseDTO<UnitaDiMisuraDTO>.SuccessResponse(
                    unitaDto,
                    $"Unità di misura '{sigla}' creata con successo (ID: {unitaDiMisura.UnitaMisuraId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per unitaDto: {@UnitaDto}", unitaDto);
                return SingleResponseDTO<UnitaDiMisuraDTO>.ErrorResponse("Errore interno durante la creazione dell'unità di misura");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(UnitaDiMisuraDTO unitaDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(unitaDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(unitaDto.Sigla))
                    return SingleResponseDTO<bool>.ErrorResponse("Sigla obbligatoria");

                if (string.IsNullOrWhiteSpace(unitaDto.Descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(unitaDto.Sigla, 10))
                    return SingleResponseDTO<bool>.ErrorResponse("Sigla non valida");

                if (!SecurityHelper.IsValidInput(unitaDto.Descrizione, 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione non valida");

                // ✅ SOLO DOPO la validazione, normalizza
                var sigla = StringHelper.NormalizeSearchTerm(unitaDto.Sigla);
                var descrizione = StringHelper.NormalizeSearchTerm(unitaDto.Descrizione);

                var unitaDiMisura = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId == unitaDto.UnitaMisuraId);

                if (unitaDiMisura == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Unità di misura con ID {unitaDto.UnitaMisuraId} non trovata");

                // ✅ Controllo duplicati ESCLUDENDO questa unità (usa metodi interni)
                if (await SiglaExistsForOtherInternalAsync(unitaDto.UnitaMisuraId, sigla))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra unità di misura con sigla '{sigla}'");

                if (await DescrizioneExistsForOtherInternalAsync(unitaDto.UnitaMisuraId, descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra unità di misura con descrizione '{descrizione}'");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                if (!StringHelper.EqualsCaseInsensitive(unitaDiMisura.Sigla, sigla))
                {
                    unitaDiMisura.Sigla = sigla.ToUpperInvariant();
                    hasChanges = true;
                }

                if (!StringHelper.EqualsCaseInsensitive(unitaDiMisura.Descrizione, descrizione))
                {
                    unitaDiMisura.Descrizione = descrizione;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Unità di misura con ID {unitaDto.UnitaMisuraId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per l'unità di misura con ID {unitaDto.UnitaMisuraId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per unitaDto: {@UnitaDto}", unitaDto);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento dell'unità di misura");
            }
        }

        private async Task<bool> HasDependenciesAsync(int id)
        {
            bool hasDimensioneBicchiere = await _context.DimensioneBicchiere
                .AnyAsync(d => d.UnitaMisuraId == id);

            bool hasPersonalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.UnitaMisuraId == id);

            return hasDimensioneBicchiere || hasPersonalizzazioneIngrediente;
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int unitaDiMisuraId)
        {
            try
            {
                if (unitaDiMisuraId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID unità di misura non valido");

                var unita = await _context.UnitaDiMisura
                    .FirstOrDefaultAsync(u => u.UnitaMisuraId == unitaDiMisuraId);

                if (unita == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Unità di misura con ID {unitaDiMisuraId} non trovata");

                if (await HasDependenciesAsync(unitaDiMisuraId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare l'unità di misura perché ci sono dipendenze attive (bicchieri o personalizzazioni collegati)");

                _context.UnitaDiMisura.Remove(unita);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Unità di misura '{unita.Sigla}' (ID: {unitaDiMisuraId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per unitaDiMisuraId: {UnitaDiMisuraId}", unitaDiMisuraId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione dell'unità di misura");
            }
        }        

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int unitaDiMisuraId)
        {
            try
            {
                if (unitaDiMisuraId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID unità di misura non valido");

                var exists = await _context.UnitaDiMisura
                    .AsNoTracking()
                    .AnyAsync(u => u.UnitaMisuraId == unitaDiMisuraId);

                string message = exists
                    ? $"Unità di misura con ID {unitaDiMisuraId} esiste"
                    : $"Unità di misura con ID {unitaDiMisuraId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per unitaDiMisuraId: {UnitaDiMisuraId}", unitaDiMisuraId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'unità di misura");
            }
        }

        public async Task<SingleResponseDTO<bool>> SiglaExistsAsync(string sigla)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sigla))
                    return SingleResponseDTO<bool>.ErrorResponse("La sigla è obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 10))
                    return SingleResponseDTO<bool>.ErrorResponse("La sigla contiene caratteri non validi");

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(sigla);

                var exists = await _context.UnitaDiMisura
                    .AsNoTracking()
                    .AnyAsync(u => StringHelper.EqualsCaseInsensitive(u.Sigla, searchTerm));

                string message = exists
                    ? $"Unità di misura con sigla '{searchTerm}' esiste"
                    : $"Unità di misura con sigla '{searchTerm}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in SiglaExistsAsync per sigla: {Sigla}", sigla);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza per sigla");
            }
        }

        public async Task<SingleResponseDTO<bool>> DescrizioneExistsAsync(string descrizione)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione è obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione contiene caratteri non validi");

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                var exists = await _context.UnitaDiMisura
                    .AsNoTracking()
                    .AnyAsync(u => StringHelper.EqualsCaseInsensitive(u.Descrizione, searchTerm));

                string message = exists
                    ? $"Unità di misura con descrizione '{searchTerm}' esiste"
                    : $"Unità di misura con descrizione '{searchTerm}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DescrizioneExistsAsync per descrizione: {Descrizione}", descrizione);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza per descrizione");
            }
        }
    }
}