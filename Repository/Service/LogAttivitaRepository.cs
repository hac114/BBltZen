using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
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

        private LogAttivitaDTO MapToDTO(LogAttivita logAttivita)
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

        public async Task<IEnumerable<LogAttivitaDTO>> GetAllAsync()
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
            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.TipoAttivita == tipoAttivita)
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => MapToDTO(l))
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.DataEsecuzione >= dataInizio && l.DataEsecuzione <= dataFine)
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => MapToDTO(l))
                .ToListAsync();
        }

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

        public async Task UpdateAsync(LogAttivitaDTO logAttivitaDto)
        {
            var logAttivita = await _context.LogAttivita
                .FirstOrDefaultAsync(l => l.LogId == logAttivitaDto.LogId);

            if (logAttivita == null)
                return; // ✅ SILENT FAIL - Non lanciare eccezione

            // ✅ AGGIORNA SOLO SE ESISTE
            logAttivita.TipoAttivita = logAttivitaDto.TipoAttivita;
            logAttivita.Descrizione = logAttivitaDto.Descrizione;
            logAttivita.DataEsecuzione = logAttivitaDto.DataEsecuzione;
            logAttivita.Dettagli = logAttivitaDto.Dettagli;
            logAttivita.UtenteId = logAttivitaDto.UtenteId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int logId)
        {
            var logAttivita = await _context.LogAttivita
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (logAttivita != null)
            {
                _context.LogAttivita.Remove(logAttivita);
                await _context.SaveChangesAsync();
            }
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

        public async Task<IEnumerable<LogAttivitaDTO>> GetByUtenteIdAsync(int utenteId)
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.UtenteId == utenteId)
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
    }
}