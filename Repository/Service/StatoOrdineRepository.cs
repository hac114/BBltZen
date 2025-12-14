using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Comparers;
using Repository.Helper;
using Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class StatoOrdineRepository(BubbleTeaContext context, ILogger<StatoOrdineRepository> logger) : IStatoOrdineRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<StatoOrdineRepository> _logger = logger;

        private static StatoOrdineDTO MapToDTO(StatoOrdine statoOrdine)
        {
            return new StatoOrdineDTO
            {
                StatoOrdineId = statoOrdine.StatoOrdineId,
                StatoOrdine1 = statoOrdine.StatoOrdine1,
                Terminale = statoOrdine.Terminale
            };
        }

        public async Task<PaginatedResponseDTO<StatoOrdineDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.StatoOrdine
                    .AsNoTracking()
                    .OrderBy(s => s.StatoOrdine1, new StatoOrdineComparer())
                    .ThenBy(s => s.StatoOrdineId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                // ✅ CORRETTO: messaggio appropriato per stati ordine
                return new PaginatedResponseDTO<StatoOrdineDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuno stato ordine trovato"
                        : $"Trovati {totalCount} stati ordine"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<StatoOrdineDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli stati ordine"
                };
            }
        }

        public async Task<SingleResponseDTO<StatoOrdineDTO>> GetByIdAsync(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse("ID stato ordine non valido");

                var statoOrdine = await _context.StatoOrdine
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineId);

                if (statoOrdine == null)
                    return SingleResponseDTO<StatoOrdineDTO>.NotFoundResponse(
                        $"Stato ordine con ID {statoOrdineId} non trovato");

                return SingleResponseDTO<StatoOrdineDTO>.SuccessResponse(
                    MapToDTO(statoOrdine),
                    $"Stato ordine con ID {statoOrdineId} trovato ({statoOrdine.StatoOrdine1})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per statoOrdineId: {statoOrdineId}", statoOrdineId);
                return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                    "Errore interno nel recupero dello stato ordine");
            }
        }

        public async Task<SingleResponseDTO<StatoOrdineDTO>> GetByNomeAsync(string nomeStatoOrdine)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nomeStatoOrdine, maxLength: 100))
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoOrdine' contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(nomeStatoOrdine);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoOrdine' è obbligatorio");

                var statoOrdine = await _context.StatoOrdine
                    .AsNoTracking()
                    .Where(s => s.StatoOrdine1 != null &&
                               StringHelper.EqualsCaseInsensitive(s.StatoOrdine1, searchTerm))
                    .Select(s => MapToDTO(s))
                    .FirstOrDefaultAsync();

                if (statoOrdine == null)
                    return SingleResponseDTO<StatoOrdineDTO>.NotFoundResponse(
                        $"Nessuno stato ordine trovato con nome '{searchTerm}'");

                return SingleResponseDTO<StatoOrdineDTO>.SuccessResponse(
                    statoOrdine,
                    $"Stato ordine '{searchTerm}' trovato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByNomeAsync per nomeStatoOrdine: {NomeStatoOrdine}", nomeStatoOrdine);
                return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                    "Errore interno nel recupero dello stato ordine per nome");
            }
        }

        public async Task<PaginatedResponseDTO<StatoOrdineDTO>> GetStatiNonTerminaliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.StatoOrdine
                    .AsNoTracking()
                    .Where(s => s.Terminale == false)
                    .OrderBy(s => s.StatoOrdine1, new StatoOrdineComparer())
                    .ThenBy(s => s.StatoOrdineId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                string message;
                if (totalCount == 0) message = "Nessuno stato non terminale trovato";
                else if (totalCount == 1) message = "Trovato 1 stato non terminale";
                else message = $"Trovati {totalCount} stati non terminali";

                return new PaginatedResponseDTO<StatoOrdineDTO>
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
                _logger.LogError(ex, "Errore in GetStatiNonTerminaliAsync");
                return new PaginatedResponseDTO<StatoOrdineDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli stati non terminali"
                };
            }
        }

        public async Task<PaginatedResponseDTO<StatoOrdineDTO>> GetStatiTerminaliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.StatoOrdine
                    .AsNoTracking()
                    .Where(s => s.Terminale == true)
                    .OrderBy(s => s.StatoOrdine1, new StatoOrdineComparer())
                    .ThenBy(s => s.StatoOrdineId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                string message;
                if (totalCount == 0) message = "Nessuno stato terminale trovato";
                else if (totalCount == 1) message = "Trovato 1 stato terminale";
                else message = $"Trovati {totalCount} stati terminali";

                return new PaginatedResponseDTO<StatoOrdineDTO>
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
                _logger.LogError(ex, "Errore in GetStatiTerminaliAsync");
                return new PaginatedResponseDTO<StatoOrdineDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli stati terminali"
                };
            }
        }

        // ✅ METODO INTERNO per verificare esistenza (senza validazione sicurezza)
        private async Task<bool> ExistsByStatoOrdineInternalAsync(string statoOrdine)
        {
            if (string.IsNullOrWhiteSpace(statoOrdine)) return false;
            var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdine);
            return await _context.StatoOrdine
                .AsNoTracking()
                .AnyAsync(s => StringHelper.EqualsCaseInsensitive(s.StatoOrdine1, searchTerm));
        }

        // ✅ METODO INTERNO per verificare esistenza ESCLUDENDO un ID
        private async Task<bool> ExistsByStatoOrdineInternalAsync(int excludeId, string statoOrdine)
        {
            if (string.IsNullOrWhiteSpace(statoOrdine)) return false;
            var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdine);
            return await _context.StatoOrdine
                .AsNoTracking()
                .AnyAsync(s => s.StatoOrdineId != excludeId &&
                              StringHelper.EqualsCaseInsensitive(s.StatoOrdine1, searchTerm));
        }

        public async Task<SingleResponseDTO<StatoOrdineDTO>> AddAsync(StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(statoOrdineDto);

                if (string.IsNullOrWhiteSpace(statoOrdineDto.StatoOrdine1))
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse("Nome stato ordine obbligatorio");

                if (!SecurityHelper.IsValidInput(statoOrdineDto.StatoOrdine1, 100))
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse("Nome stato ordine non valido");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdineDto.StatoOrdine1);

                if (await ExistsByStatoOrdineInternalAsync(searchTerm))
                    return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                        $"Esiste già uno stato ordine '{searchTerm}'");

                var statoOrdine = new StatoOrdine
                {
                    StatoOrdine1 = searchTerm,
                    Terminale = statoOrdineDto.Terminale
                };

                await _context.StatoOrdine.AddAsync(statoOrdine);
                await _context.SaveChangesAsync();

                statoOrdineDto.StatoOrdineId = statoOrdine.StatoOrdineId;
                statoOrdineDto.StatoOrdine1 = statoOrdine.StatoOrdine1;

                return SingleResponseDTO<StatoOrdineDTO>.SuccessResponse(
                    statoOrdineDto,
                    $"Stato ordine '{searchTerm}' creato con successo (ID: {statoOrdine.StatoOrdineId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per stato ordine: {statoOrdineDto}", statoOrdineDto?.StatoOrdine1);
                return SingleResponseDTO<StatoOrdineDTO>.ErrorResponse(
                    "Errore interno durante la creazione dello stato ordine");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(statoOrdineDto);

                if (string.IsNullOrWhiteSpace(statoOrdineDto.StatoOrdine1))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome stato ordine obbligatorio");

                if (!SecurityHelper.IsValidInput(statoOrdineDto.StatoOrdine1, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome stato ordine non valido");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdineDto.StatoOrdine1);

                var statoOrdine = await _context.StatoOrdine
                    .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineDto.StatoOrdineId);

                if (statoOrdine == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Stato ordine con ID {statoOrdineDto.StatoOrdineId} non trovato");

                if (await ExistsByStatoOrdineInternalAsync(statoOrdineDto.StatoOrdineId, searchTerm))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altro stato ordine con nome '{searchTerm}'");

                bool hasChanges = false;

                if (!StringHelper.EqualsCaseInsensitive(statoOrdine.StatoOrdine1, searchTerm))
                {
                    statoOrdine.StatoOrdine1 = searchTerm;
                    hasChanges = true;
                }

                // ✅ AGGIUNTO: Controllo per aggiornare Terminale
                if (statoOrdine.Terminale != statoOrdineDto.Terminale)
                {
                    statoOrdine.Terminale = statoOrdineDto.Terminale;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Stato ordine con ID {statoOrdineDto.StatoOrdineId} aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per lo stato ordine con ID {statoOrdineDto.StatoOrdineId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per statoOrdineId: {StatoOrdineId}", statoOrdineDto?.StatoOrdineId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'aggiornamento dello stato ordine");
            }
        }

        private async Task<bool> HasDependenciesAsync(int statoOrdineId)
        {
            bool hasConfigSoglieTempi = await _context.ConfigSoglieTempi
                .AnyAsync(c => c.StatoOrdineId == statoOrdineId);
            bool hasOrdine = await _context.Ordine
                .AnyAsync(o => o.StatoOrdineId == statoOrdineId);
            bool hasStatoStoricoOrdine = await _context.StatoStoricoOrdine
                .AnyAsync(s => s.StatoOrdineId == statoOrdineId);
            return hasConfigSoglieTempi || hasOrdine || hasStatoStoricoOrdine;
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID stato ordine non valido");

                var statoOrdine = await _context.StatoOrdine
                    .FirstOrDefaultAsync(s => s.StatoOrdineId == statoOrdineId);

                if (statoOrdine == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Stato ordine con ID {statoOrdineId} non trovato");

                if (await HasDependenciesAsync(statoOrdineId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare lo stato ordine perché ci sono dipendenze collegate");

                _context.StatoOrdine.Remove(statoOrdine);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Stato ordine '{statoOrdine.StatoOrdine1}' (ID: {statoOrdineId}) eliminato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per statoOrdineId: {StatoOrdineId}", statoOrdineId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione dello stato ordine");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID stato ordine non valido");

                var exists = await _context.StatoOrdine
                    .AsNoTracking()
                    .AnyAsync(s => s.StatoOrdineId == statoOrdineId);

                string message = exists
                    ? $"Stato ordine con ID {statoOrdineId} esiste"
                    : $"Stato ordine con ID {statoOrdineId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per statoOrdineId: {StatoOrdineId}", statoOrdineId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza dello stato ordine");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string statoOrdine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statoOrdine))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome dello stato ordine è obbligatorio");

                if (!SecurityHelper.IsValidInput(statoOrdine, maxLength: 100))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Il nome dello stato ordine contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdine);
                var exists = await _context.StatoOrdine
                    .AsNoTracking()
                    .AnyAsync(s => StringHelper.EqualsCaseInsensitive(s.StatoOrdine1, searchTerm));

                string message = exists
                    ? $"Stato ordine con nome '{searchTerm}' esiste"
                    : $"Stato ordine con nome '{searchTerm}' non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByNomeAsync per statoOrdine: {StatoOrdine1}", statoOrdine);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza dello stato ordine per nome");
            }
        }
    }
}