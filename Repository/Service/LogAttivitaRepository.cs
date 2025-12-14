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
    public class LogAttivitaRepository(BubbleTeaContext context, ILogger<LogAttivitaRepository> logger) : ILogAttivitaRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<LogAttivitaRepository> _logger = logger;

        private static LogAttivitaDTO MapToDTO(LogAttivita logAttivita)
        {
            var dto = new LogAttivitaDTO
            {
                LogId = logAttivita.LogId,
                TipoAttivita = SafeString(logAttivita.TipoAttivita, 50) ?? string.Empty,
                Descrizione = SafeString(logAttivita.Descrizione, 500) ?? string.Empty,
                DataEsecuzione = logAttivita.DataEsecuzione,
                Dettagli = SafeString(logAttivita.Dettagli, 2000, allowNull: true),
                UtenteId = logAttivita.UtenteId,
                TipoUtente = SafeString(logAttivita.Utente?.TipoUtente, 20, allowNull: true),
                ClienteId = logAttivita.Utente?.ClienteId,
                UtenteEmail = SafeString(logAttivita.Utente?.Email, 100, allowNull: true),
                UtenteNome = GetUtenteNomeSicuro(logAttivita.Utente)
            };

            return dto;
        }

        // ✅ METODO HELPER PER STRINGHE SICURE
        // Opzionale: per coerenza assoluta
        private static string? SafeString(string? input, int maxLength, bool allowNull = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return allowNull ? null : string.Empty;

            // Validazione PRIMA (anche se dal DB)
            if (!SecurityHelper.IsValidInput(input, maxLength))
                return string.Empty;

            // Poi normalizzazione
            return StringHelper.NormalizeSearchTerm(input);
        }

        // ✅ METODO HELPER PER NOME UTENTE SICURO
        private static string? GetUtenteNomeSicuro(Utenti? utente)
        {
            if (utente == null)
                return null;

            var nome = StringHelper.NormalizeSearchTerm(utente.Nome ?? string.Empty);
            var cognome = StringHelper.NormalizeSearchTerm(utente.Cognome ?? string.Empty);
            var email = StringHelper.NormalizeSearchTerm(utente.Email ?? string.Empty);

            string? nomeUtente = null;

            if (!string.IsNullOrEmpty(nome) || !string.IsNullOrEmpty(cognome))
            {
                nomeUtente = $"{nome} {cognome}".Trim();
            }
            else if (!string.IsNullOrEmpty(email))
            {
                var atIndex = email.IndexOf('@');
                nomeUtente = atIndex > 0
                    ? email.Substring(0, Math.Min(atIndex, 50))
                    : email;
            }

            return !string.IsNullOrEmpty(nomeUtente) && SecurityHelper.IsValidInput(nomeUtente, 100)
                ? nomeUtente
                : null;
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ ORDINAMENTO PER DATA (più logico per i log)
                var query = _context.LogAttivita
                    .AsNoTracking()
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(l => MapToDTO(l))
                    .ToListAsync();

                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna attività di log trovata"
                        : $"Trovate {totalCount} attività di log"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync per LogAttivita");
                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle attività di log"
                };
            }
        }
                
        public async Task<SingleResponseDTO<LogAttivitaDTO>> GetByIdAsync(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse("ID non valido");

                var log = await _context.LogAttivita
                    .AsNoTracking()
                    .Include(l => l.Utente)
                    .FirstOrDefaultAsync(l => l.LogId == logId);

                if (log == null)
                    return SingleResponseDTO<LogAttivitaDTO>.NotFoundResponse(
                        $"Log attività con ID {logId} non trovato");

                return SingleResponseDTO<LogAttivitaDTO>.SuccessResponse(
                    MapToDTO(log),
                    $"Log attività con ID {logId} trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per LogId: {LogId}", logId);
                return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse(
                    "Errore interno nel recupero del log");
            }
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita, int page = 1, int pageSize = 10)
        {
            try
            {
                var searchTerm = StringHelper.NormalizeSearchTerm(tipoAttivita);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<LogAttivitaDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo di attività' è obbligatorio"
                    };
                }

                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 50))
                {
                    return new PaginatedResponseDTO<LogAttivitaDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo di attività' contiene caratteri non validi"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.LogAttivita
                    .AsNoTracking()
                    .Include(l => l.Utente) // ✅ Include PER MapToDTO
                    .Where(l => l.TipoAttivita != null &&
                               StringHelper.StartsWithCaseInsensitive(l.TipoAttivita, searchTerm))
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(l => MapToDTO(l))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna attività di log trovata con tipo che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 attività di log con tipo che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} attività di log con tipo che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<LogAttivitaDTO>
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
                _logger.LogError(ex, "Errore in GetByTipoAttivitaAsync per tipo: {Tipo}", tipoAttivita);
                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle attività filtrate per tipo"
                };
            }
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByUtenteIdAsync(int utenteId, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ VALIDAZIONE INPUT
                if (utenteId <= 0)
                {
                    return new PaginatedResponseDTO<LogAttivitaDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "ID utente non valido"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.LogAttivita
                    .AsNoTracking()
                    .Include(l => l.Utente)
                    .Where(l => l.UtenteId == utenteId)
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(l => MapToDTO(l))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna attività di log trovata per utente ID {utenteId}";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 attività di log per utente ID {utenteId}";
                }
                else
                {
                    message = $"Trovate {totalCount} attività di log per utente ID {utenteId}";
                }

                return new PaginatedResponseDTO<LogAttivitaDTO>
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
                _logger.LogError(ex, "Errore in GetByUtenteIdAsync per utenteId: {UtenteId}", utenteId);
                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle attività di log"
                };
            }
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoUtenteAsync(string tipoUtente, int page = 1, int pageSize = 10)
        {
            try
            {
                var searchTerm = StringHelper.NormalizeSearchTerm(tipoUtente);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<LogAttivitaDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo di utente' è obbligatorio"
                    };
                }

                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 20))
                {
                    return new PaginatedResponseDTO<LogAttivitaDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'tipo di utente' contiene caratteri non validi"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.LogAttivita
                    .AsNoTracking()
                    .Include(l => l.Utente)
                    .Where(l => l.Utente != null &&
                               !string.IsNullOrEmpty(l.Utente.TipoUtente) &&
                               StringHelper.StartsWithCaseInsensitive(l.Utente.TipoUtente, searchTerm))
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(l => MapToDTO(l))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna attività di log trovata per tipo utente che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 attività di log per tipo utente che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} attività di log per tipo utente che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<LogAttivitaDTO>
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
                _logger.LogError(ex, "Errore in GetByTipoUtenteAsync per tipoUtente: {TipoUtente}", tipoUtente);
                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle attività filtrate per tipo di utente"
                };
            }
        }

        public async Task<SingleResponseDTO<int>> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            try
            {
                var query = _context.LogAttivita.AsNoTracking().AsQueryable();

                if (dataInizio.HasValue)
                    query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);

                if (dataFine.HasValue)
                    query = query.Where(l => l.DataEsecuzione <= dataFine.Value);

                var count = await query.CountAsync();

                string message = count == 0
                    ? "Nessuna attività di log trovata" + (dataInizio.HasValue || dataFine.HasValue ? " nel periodo specificato" : "")
                    : $"Trovate {count} attività di log" + (dataInizio.HasValue || dataFine.HasValue ? " nel periodo specificato" : "");

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetNumeroAttivitaAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel recupero del numero di attività");
            }
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByDateRangeAsync(DateTime dataInizio, DateTime dataFine, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validazione date
                if (dataInizio > dataFine)
                    throw new ArgumentException("Data inizio deve essere precedente a data fine");

                if (dataFine > DateTime.UtcNow.AddDays(1)) // Permetti oggi
                    throw new ArgumentException("Data fine non può essere nel futuro");

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.LogAttivita
                    .AsNoTracking()
                    .Include(l => l.Utente)
                    .Where(l => l.DataEsecuzione >= dataInizio && l.DataEsecuzione <= dataFine)
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(l => MapToDTO(l))
                    .ToListAsync();

                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? $"Nessuna attività trovata nel periodo {dataInizio:dd/MM/yyyy} - {dataFine:dd/MM/yyyy}"
                        : $"Trovate {totalCount} attività nel periodo specificato"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByDateRangeAsync");
                throw;
            }
        }

        public async Task<SingleResponseDTO<Dictionary<string, int>>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            try
            {
                var query = _context.LogAttivita.AsNoTracking().AsQueryable();

                if (dataInizio.HasValue)
                    query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);

                if (dataFine.HasValue)
                    query = query.Where(l => l.DataEsecuzione <= dataFine.Value);

                var stats = await query
                    .GroupBy(l => l.TipoAttivita)
                    .Select(g => new { Tipo = g.Key ?? "Sconosciuto", Count = g.Count() })
                    .ToDictionaryAsync(x => x.Tipo, x => x.Count);

                string message = stats.Count == 0
                    ? "Nessuna statistica disponibile" + (dataInizio.HasValue || dataFine.HasValue ? " nel periodo specificato" : "")
                    : $"Statistiche calcolate per {stats.Count} tipi di attività" + (dataInizio.HasValue || dataFine.HasValue ? " nel periodo specificato" : "");

                return SingleResponseDTO<Dictionary<string, int>>.SuccessResponse(stats, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetStatisticheAttivitaAsync");
                return SingleResponseDTO<Dictionary<string, int>>.ErrorResponse("Errore nel calcolo delle statistiche");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int logId)
        {
            try
            {
                if (logId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.LogAttivita
                    .AsNoTracking()
                    .AnyAsync(l => l.LogId == logId);

                string message = exists
                    ? $"Log attività con ID {logId} esiste"
                    : $"Log attività con ID {logId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per LogId: {LogId}", logId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza");
            }
        }

        public async Task<SingleResponseDTO<LogAttivitaDTO>> AddAsync(LogAttivitaDTO logAttivitaDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(logAttivitaDto);

                // ✅ 1. VALIDAZIONE CAMPI OBBLIGATORI
                if (string.IsNullOrWhiteSpace(logAttivitaDto.TipoAttivita))
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse("Il campo 'TipoAttivita' è obbligatorio");

                if (string.IsNullOrWhiteSpace(logAttivitaDto.Descrizione))
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse("Il campo 'Descrizione' è obbligatorio");

                // ✅ 2. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE
                if (!SecurityHelper.IsValidInput(logAttivitaDto.TipoAttivita, 50))
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse(
                        "Il campo 'TipoAttivita' non è valido o contiene caratteri pericolosi");

                if (!SecurityHelper.IsValidInput(logAttivitaDto.Descrizione, 500))
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse(
                        "Il campo 'Descrizione' non è valido o contiene caratteri pericolosi");

                if (!string.IsNullOrWhiteSpace(logAttivitaDto.Dettagli) &&
                    !SecurityHelper.IsValidInput(logAttivitaDto.Dettagli, 2000))
                    return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse(
                        "Il campo 'Dettagli' non è valido o contiene caratteri pericolosi");

                // ✅ 3. SOLO DOPO la validazione, normalizza
                var tipoAttivita = StringHelper.NormalizeSearchTerm(logAttivitaDto.TipoAttivita);
                var descrizione = StringHelper.NormalizeSearchTerm(logAttivitaDto.Descrizione);
                var dettagli = !string.IsNullOrWhiteSpace(logAttivitaDto.Dettagli)
                    ? StringHelper.NormalizeSearchTerm(logAttivitaDto.Dettagli)
                    : null;

                // ✅ 4. VERIFICA UTENTE (se specificato) - QUESTA È MANCANTE!
                if (logAttivitaDto.UtenteId.HasValue && logAttivitaDto.UtenteId.Value > 0)
                {
                    var utenteEsiste = await _context.Utenti
                        .AnyAsync(u => u.UtenteId == logAttivitaDto.UtenteId.Value);

                    if (!utenteEsiste)
                        return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse(
                            $"Utente con ID {logAttivitaDto.UtenteId} non trovato");
                }

                // ✅ 5. CREAZIONE ENTITÀ
                var logAttivita = new LogAttivita
                {
                    TipoAttivita = tipoAttivita,
                    Descrizione = descrizione,
                    DataEsecuzione = DateTime.UtcNow,
                    Dettagli = dettagli,
                    UtenteId = logAttivitaDto.UtenteId
                };

                // ✅ 6. SALVATAGGIO
                await _context.LogAttivita.AddAsync(logAttivita);
                await _context.SaveChangesAsync();

                // ✅ 7. AGGIORNA DTO
                logAttivitaDto.LogId = logAttivita.LogId;
                logAttivitaDto.DataEsecuzione = logAttivita.DataEsecuzione;

                _logger.LogInformation("Log attività aggiunto: {LogId} - {TipoAttivita}", logAttivita.LogId, tipoAttivita);

                return SingleResponseDTO<LogAttivitaDTO>.SuccessResponse(
                    logAttivitaDto,
                    $"Log attività '{tipoAttivita}' creato con successo (ID: {logAttivita.LogId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per logAttivitaDto: {@LogAttivitaDto}", logAttivitaDto);
                return SingleResponseDTO<LogAttivitaDTO>.ErrorResponse("Errore interno durante la creazione del log attività");
            }
        }

        public async Task<SingleResponseDTO<int>> CleanupOldLogsAsync(int giorniRitenzione = 90)
        {
            try
            {
                // ✅ SICUREZZA: Impedisci cancellazione accidentale
                if (giorniRitenzione <= 0)
                    return SingleResponseDTO<int>.ErrorResponse("Il numero di giorni di ritenzione deve essere positivo");

                var cutoffDate = DateTime.UtcNow.AddDays(-giorniRitenzione);

                var query = _context.LogAttivita
                    .Where(l => l.DataEsecuzione < cutoffDate);

                var oldLogs = await query.ToListAsync();
                int deletedCount = oldLogs.Count;

                if (deletedCount > 0)
                {
                    _context.LogAttivita.RemoveRange(oldLogs);
                    await _context.SaveChangesAsync();
                }

                // ✅ LOG DELL'AZIONE (opzionale ma consigliato)
                var cleanupLog = new LogAttivita
                {
                    TipoAttivita = "manutenzione",
                    Descrizione = $"Puliti {deletedCount} log attività vecchi di {giorniRitenzione} giorni",
                    DataEsecuzione = DateTime.UtcNow,
                    Dettagli = $"Data limite: {cutoffDate:yyyy-MM-dd}",
                    UtenteId = null
                };

                await _context.LogAttivita.AddAsync(cleanupLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleanup log completato: {DeletedCount} record eliminati", deletedCount);

                string message = deletedCount == 0
                    ? $"Nessun log eliminato (nessun log più vecchio di {giorniRitenzione} giorni)"
                    : deletedCount == 1
                        ? $"Eliminato 1 log attività più vecchio di {giorniRitenzione} giorni"
                        : $"Eliminati {deletedCount} log attività più vecchi di {giorniRitenzione} giorni";

                return SingleResponseDTO<int>.SuccessResponse(deletedCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CleanupOldLogsAsync per giorniRitenzione: {Giorni}", giorniRitenzione);
                return SingleResponseDTO<int>.ErrorResponse("Errore interno durante la pulizia dei log vecchi");
            }
        }
    }
}