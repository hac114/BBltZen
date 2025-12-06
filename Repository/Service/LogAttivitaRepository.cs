using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class LogAttivitaRepository : ILogAttivitaRepository
    {
        private readonly BubbleTeaContext _context;

        public LogAttivitaRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        // ------------------ MAPPINGS ------------------
        private static LogAttivitaDTO MapToDTO(LogAttivita logAttivita)
        {
            return new LogAttivitaDTO
            {
                LogId = logAttivita.LogId,
                TipoAttivita = logAttivita.TipoAttivita,
                Descrizione = logAttivita.Descrizione,
                DataEsecuzione = logAttivita.DataEsecuzione,
                Dettagli = logAttivita.Dettagli,
                UtenteId = logAttivita.UtenteId
            };
        }

        // ------------------ CRUD BASE ------------------
        public async Task<LogAttivitaDTO> AddAsync(LogAttivitaDTO logAttivitaDto)
        {
            if (logAttivitaDto == null)
                throw new ArgumentNullException(nameof(logAttivitaDto));

            if (!SecurityHelper.IsValidInput(logAttivitaDto.TipoAttivita, 50) ||
                !SecurityHelper.IsValidInput(logAttivitaDto.Descrizione, 500) ||
                (logAttivitaDto.Dettagli != null && !SecurityHelper.IsValidInput(logAttivitaDto.Dettagli, 2000)))
            {
                throw new ArgumentException("Input non valido o troppo lungo");
            }

            var logAttivita = new LogAttivita
            {
                TipoAttivita = logAttivitaDto.TipoAttivita.Trim(),
                Descrizione = logAttivitaDto.Descrizione.Trim(),
                DataEsecuzione = DateTime.Now,
                Dettagli = logAttivitaDto.Dettagli?.Trim(),
                UtenteId = logAttivitaDto.UtenteId
            };

            _context.LogAttivita.Add(logAttivita);
            await _context.SaveChangesAsync();

            logAttivitaDto.LogId = logAttivita.LogId;
            logAttivitaDto.DataEsecuzione = logAttivita.DataEsecuzione;

            return logAttivitaDto;
        }

        public async Task<bool> ExistsAsync(int logId)
        {
            return await _context.LogAttivita.AnyAsync(l => l.LogId == logId);
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByIdAsync(int? id = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (!id.HasValue)
            {
                var queryAll = _context.LogAttivita
                    .AsNoTracking()
                    .OrderByDescending(l => l.DataEsecuzione);

                var totalCount = await queryAll.CountAsync();

                // Mapping inline dopo il caricamento
                var items = await queryAll
                    .Skip((safePage - 1) * safePageSize)
                    .Take(safePageSize)
                    .ToListAsync();

                var dtoItems = items.Select(l => new LogAttivitaDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli,
                    UtenteId = l.UtenteId
                }).ToList();

                return new PaginatedResponseDTO<LogAttivitaDTO>
                {
                    Data = dtoItems,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato"
                };
            }

            if (id <= 0)
                return new PaginatedResponseDTO<LogAttivitaDTO> { Message = "ID non valido" };

            var log = await _context.LogAttivita
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LogId == id.Value);

            if (log == null)
                return new PaginatedResponseDTO<LogAttivitaDTO> { Message = $"Log attività con ID {id} non trovato" };

            return new PaginatedResponseDTO<LogAttivitaDTO>
            {
                Data = new List<LogAttivitaDTO>
                {
                    new LogAttivitaDTO
                    {
                        LogId = log.LogId,
                        TipoAttivita = log.TipoAttivita,
                        Descrizione = log.Descrizione,
                        DataEsecuzione = log.DataEsecuzione,
                        Dettagli = log.Dettagli,
                        UtenteId = log.UtenteId
                    }
                },
                Page = 1,
                PageSize = 1,
                TotalCount = 1,
                Message = $"Log attività {id} trovato"
            };
        }

        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoAttivitaAsync(string? tipoAttivita = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (tipoAttivita != null && !SecurityHelper.IsValidInput(tipoAttivita, 50))
                return new PaginatedResponseDTO<LogAttivitaDTO> { Message = "Tipo attività non valido" };

            var query = _context.LogAttivita.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipoAttivita))
            {
                var normalizedTipo = SecurityHelper.NormalizeSafe(tipoAttivita);
                query = query.Where(l => l.TipoAttivita != null && l.TipoAttivita.ToUpper().StartsWith(normalizedTipo));
            }

            query = query.OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();

            // Carico i dati e poi faccio il mapping in memoria
            var itemsFromDb = await query
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            var items = itemsFromDb.Select(l => new LogAttivitaDTO
            {
                LogId = l.LogId,
                TipoAttivita = l.TipoAttivita,
                Descrizione = l.Descrizione,
                DataEsecuzione = l.DataEsecuzione,
                Dettagli = l.Dettagli,
                UtenteId = l.UtenteId
            }).ToList();

            return new PaginatedResponseDTO<LogAttivitaDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = !string.IsNullOrWhiteSpace(tipoAttivita)
                    ? (totalCount > 0 ? $"Trovati {totalCount} log attività per tipo '{tipoAttivita}'"
                                       : $"Nessun log attività trovato per tipo '{tipoAttivita}'")
                    : (totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato")
            };
        }


        public async Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByUtenteIdAsync(int? utenteId = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            var query = _context.LogAttivita.AsNoTracking().AsQueryable();

            if (utenteId.HasValue && utenteId.Value > 0)
                query = query.Where(l => l.UtenteId == utenteId.Value);

            query = query.OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();

            // Carico i dati e faccio il mapping in memoria
            var itemsFromDb = await query
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            var items = itemsFromDb.Select(l => new LogAttivitaDTO
            {
                LogId = l.LogId,
                TipoAttivita = l.TipoAttivita,
                Descrizione = l.Descrizione,
                DataEsecuzione = l.DataEsecuzione,
                Dettagli = l.Dettagli,
                UtenteId = l.UtenteId
            }).ToList();

            return new PaginatedResponseDTO<LogAttivitaDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = utenteId.HasValue
                    ? (totalCount > 0 ? $"Trovati {totalCount} log attività per utente {utenteId}" : $"Nessun log attività trovato per utente {utenteId}")
                    : (totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato")
            };
        }


        public async Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            var query = _context.LogAttivita.AsQueryable();
            if (dataInizio.HasValue) query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);
            if (dataFine.HasValue) query = query.Where(l => l.DataEsecuzione <= dataFine.Value);
            return await query.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            var query = _context.LogAttivita.AsQueryable();
            if (dataInizio.HasValue) query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);
            if (dataFine.HasValue) query = query.Where(l => l.DataEsecuzione <= dataFine.Value);

            return await query
                .GroupBy(l => l.TipoAttivita)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task<int> CleanupOldLogsAsync(int giorniRitenzione = -1)
        {
            var query = _context.LogAttivita.AsQueryable();

            if (giorniRitenzione != -1)
            {
                var cutoffDate = DateTime.Now.AddDays(-giorniRitenzione);
                query = query.Where(l => l.DataEsecuzione < cutoffDate);
            }

            var oldLogs = await query.ToListAsync();
            int deletedCount = oldLogs.Count;

            if (deletedCount > 0)
            {
                _context.LogAttivita.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
            }

            var cleanupLog = new LogAttivita
            {
                TipoAttivita = "manutenzione",
                Descrizione = giorniRitenzione == -1
                    ? $"Puliti tutti i log attività: {deletedCount} record"
                    : $"Puliti {deletedCount} log vecchi di {giorniRitenzione} giorni",
                DataEsecuzione = DateTime.Now,
                Dettagli = giorniRitenzione == -1
                    ? "Cleanup completo di tutti i log"
                    : $"Cutoff date: {DateTime.Now.AddDays(-giorniRitenzione):yyyy-MM-dd}",
                UtenteId = null
            };

            _context.LogAttivita.Add(cleanupLog);
            await _context.SaveChangesAsync();

            return deletedCount;
        }

        // ------------------ METODI FRONTEND ------------------
        public async Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetAllPerFrontendAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            var query = _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((safePage - 1) * safePageSize)
                                   .Take(safePageSize)
                                   .Select(l => new LogAttivitaFrontendDTO
                                   {
                                       LogId = l.LogId,
                                       TipoAttivita = l.TipoAttivita,
                                       Descrizione = l.Descrizione,
                                       DataEsecuzione = l.DataEsecuzione,
                                       Dettagli = l.Dettagli,
                                       UtenteId = l.UtenteId,
                                       TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
                                   })
                                   .ToListAsync();

            return new PaginatedResponseDTO<LogAttivitaFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato"
            };
        }

        public async Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByPeriodoPerFrontendAsync(DateTime? dataInizio = null, DateTime? dataFine = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            var query = _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .AsQueryable();

            if (dataInizio.HasValue)
                query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);
            if (dataFine.HasValue)
                query = query.Where(l => l.DataEsecuzione <= dataFine.Value);

            query = query.OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((safePage - 1) * safePageSize)
                                   .Take(safePageSize)
                                   .Select(l => new LogAttivitaFrontendDTO
                                   {
                                       LogId = l.LogId,
                                       TipoAttivita = l.TipoAttivita,
                                       Descrizione = l.Descrizione,
                                       DataEsecuzione = l.DataEsecuzione,
                                       Dettagli = l.Dettagli,
                                       UtenteId = l.UtenteId,
                                       TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
                                   })
                                   .ToListAsync();

            string message = (dataInizio.HasValue || dataFine.HasValue) ? "Risultati filtrati per periodo" : "Tutti i risultati";

            return new PaginatedResponseDTO<LogAttivitaFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = message
            };
        }

        public async Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByTipoAttivitaPerFrontendAsync(string? tipoAttivita = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (tipoAttivita != null && !SecurityHelper.IsValidInput(tipoAttivita, 50))
                return new PaginatedResponseDTO<LogAttivitaFrontendDTO> { Message = "Tipo attività non valido" };

            var query = _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipoAttivita))
            {
                var normalizedTipo = SecurityHelper.NormalizeSafe(tipoAttivita);
                query = query.Where(l => l.TipoAttivita != null && l.TipoAttivita.ToUpper().StartsWith(normalizedTipo));
            }

            query = query.OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((safePage - 1) * safePageSize)
                                   .Take(safePageSize)
                                   .Select(l => new LogAttivitaFrontendDTO
                                   {
                                       LogId = l.LogId,
                                       TipoAttivita = l.TipoAttivita,
                                       Descrizione = l.Descrizione,
                                       DataEsecuzione = l.DataEsecuzione,
                                       Dettagli = l.Dettagli,
                                       UtenteId = l.UtenteId,
                                       TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
                                   })
                                   .ToListAsync();

            return new PaginatedResponseDTO<LogAttivitaFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = !string.IsNullOrWhiteSpace(tipoAttivita)
                    ? (totalCount > 0 ? $"Trovati {totalCount} log attività per tipo '{tipoAttivita}'"
                                       : $"Nessun log attività trovato per tipo '{tipoAttivita}'")
                    : (totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato")
            };
        }

        public async Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByTipoUtenteAsync(string? tipoUtente = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);

            if (tipoUtente != null && !SecurityHelper.IsValidInput(tipoUtente, 20))
                return new PaginatedResponseDTO<LogAttivitaFrontendDTO> { Message = "Tipo utente non valido" };

            var query = _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipoUtente))
            {
                var normalizedTipoUtente = SecurityHelper.NormalizeSafe(tipoUtente);
                query = query.Where(l => l.Utente != null &&
                                         l.Utente.TipoUtente != null &&
                                         l.Utente.TipoUtente.ToUpper().StartsWith(normalizedTipoUtente));
            }

            query = query.OrderByDescending(l => l.DataEsecuzione);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((safePage - 1) * safePageSize)
                                   .Take(safePageSize)
                                   .Select(l => new LogAttivitaFrontendDTO
                                   {
                                       LogId = l.LogId,
                                       TipoAttivita = l.TipoAttivita,
                                       Descrizione = l.Descrizione,
                                       DataEsecuzione = l.DataEsecuzione,
                                       Dettagli = l.Dettagli,
                                       UtenteId = l.UtenteId,
                                       TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
                                   })
                                   .ToListAsync();

            return new PaginatedResponseDTO<LogAttivitaFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = !string.IsNullOrWhiteSpace(tipoUtente)
                    ? (totalCount > 0 ? $"Trovati {totalCount} log attività per tipo utente '{tipoUtente}'"
                                       : $"Nessun log attività trovato per tipo utente '{tipoUtente}'")
                    : (totalCount > 0 ? $"Trovati {totalCount} log attività" : "Nessun log attività trovato")
            };
        }
    }
}
