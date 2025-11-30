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

        private async Task<IEnumerable<LogAttivitaDTO>> GetAllAsync()
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => MapToDTO(l))
                .ToListAsync();
        }

        public async Task<LogAttivitaDTO?> GetByIdAsync(int logId)
        {
            var logAttivita = await _context.LogAttivita
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LogId == logId);

            return logAttivita == null ? null : MapToDTO(logAttivita);
        }

        public async Task<IEnumerable<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita)
        {
            if (string.IsNullOrWhiteSpace(tipoAttivita))
                return await GetAllAsync();

            var normalizedSearch = StringHelper.NormalizeSearchTerm(tipoAttivita);

            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.TipoAttivita.Contains(normalizedSearch, StringComparison.CurrentCultureIgnoreCase))
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => MapToDTO(l))
                .ToListAsync();
        }

        //public async Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        //{
        //    return await _context.LogAttivita
        //        .AsNoTracking()
        //        .Where(l => l.DataEsecuzione >= dataInizio && l.DataEsecuzione <= dataFine)
        //        .OrderByDescending(l => l.DataEsecuzione)
        //        .Select(l => MapToDTO(l))
        //        .ToListAsync();
        //}

        public async Task<LogAttivitaDTO> AddAsync(LogAttivitaDTO logAttivitaDto)
        {
            if (logAttivitaDto == null)
                throw new ArgumentNullException(nameof(logAttivitaDto));

            var logAttivita = new LogAttivita
            {
                TipoAttivita = logAttivitaDto.TipoAttivita,
                Descrizione = logAttivitaDto.Descrizione,
                DataEsecuzione = DateTime.Now, // ✅ SEMPRE DateTime.Now
                Dettagli = logAttivitaDto.Dettagli,
                UtenteId = logAttivitaDto.UtenteId
            };

            _context.LogAttivita.Add(logAttivita);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO
            logAttivitaDto.LogId = logAttivita.LogId;
            logAttivitaDto.DataEsecuzione = logAttivita.DataEsecuzione;

            return logAttivitaDto; // ✅ IMPORTANTE: ritorna DTO
        }        

        public async Task<bool> ExistsAsync(int logId)
        {
            return await _context.LogAttivita
                .AnyAsync(l => l.LogId == logId);
        }

        public async Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            var query = _context.LogAttivita.AsQueryable();

            if (dataInizio.HasValue)
            {
                query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);
            }

            if (dataFine.HasValue)
            {
                query = query.Where(l => l.DataEsecuzione <= dataFine.Value);
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<LogAttivitaDTO>> GetByUtenteIdAsync(int? utenteId = null)
        {
            var query = _context.LogAttivita.AsQueryable();

            if (utenteId.HasValue)
            {
                query = query.Where(l => l.UtenteId == utenteId.Value);
            }

            return await query
                .AsNoTracking()
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => MapToDTO(l))
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            var query = _context.LogAttivita.AsQueryable();

            if (dataInizio.HasValue)
                query = query.Where(l => l.DataEsecuzione >= dataInizio.Value);

            if (dataFine.HasValue)
                query = query.Where(l => l.DataEsecuzione <= dataFine.Value);

            var statistiche = await query
                .GroupBy(l => l.TipoAttivita)
                .Select(g => new { TipoAttivita = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TipoAttivita, x => x.Count);

            return statistiche;
        }

        // ✅ METODO CLEANUP CORRETTO
        public async Task<int> CleanupOldLogsAsync(int giorniRitenzione = -1)
        {
            var query = _context.LogAttivita.AsQueryable();

            // ✅ SE giorniRitenzione = -1, ELIMINA TUTTO
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

            // ✅ LOG DELLA PULIZIA (USA 'giorniRitenzione' invece di 'retention')
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

        // ✅ METODI FRONTEND AGGIORNATI CON JOIN E TIPO_UTENTE

        public async Task<IEnumerable<LogAttivitaFrontendDTO>> GetAllPerFrontendAsync()
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente) // ✅ JOIN CON UTENTI
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => new LogAttivitaFrontendDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli,
                    UtenteId = l.UtenteId,
                    TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null // ✅ TIPO UTENTE
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAttivitaFrontendDTO>> GetByPeriodoPerFrontendAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente) // ✅ JOIN CON UTENTI
                .Where(l => l.DataEsecuzione >= dataInizio && l.DataEsecuzione <= dataFine)
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => new LogAttivitaFrontendDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli,
                    UtenteId = l.UtenteId,
                    TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null // ✅ TIPO UTENTE
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAttivitaFrontendDTO>> GetByTipoAttivitaPerFrontendAsync(string tipoAttivita)
        {
            if (string.IsNullOrWhiteSpace(tipoAttivita))
                return await GetAllPerFrontendAsync();

            var normalizedSearch = StringHelper.NormalizeSearchTerm(tipoAttivita);

            return await _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .Where(l => l.TipoAttivita.Contains(tipoAttivita, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(l => l.DataEsecuzione)
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
        }

        // ✅ NUOVI METODI INTELLIGENTI NEL REPOSITORY
        //public async Task<IEnumerable<LogAttivitaFrontendDTO>> SearchIntelligenteAsync(string searchTerm)
        //{
        //    if (string.IsNullOrWhiteSpace(searchTerm))
        //        return await GetAllPerFrontendAsync();

        //    searchTerm = searchTerm.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);

        //    return await _context.LogAttivita
        //        .AsNoTracking()
        //        .Include(l => l.Utente)
        //        .Where(l =>
        //            l.TipoAttivita.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
        //            l.Descrizione.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
        //            (l.Dettagli != null && l.Dettagli.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase)) ||
        //            (l.Utente != null && l.Utente.TipoUtente.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
        //        )
        //        .OrderByDescending(l => l.DataEsecuzione)
        //        .Select(l => new LogAttivitaFrontendDTO
        //        {
        //            LogId = l.LogId,
        //            TipoAttivita = l.TipoAttivita,
        //            Descrizione = l.Descrizione,
        //            DataEsecuzione = l.DataEsecuzione,
        //            Dettagli = l.Dettagli,
        //            UtenteId = l.UtenteId,
        //            TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
        //        })
        //        .ToListAsync();
        //}

        //public async Task<IEnumerable<LogAttivitaFrontendDTO>> GetByTipoAttivitaIntelligenteAsync(string tipoAttivita)
        //{
        //    if (string.IsNullOrWhiteSpace(tipoAttivita))
        //        return await GetAllPerFrontendAsync();

        //    return await _context.LogAttivita
        //        .AsNoTracking()
        //        .Include(l => l.Utente)
        //        .Where(l => l.TipoAttivita.Contains(tipoAttivita, StringComparison.InvariantCultureIgnoreCase))
        //        .OrderByDescending(l => l.DataEsecuzione)
        //        .Select(l => new LogAttivitaFrontendDTO
        //        {
        //            LogId = l.LogId,
        //            TipoAttivita = l.TipoAttivita,
        //            Descrizione = l.Descrizione,
        //            DataEsecuzione = l.DataEsecuzione,
        //            Dettagli = l.Dettagli,
        //            UtenteId = l.UtenteId,
        //            TipoUtente = l.Utente != null ? l.Utente.TipoUtente : null
        //        })
        //        .ToListAsync();
        //}

        // ✅ NUOVO METODO: FILTRO PER TIPO_UTENTE
        public async Task<IEnumerable<LogAttivitaFrontendDTO>> GetByTipoUtenteAsync(string tipoUtente)
        {
            var query = _context.LogAttivita
                .AsNoTracking()
                .Include(l => l.Utente)
                .AsQueryable();

            // ✅ FILTRA SOLO SE SPECIFICATO (string, non nullable)
            if (!string.IsNullOrWhiteSpace(tipoUtente))
            {
                query = query.Where(l => l.Utente != null &&
                                       l.Utente.TipoUtente.Contains(tipoUtente, StringComparison.InvariantCultureIgnoreCase));
            }

            return await query
                .OrderByDescending(l => l.DataEsecuzione)
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
        }
    }
}
