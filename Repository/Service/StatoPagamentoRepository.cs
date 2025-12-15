using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Comparers;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class StatoPagamentoRepository(BubbleTeaContext context, ILogger<StatoPagamentoRepository> logger) : IStatoPagamentoRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<StatoPagamentoRepository> _logger = logger;

        private static StatoPagamentoDTO MapToDTO(StatoPagamento statoPagamento)
        {
            return new StatoPagamentoDTO
            {
                StatoPagamentoId = statoPagamento.StatoPagamentoId,
                StatoPagamento1 = statoPagamento.StatoPagamento1
            };
        }

        public async Task<PaginatedResponseDTO<StatoPagamentoDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // 1. Query base SENZA ordinamento con comparatore
                var query = _context.StatoPagamento.AsNoTracking();

                var totalCount = await query.CountAsync();

                // 2. Porta i dati in memoria PRIMA di ordinare
                var allItems = await query.ToListAsync();

                // 3. Applica ordinamento personalizzato IN MEMORIA
                var orderedItems = allItems
                    .OrderBy(s => s.StatoPagamento1, new StatoPagamentoComparer())
                    .ThenBy(s => s.StatoPagamentoId)
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(s => MapToDTO(s))
                    .ToList();

                return new PaginatedResponseDTO<StatoPagamentoDTO>
                {
                    Data = orderedItems,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuno stato pagamento trovato"
                        : $"Trovati {totalCount} stati pagamento"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<StatoPagamentoDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli stati pagamento"
                };
            }
        }

        public async Task<SingleResponseDTO<StatoPagamentoDTO>> GetByIdAsync(int statoPagamentoId)
        {
            try
            {
                if (statoPagamentoId <= 0)
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse("ID stato pagamento non valido");

                var statoPagamento = await _context.StatoPagamento
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoId);

                if (statoPagamento == null)
                    return SingleResponseDTO<StatoPagamentoDTO>.NotFoundResponse(
                        $"Stato pagamento con ID {statoPagamentoId} non trovato");

                return SingleResponseDTO<StatoPagamentoDTO>.SuccessResponse(
                    MapToDTO(statoPagamento),
                    $"Stato pagamento con ID {statoPagamentoId} trovato ({statoPagamento.StatoPagamento1})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per statoPagamentoId: {StatoPagamentoId}", statoPagamentoId);
                return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                    "Errore interno nel recupero dello stato pagamento");
            }
        }

        public async Task<SingleResponseDTO<StatoPagamentoDTO>> GetByNomeAsync(string nomeStatoPagamento)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nomeStatoPagamento, maxLength: 100))
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoPagamento' contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(nomeStatoPagamento);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoPagamento' è obbligatorio");

                var statoPagamento = await _context.StatoPagamento
                    .AsNoTracking()
                    .Where(s => s.StatoPagamento1 != null &&
                               StringHelper.EqualsCaseInsensitive(s.StatoPagamento1, searchTerm))
                    .Select(s => MapToDTO(s))
                    .FirstOrDefaultAsync();

                if (statoPagamento == null)
                    return SingleResponseDTO<StatoPagamentoDTO>.NotFoundResponse(
                        $"Nessuno stato pagamento trovato con nome '{searchTerm}'");

                return SingleResponseDTO<StatoPagamentoDTO>.SuccessResponse(
                    statoPagamento,
                    $"Stato pagamento '{searchTerm}' trovato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByNomeAsync per nomeStatoPagamento: {NomeStatoPagamento}", nomeStatoPagamento);
                return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                    "Errore interno nel recupero dello stato pagamento per nome");
            }
        }

        // ✅ METODO INTERNO per verificare esistenza (senza validazione sicurezza)
        private async Task<bool> ExistsByStatoPagamentoInternalAsync(string statoPagamento)
        {
            if (string.IsNullOrWhiteSpace(statoPagamento)) return false;
            var searchTerm = StringHelper.NormalizeSearchTerm(statoPagamento);
            return await _context.StatoPagamento
                .AsNoTracking()
                .AnyAsync(s => StringHelper.EqualsCaseInsensitive(s.StatoPagamento1, searchTerm));
        }        

        public async Task<SingleResponseDTO<StatoPagamentoDTO>> AddAsync(StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(statoPagamentoDto);

                if (string.IsNullOrWhiteSpace(statoPagamentoDto.StatoPagamento1))
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse("Nome stato pagamento obbligatorio");

                if (!SecurityHelper.IsValidInput(statoPagamentoDto.StatoPagamento1, 100))
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse("Nome stato pagamento non valido");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoPagamentoDto.StatoPagamento1);

                if (await ExistsByStatoPagamentoInternalAsync(searchTerm))
                    return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                        $"Esiste già uno stato pagamento '{searchTerm}'");

                var statoPagamento = new StatoPagamento
                {
                    StatoPagamento1 = searchTerm                    
                };

                await _context.StatoPagamento.AddAsync(statoPagamento);
                await _context.SaveChangesAsync();

                statoPagamentoDto.StatoPagamentoId = statoPagamento.StatoPagamentoId;
                statoPagamentoDto.StatoPagamento1 = statoPagamento.StatoPagamento1;

                return SingleResponseDTO<StatoPagamentoDTO>.SuccessResponse(
                    statoPagamentoDto,
                    $"Stato pagamento '{searchTerm}' creato con successo (ID: {statoPagamento.StatoPagamentoId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per stato pagamento: {statoPagamentoDto}", statoPagamentoDto?.StatoPagamento1);
                return SingleResponseDTO<StatoPagamentoDTO>.ErrorResponse(
                    "Errore interno durante la creazione dello stato pagamento");
            }
        }

        // ✅ METODO INTERNO per verificare esistenza ESCLUDENDO un ID
        private async Task<bool> ExistsByStatoPagamentoInternalAsync(int excludeId, string statoPagamento)
        {
            if (string.IsNullOrWhiteSpace(statoPagamento)) return false;
            var searchTerm = StringHelper.NormalizeSearchTerm(statoPagamento);
            return await _context.StatoPagamento
                .AsNoTracking()
                .AnyAsync(s => s.StatoPagamentoId != excludeId &&
                              StringHelper.EqualsCaseInsensitive(s.StatoPagamento1, searchTerm));
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(statoPagamentoDto);

                if (string.IsNullOrWhiteSpace(statoPagamentoDto.StatoPagamento1))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome stato pagamento obbligatorio");

                if (!SecurityHelper.IsValidInput(statoPagamentoDto.StatoPagamento1, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome stato pagamento non valido");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoPagamentoDto.StatoPagamento1);

                var statoPagamento = await _context.StatoPagamento
                    .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoDto.StatoPagamentoId);

                if (statoPagamento == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Stato pagamento con ID {statoPagamentoDto.StatoPagamentoId} non trovato");

                if (await ExistsByStatoPagamentoInternalAsync(statoPagamentoDto.StatoPagamentoId, searchTerm))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altro stato pagamento con nome '{searchTerm}'");

                bool hasChanges = false;

                if (!StringHelper.EqualsCaseInsensitive(statoPagamento.StatoPagamento1, searchTerm))
                {
                    statoPagamento.StatoPagamento1 = searchTerm;
                    hasChanges = true;
                }                

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Stato pagamento con ID {statoPagamentoDto.StatoPagamentoId} aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per lo stato pagamento con ID {statoPagamentoDto.StatoPagamentoId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per statoPagamentoId: {StatoPagamentoId}", statoPagamentoDto?.StatoPagamentoId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'aggiornamento dello stato pagamento");
            }
        }

        private async Task<bool> HasDependenciesAsync(int statoPagamentoId)
        {
            try
            {
                _logger.LogInformation("Verifica dipendenze per stato pagamento ID: {StatoPagamentoId}", statoPagamentoId);

                bool hasOrdine = await _context.Ordine
                    .AnyAsync(o => o.StatoPagamentoId == statoPagamentoId);                

                // Log dei risultati
                _logger.LogInformation(
                    "Dipendenze trovate per stato pagamento ID {StatoPagamentoId}: " +
                    "Ordine={Ordine}", statoPagamentoId, hasOrdine);

                return hasOrdine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in HasDependenciesAsync per statoPagamentoId: {StatoPagamentoId}", statoPagamentoId);
                // In caso di errore, per sicurezza restituiamo true (ci sono dipendenze)
                return true;
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int statoPagamentoId)
        {
            try
            {
                _logger.LogInformation("Tentativo eliminazione stato pagamento ID: {StatoPagamentoId}", statoPagamentoId);

                if (statoPagamentoId <= 0)
                {
                    _logger.LogWarning("ID stato pagamento non valido: {StatoPagamentoId}", statoPagamentoId);
                    return SingleResponseDTO<bool>.ErrorResponse("ID stato pagamento non valido");
                }

                var statoPagamento = await _context.StatoPagamento
                    .FirstOrDefaultAsync(s => s.StatoPagamentoId == statoPagamentoId);

                if (statoPagamento == null)
                {
                    _logger.LogWarning("Stato pagamento con ID {StatoPagamentoId} non trovato", statoPagamentoId);
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Stato pagamento con ID {statoPagamentoId} non trovato");
                }

                _logger.LogInformation("Trovato stato pagamento: ID={StatoPagamentoId}, Nome={Nome}",
                    statoPagamentoId, statoPagamento.StatoPagamento1);

                // Verifica dipendenze
                var hasDependencies = await HasDependenciesAsync(statoPagamentoId);

                if (hasDependencies)
                {
                    _logger.LogWarning("Impossibile eliminare stato pagamento ID {StatoPagamentoId}: ci sono dipendenze", statoPagamentoId);
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare lo stato pagamento perché ci sono dipendenze collegate");
                }

                _logger.LogInformation("Procedo con eliminazione stato pagamento ID: {StatoPagamentoId} - {Nome}",
                    statoPagamentoId, statoPagamento.StatoPagamento1);

                // Tentativo di eliminazione
                _context.StatoPagamento.Remove(statoPagamento);
                var saveResult = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChangesAsync completato. Record modificati: {RecordCount}", saveResult);
                _logger.LogInformation("Stato pagamento ID: {StatoPagamentoId} eliminato con successo", statoPagamentoId);

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Stato pagamento '{statoPagamento.StatoPagamento1}' (ID: {statoPagamentoId}) eliminato con successo");
            }
            catch (DbUpdateException dbEx)
            {
                // Errore specifico di database (FK constraint, etc.)
                _logger.LogError(dbEx, "Errore DB durante eliminazione statoPagamentoId: {StatoPagamentoId}. Inner: {InnerMessage}",
                    statoPagamentoId, dbEx.InnerException?.Message);

                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore di database durante l'eliminazione. Verificare che non ci siano dipendenze attive.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore generico in DeleteAsync per statoPagamentoId: {StatoPagamentoId}", statoPagamentoId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione dello stato pagamento");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int statoPagamentoId)
        {
            try
            {
                if (statoPagamentoId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID stato pagamento non valido");

                var exists = await _context.StatoPagamento
                    .AsNoTracking()
                    .AnyAsync(s => s.StatoPagamentoId == statoPagamentoId);

                string message = exists
                    ? $"Stato pagamento con ID {statoPagamentoId} esiste"
                    : $"Stato pagamento con ID {statoPagamentoId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per statoPagamentoId: {StatoPagamentoId}", statoPagamentoId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza dello stato pagamento");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string statoPagamento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statoPagamento))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome dello stato pagamento è obbligatorio");

                if (!SecurityHelper.IsValidInput(statoPagamento, maxLength: 100))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Il nome dello stato pagamento contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoPagamento);
                var exists = await _context.StatoPagamento
                    .AsNoTracking()
                    .AnyAsync(s => StringHelper.EqualsCaseInsensitive(s.StatoPagamento1, searchTerm));

                string message = exists
                    ? $"Stato pagamento con nome '{searchTerm}' esiste"
                    : $"Stato pagamento con nome '{searchTerm}' non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByNomeAsync per statoPagamento: {StatoPagamento1}", statoPagamento);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza dello stato pagamento per nome");
            }
        }
    }
}