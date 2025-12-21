using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class ArticoloRepository(BubbleTeaContext context, ILogger<ArticoloRepository> logger) : IArticoloRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<ArticoloRepository> _logger = logger;

        private static ArticoloDTO MapToDTO(Articolo articolo)
        {
            return new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = articolo.Tipo,
                DataCreazione = articolo.DataCreazione,
                DataAggiornamento = articolo.DataAggiornamento
            };
        }

        public async Task<PaginatedResponseDTO<ArticoloDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Articolo
                    .AsNoTracking()
                    .OrderBy(a => a.Tipo);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                return new PaginatedResponseDTO<ArticoloDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessun articolo trovato"
                        : $"Trovati {totalCount} articoli"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<ArticoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli articoli"
                };
            }
        }

        public async Task<SingleResponseDTO<ArticoloDTO>> GetByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<ArticoloDTO>.ErrorResponse("ID articolo non valido");

                var articolo = await _context.Articolo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo == null)
                    return SingleResponseDTO<ArticoloDTO>.NotFoundResponse(
                        $"Articolo con ID {articoloId} non trovato");

                return SingleResponseDTO<ArticoloDTO>.SuccessResponse(
                    MapToDTO(articolo),
                    $"Articolo con ID {articoloId} trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<ArticoloDTO>.ErrorResponse(
                    "Errore interno nel recupero dell'articolo");
            }
        }

        public async Task<PaginatedResponseDTO<ArticoloDTO>> GetByTipoAsync(string tipo, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ CORREZIONE: Controlla se tipo è nullo prima
                if (string.IsNullOrWhiteSpace(tipo))
                    return new PaginatedResponseDTO<ArticoloDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo' è obbligatorio"
                    };

                // ✅ CORREZIONE: Validazione sicurezza con controllo valori specifici
                if (!SecurityHelper.IsValidInput(tipo, maxLength: 2))
                {
                    return new PaginatedResponseDTO<ArticoloDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo' contiene caratteri non validi"
                    };
                }

                // ✅ CORREZIONE: Normalizza e converte in maiuscolo per match esatto
                var searchTerm = StringHelper.NormalizeSearchTerm(tipo).ToUpper();

                // ✅ CORREZIONE: Verifica che sia un valore valido
                if (!new[] { "BC", "BS", "D" }.Contains(searchTerm))
                {
                    return new PaginatedResponseDTO<ArticoloDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Tipo non valido. Valori ammessi: BC, BS, D"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ CORREZIONE: Query per match ESATTO (non "inizia con")
                var query = _context.Articolo
                    .AsNoTracking()
                    .Where(a => a.Tipo == searchTerm)  // Match esatto, case-sensitive
                    .OrderBy(a => a.ArticoloId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(a => MapToDTO(a))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessun articolo trovato con tipo '{searchTerm}'",
                    1 => $"Trovato 1 articolo con tipo '{searchTerm}'",
                    _ => $"Trovati {totalCount} articoli con tipo '{searchTerm}'",
                };

                return new PaginatedResponseDTO<ArticoloDTO>
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
                _logger.LogError(ex, "Errore in GetByTipoAsync per tipo: {tipo}", tipo);
                return new PaginatedResponseDTO<ArticoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli articoli in base al tipo"
                };
            }
        }

        public async Task<SingleResponseDTO<ArticoloDTO>> AddAsync(ArticoloDTO articoloDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(articoloDto);

                // ✅ Validazioni input base
                if (string.IsNullOrWhiteSpace(articoloDto.Tipo))
                    return SingleResponseDTO<ArticoloDTO>.ErrorResponse("Tipo articolo obbligatorio");

                // ✅ PRIMA normalizza, POI valida
                var tipoNormalizzato = StringHelper.NormalizeSearchTerm(articoloDto.Tipo).ToUpper();

                // ✅ Validazione sicurezza sulla stringa NORMALIZZATA
                if (!SecurityHelper.IsValidInput(tipoNormalizzato, 2))
                    return SingleResponseDTO<ArticoloDTO>.ErrorResponse("Tipo non valido (contiene caratteri pericolosi o troppo lungo)");

                // ✅ Validazione specifica valori consentiti
                if (!new[] { "BC", "BS", "D" }.Contains(tipoNormalizzato))
                    return SingleResponseDTO<ArticoloDTO>.ErrorResponse("Tipo non valido. Valori ammessi: BC, BS, D");

                // ✅ Verifica lunghezza: 1 o 2 caratteri
                if (tipoNormalizzato.Length < 1 || tipoNormalizzato.Length > 2)
                    return SingleResponseDTO<ArticoloDTO>.ErrorResponse("Il tipo deve essere di 1 o 2 caratteri");

                // ✅ Crea entità con la STESSA data
                var now = DateTime.UtcNow;
                var articolo = new Articolo
                {
                    Tipo = tipoNormalizzato,
                    DataCreazione = now,
                    DataAggiornamento = now
                };

                await _context.Articolo.AddAsync(articolo);
                await _context.SaveChangesAsync();

                articoloDto.ArticoloId = articolo.ArticoloId;
                articoloDto.DataCreazione = articolo.DataCreazione;
                articoloDto.DataAggiornamento = articolo.DataAggiornamento;

                return SingleResponseDTO<ArticoloDTO>.SuccessResponse(articoloDto,
                    $"Articolo di tipo '{articolo.Tipo}' creato con successo (ID: {articolo.ArticoloId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per tipo: {tipo}", articoloDto?.Tipo);
                return SingleResponseDTO<ArticoloDTO>.ErrorResponse("Errore interno durante la creazione dell'articolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(ArticoloDTO articoloDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(articoloDto);

                // ✅ Validazione ID
                if (articoloDto.ArticoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                // ✅ Validazione obbligatorietà
                if (string.IsNullOrWhiteSpace(articoloDto.Tipo))
                    return SingleResponseDTO<bool>.ErrorResponse("Il tipo articolo è obbligatorio");

                // ✅ Normalizzazione e validazione
                var tipoNormalizzato = StringHelper.NormalizeSearchTerm(articoloDto.Tipo).ToUpper();

                // ✅ Validazione sicurezza sulla stringa NORMALIZZATA
                if (!SecurityHelper.IsValidInput(tipoNormalizzato, 2))
                    return SingleResponseDTO<bool>.ErrorResponse("Tipo non valido (contiene caratteri pericolosi o troppo lungo)");

                // ✅ Validazione specifica valori consentiti
                if (!new[] { "BC", "BS", "D" }.Contains(tipoNormalizzato))
                    return SingleResponseDTO<bool>.ErrorResponse("Tipo non valido. Valori ammessi: BC, BS, D");

                // ✅ Verifica lunghezza: 1 o 2 caratteri
                if (tipoNormalizzato.Length < 1 || tipoNormalizzato.Length > 2)
                    return SingleResponseDTO<bool>.ErrorResponse("Il tipo deve essere di 1 o 2 caratteri");

                // ✅ Recupera articolo
                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloDto.ArticoloId);

                if (articolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Articolo con ID {articoloDto.ArticoloId} non trovato");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                if (articolo.Tipo != tipoNormalizzato)
                {
                    articolo.Tipo = tipoNormalizzato;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    // ✅ AGGIORNA SOLO DATA AGGIORNAMENTO (non DataCreazione!)
                    articolo.DataAggiornamento = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Articolo con ID {articoloDto.ArticoloId} aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per l'articolo con ID {articoloDto.ArticoloId}");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database in UpdateAsync per articoloId: {articoloId}",
                    articoloDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore database durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per articoloId: {articoloId}",
                    articoloDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento");
            }
        }

        private async Task<bool> HasDependenciesAsync(int articoloId)
        {
            // ✅ CORREZIONE: Controlla TUTTE le dipendenze
            return await _context.OrderItem.AnyAsync(o => o.ArticoloId == articoloId) ||
                   await _context.BevandaCustom.AnyAsync(bc => bc.ArticoloId == articoloId) ||
                   await _context.BevandaStandard.AnyAsync(bs => bs.ArticoloId == articoloId) ||
                   await _context.Dolce.AnyAsync(d => d.ArticoloId == articoloId);
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(i => i.ArticoloId == articoloId);

                if (articolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Articolo con ID {articoloId} non trovato");

                // ✅ Controllo dipendenze esteso
                if (await HasDependenciesAsync(articoloId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare l'articolo perché ci sono dipendenze collegate");

                _context.Articolo.Remove(articolo);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Articolo di tipo '{articolo.Tipo}' (ID: {articoloId}) eliminato con successo");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database in DeleteAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore database durante l'eliminazione. Potrebbero esserci dipendenze attive.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione dell'articolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var exists = await _context.Articolo
                    .AsNoTracking()
                    .AnyAsync(a => a.ArticoloId == articoloId);

                string message = exists
                    ? $"Articolo con ID {articoloId} esiste"
                    : $"Articolo con ID {articoloId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica esistenza articolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByTipoAsync(string tipo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipo))
                    return SingleResponseDTO<bool>.ErrorResponse("Il tipo è obbligatorio");

                if (!SecurityHelper.IsValidInput(tipo, maxLength: 2))
                    return SingleResponseDTO<bool>.ErrorResponse("Il tipo contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(tipo).ToUpper();

                // ✅ CORREZIONE: Controlla se il tipo è valido
                if (!new[] { "BC", "BS", "D" }.Contains(searchTerm))
                    return SingleResponseDTO<bool>.ErrorResponse("Tipo non valido. Valori ammessi: BC, BS, D");

                var exists = await _context.Articolo
                    .AsNoTracking()
                    .AnyAsync(a => a.Tipo == searchTerm);  // Match esatto case-sensitive

                string message = exists
                    ? $"Esiste almeno un articolo di tipo '{searchTerm}'"
                    : $"Nessun articolo trovato di tipo '{searchTerm}'";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByTipoAsync per tipo: {tipo}", tipo);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'articolo");
            }
        }
    }
}