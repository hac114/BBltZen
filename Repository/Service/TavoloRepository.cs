using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TavoloRepository(BubbleTeaContext context, ILogger<TavoloRepository> logger) : ITavoloRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<TavoloRepository> _logger = logger;

        private static TavoloDTO MapToDTO(Tavolo tavolo)
        {
            return new TavoloDTO
            {
                TavoloId = tavolo.TavoloId,
                Numero = tavolo.Numero,
                Disponibile = tavolo.Disponibile,
                Zona = tavolo.Zona
            };
        }       

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. Validazione paginazione (SOLO questa è ESSENZIALE)
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 2. Query base SEMPLICE
                var query = _context.Tavolo
                    .AsNoTracking()
                    .OrderBy(t => t.Numero); // Ordinamento fisso, niente parametri extra

                // ✅ 3. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                // ✅ 4. Risposta pulita
                return new PaginatedResponseDTO<TavoloDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessun tavolo trovato"
                        : $"Trovati {totalCount} tavoli"
                };
            }
            catch (Exception)
            {
                // ✅ 5. Gestione errori minimale
                return new PaginatedResponseDTO<TavoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei tavoli"
                };
            }
        }

        public async Task<SingleResponseDTO<TavoloDTO>> GetByIdAsync(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SingleResponseDTO<TavoloDTO>.ErrorResponse("ID tavolo non valido");

                var tavolo = await _context.Tavolo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TavoloId == tavoloId);

                if (tavolo == null)
                    return SingleResponseDTO<TavoloDTO>.NotFoundResponse(
                        $"Tavolo con ID {tavoloId} non trovato");

                return SingleResponseDTO<TavoloDTO>.SuccessResponse(
                    MapToDTO(tavolo),
                    $"Tavolo con ID {tavoloId} trovato (Numero: {tavolo.Numero})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per tavoloId: {TavoloId}", tavoloId);
                return SingleResponseDTO<TavoloDTO>.ErrorResponse(
                    "Errore interno nel recupero del tavolo");
            }
        }

        public async Task<SingleResponseDTO<TavoloDTO>> GetByNumeroAsync(int numero)
        {
            try
            {
                if (numero <= 0)
                    return SingleResponseDTO<TavoloDTO>.ErrorResponse("Il numero tavolo deve essere maggiore di 0");

                var tavolo = await _context.Tavolo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Numero == numero);

                if (tavolo == null)
                    return SingleResponseDTO<TavoloDTO>.NotFoundResponse(
                        $"Tavolo con numero {numero} non trovato");

                return SingleResponseDTO<TavoloDTO>.SuccessResponse(
                    MapToDTO(tavolo),
                    $"Tavolo con numero {numero} trovato (ID: {tavolo.TavoloId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByNumeroAsync per numero: {Numero}", numero);
                return SingleResponseDTO<TavoloDTO>.ErrorResponse(
                    "Errore interno nel recupero del tavolo per numero");
            }
        }

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. Validazione paginazione (coerente con GetAllAsync)
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 2. Query semplice (solo aggiunto Where per disponibilità)
                var query = _context.Tavolo
                    .AsNoTracking()
                    .Where(t => t.Disponibile)
                    .OrderBy(t => t.Numero);

                // ✅ 3. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                // ✅ 4. Messaggio specifico per disponibili
                string message;
                if (totalCount == 0)
                {
                    message = "Nessun tavolo disponibile trovato";
                }
                else if (totalCount == 1)
                {
                    message = "Trovato 1 tavolo disponibile";
                }
                else
                {
                    message = $"Trovati {totalCount} tavoli disponibili";
                }

                return new PaginatedResponseDTO<TavoloDTO>
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
                // ✅ 5. Gestione errori minimale (coerente)
                return new PaginatedResponseDTO<TavoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei tavoli disponibili"
                };
            }
        }
        
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetOccupatiAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Tavolo
                    .AsNoTracking()
                    .Where(t => !t.Disponibile)
                    .OrderBy(t => t.Numero);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                string message = totalCount == 0
                    ? "Nessun tavolo occupato trovato"
                    : totalCount == 1
                        ? "Trovato 1 tavolo occupato"
                        : $"Trovati {totalCount} tavoli occupati";

                return new PaginatedResponseDTO<TavoloDTO>
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
                return new PaginatedResponseDTO<TavoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei tavoli occupati"
                };
            }
        }

        // ✅ METODO PAGINATO - MANTIENE FIRMA ORIGINALE
        public async Task<PaginatedResponseDTO<TavoloDTO>> GetByZonaAsync(string zona, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Validazione input
                if (string.IsNullOrWhiteSpace(zona))
                {
                    return new PaginatedResponseDTO<TavoloDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'zona' è obbligatorio"
                    };
                }

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(zona, maxLength: 50))
                {
                    return new PaginatedResponseDTO<TavoloDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'zona' non è valido"
                    };
                }

                // ✅ Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ RICERCA "INIZIA CON" usando StringHelper (usa l'input originale)
                var query = _context.Tavolo
                    .AsNoTracking()
                    .Where(t => t.Zona != null &&
                               StringHelper.StartsWithCaseInsensitive(t.Zona, zona))
                    .OrderBy(t => t.Numero);

                // ✅ Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                // ✅ Messaggio appropriato
                string message;
                if (totalCount == 0)
                {
                    message = $"Nessun tavolo trovato per zona che inizia con '{zona}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovato 1 tavolo per zona che inizia con '{zona}'";
                }
                else
                {
                    message = $"Trovati {totalCount} tavoli per zona che inizia con '{zona}'";
                }

                return new PaginatedResponseDTO<TavoloDTO>
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
                return new PaginatedResponseDTO<TavoloDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei tavoli per zona"
                };
            }
        }

        public async Task<SingleResponseDTO<TavoloDTO>> AddAsync(TavoloDTO tavoloDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tavoloDto);

                // ✅ Validazione input
                if (tavoloDto.Numero <= 0)
                    return SingleResponseDTO<TavoloDTO>.ErrorResponse("Il numero del tavolo deve essere maggiore di 0");

                string? zonaNormalizzata = null;
                if (!string.IsNullOrWhiteSpace(tavoloDto.Zona))
                {
                    // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                    if (!SecurityHelper.IsValidInput(tavoloDto.Zona, maxLength: 50))
                        return SingleResponseDTO<TavoloDTO>.ErrorResponse("Il campo 'Zona' contiene caratteri non validi");

                    // ✅ SOLO DOPO la validazione, normalizza
                    zonaNormalizzata = StringHelper.NormalizeSearchTerm(tavoloDto.Zona);
                }

                // ✅ Controllo duplicati (usa metodo interno)
                if (await NumeroExistsInternalAsync(tavoloDto.Numero, null))
                    return SingleResponseDTO<TavoloDTO>.ErrorResponse(
                        $"Esiste già un tavolo con numero {tavoloDto.Numero}");

                var tavolo = new Tavolo
                {
                    Numero = tavoloDto.Numero,
                    Zona = zonaNormalizzata,
                    Disponibile = tavoloDto.Disponibile
                };

                _context.Tavolo.Add(tavolo);
                await _context.SaveChangesAsync();

                tavoloDto.TavoloId = tavolo.TavoloId;

                return SingleResponseDTO<TavoloDTO>.SuccessResponse(
                    tavoloDto,
                    $"Tavolo {tavolo.Numero} creato con successo (ID: {tavolo.TavoloId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per tavoloDto: {@TavoloDto}", tavoloDto);
                return SingleResponseDTO<TavoloDTO>.ErrorResponse("Errore interno durante la creazione del tavolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(TavoloDTO tavoloDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tavoloDto);

                // ✅ Validazione input
                if (tavoloDto.Numero <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il numero del tavolo deve essere maggiore di 0");

                string? nuovaZonaNormalizzata = null;
                if (!string.IsNullOrWhiteSpace(tavoloDto.Zona))
                {
                    // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                    if (!SecurityHelper.IsValidInput(tavoloDto.Zona, maxLength: 50))
                        return SingleResponseDTO<bool>.ErrorResponse("Il campo 'Zona' contiene caratteri non validi");

                    // ✅ SOLO DOPO la validazione, normalizza
                    nuovaZonaNormalizzata = StringHelper.NormalizeSearchTerm(tavoloDto.Zona);
                }

                var tavolo = await _context.Tavolo
                    .FirstOrDefaultAsync(t => t.TavoloId == tavoloDto.TavoloId);

                if (tavolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Tavolo con ID {tavoloDto.TavoloId} non trovato");

                // ✅ Controllo duplicati ESCLUDENDO questo tavolo (usa metodo interno)
                if (await NumeroExistsInternalAsync(tavoloDto.Numero, tavoloDto.TavoloId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un altro tavolo con numero {tavoloDto.Numero}");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                if (tavolo.Numero != tavoloDto.Numero)
                {
                    tavolo.Numero = tavoloDto.Numero;
                    hasChanges = true;
                }

                if (tavolo.Zona != nuovaZonaNormalizzata)
                {
                    tavolo.Zona = nuovaZonaNormalizzata;
                    hasChanges = true;
                }

                if (tavolo.Disponibile != tavoloDto.Disponibile)
                {
                    tavolo.Disponibile = tavoloDto.Disponibile;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Tavolo con ID {tavoloDto.TavoloId} aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per il tavolo con ID {tavoloDto.TavoloId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per tavoloDto: {@TavoloDto}", tavoloDto);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento del tavolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID tavolo non valido");

                var tavolo = await _context.Tavolo.FindAsync(tavoloId);
                if (tavolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Tavolo con ID {tavoloId} non trovato");

                if (await HasDependenciesAsync(tavoloId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Impossibile eliminare il tavolo {tavolo.Numero} perché ci sono dipendenze attive");

                _context.Tavolo.Remove(tavolo);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Tavolo {tavolo.Numero} (ID: {tavoloId}) eliminato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per tavoloId: {TavoloId}", tavoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione del tavolo");
            }
        }

        private async Task<bool> HasDependenciesAsync(int tavoloId)
        {
            // ✅ Controlla le dipendenze REALI di Tavolo (dal modello)
            return await _context.Cliente.AnyAsync(c => c.TavoloId == tavoloId) ||
                   await _context.SessioniQr.AnyAsync(s => s.TavoloId == tavoloId);
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID tavolo non valido");

                var exists = await _context.Tavolo
                    .AsNoTracking()
                    .AnyAsync(t => t.TavoloId == tavoloId);

                string message = exists
                    ? $"Tavolo con ID {tavoloId} esiste"
                    : $"Tavolo con ID {tavoloId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per tavoloId: {TavoloId}", tavoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza del tavolo");
            }
        }

        public async Task<SingleResponseDTO<bool>> NumeroExistsAsync(int numero, int? excludeId = null)
        {
            try
            {
                if (numero <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il numero del tavolo deve essere maggiore di 0");

                var exists = await NumeroExistsInternalAsync(numero, excludeId);

                string message = exists
                    ? $"Tavolo con numero {numero} esiste"
                    : $"Tavolo con numero {numero} non trovato";

                // Aggiungi dettaglio se excludeId è specificato
                if (excludeId.HasValue)
                    message += $" (escludendo ID {excludeId.Value})";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in NumeroExistsAsync per numero: {Numero}, excludeId: {ExcludeId}",
                    numero, excludeId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza per numero");
            }
        }

        private async Task<bool> NumeroExistsInternalAsync(int numero, int? excludeId = null)
        {
            var query = _context.Tavolo
                .AsNoTracking()
                .Where(t => t.Numero == numero);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.TavoloId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID tavolo non valido");

                var tavolo = await _context.Tavolo.FindAsync(tavoloId);
                if (tavolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Tavolo con ID {tavoloId} non trovato");

                var nuovoStato = !tavolo.Disponibile;
                tavolo.Disponibile = nuovoStato;
                await _context.SaveChangesAsync();

                string stato = nuovoStato ? "disponibile" : "occupato";
                return SingleResponseDTO<bool>.SuccessResponse(
                    nuovoStato,
                    $"Tavolo {tavolo.Numero} (ID: {tavoloId}) impostato come {stato}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ToggleDisponibilitaAsync per tavoloId: {TavoloId}", tavoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante il cambio di disponibilità");
            }
        }

        public async Task<SingleResponseDTO<bool>> ToggleDisponibilitaByNumeroAsync(int numero)
        {
            try
            {
                if (numero <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il numero del tavolo deve essere maggiore di 0");

                var tavolo = await _context.Tavolo
                    .FirstOrDefaultAsync(t => t.Numero == numero);

                if (tavolo == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Tavolo con numero {numero} non trovato");

                var nuovoStato = !tavolo.Disponibile;
                tavolo.Disponibile = nuovoStato;
                await _context.SaveChangesAsync();

                string stato = nuovoStato ? "disponibile" : "occupato";
                return SingleResponseDTO<bool>.SuccessResponse(
                    nuovoStato,
                    $"Tavolo {tavolo.Numero} impostato come {stato}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ToggleDisponibilitaByNumeroAsync per numero: {Numero}", numero);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante il cambio di disponibilità");
            }
        }

        // Per statistiche/conteggi rapidi
        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var count = await _context.Tavolo.AsNoTracking().CountAsync();
                string message = count == 0
                    ? "Nessun tavolo presente"
                    : count == 1
                        ? "C'è 1 tavolo in totale"
                        : $"Ci sono {count} tavoli in totale";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei tavoli");
            }
        }

        public async Task<SingleResponseDTO<int>> CountDisponibiliAsync()
        {
            try
            {
                var count = await _context.Tavolo
                    .AsNoTracking()
                    .CountAsync(t => t.Disponibile);

                string message = count == 0
                    ? "Nessun tavolo disponibile"
                    : count == 1
                        ? "C'è 1 tavolo disponibile"
                        : $"Ci sono {count} tavoli disponibili";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei tavoli disponibili");
            }
        }

        public async Task<SingleResponseDTO<int>> CountOccupatiAsync()
        {
            try
            {
                var count = await _context.Tavolo
                    .AsNoTracking()
                    .CountAsync(t => !t.Disponibile);

                string message = count == 0
                    ? "Nessun tavolo occupato"
                    : count == 1
                        ? "C'è 1 tavolo occupato"
                        : $"Ci sono {count} tavoli occupati";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountOccupatiAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei tavoli occupati");
            }
        }
    }
}