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

        public async Task<IEnumerable<LogAttivitaDTO>> GetAllAsync()
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => new LogAttivitaDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<LogAttivitaDTO?> GetByIdAsync(int logId)
        {
            var logAttivita = await _context.LogAttivita
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (logAttivita == null) return null;

            return new LogAttivitaDTO
            {
                LogId = logAttivita.LogId,
                TipoAttivita = logAttivita.TipoAttivita,
                Descrizione = logAttivita.Descrizione,
                DataEsecuzione = logAttivita.DataEsecuzione,
                Dettagli = logAttivita.Dettagli
            };
        }

        public async Task<IEnumerable<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita)
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.TipoAttivita == tipoAttivita)
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => new LogAttivitaDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.LogAttivita
                .AsNoTracking()
                .Where(l => l.DataEsecuzione >= dataInizio && l.DataEsecuzione <= dataFine)
                .OrderByDescending(l => l.DataEsecuzione)
                .Select(l => new LogAttivitaDTO
                {
                    LogId = l.LogId,
                    TipoAttivita = l.TipoAttivita,
                    Descrizione = l.Descrizione,
                    DataEsecuzione = l.DataEsecuzione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task AddAsync(LogAttivitaDTO logAttivitaDto)
        {
            var logAttivita = new LogAttivita
            {
                TipoAttivita = logAttivitaDto.TipoAttivita,
                Descrizione = logAttivitaDto.Descrizione,
                DataEsecuzione = DateTime.Now,
                Dettagli = logAttivitaDto.Dettagli
            };

            _context.LogAttivita.Add(logAttivita);
            await _context.SaveChangesAsync();

            logAttivitaDto.LogId = logAttivita.LogId;
            logAttivitaDto.DataEsecuzione = logAttivita.DataEsecuzione;
        }

        public async Task UpdateAsync(LogAttivitaDTO logAttivitaDto)
        {
            var logAttivita = await _context.LogAttivita
                .FirstOrDefaultAsync(l => l.LogId == logAttivitaDto.LogId);

            if (logAttivita == null)
                throw new ArgumentException($"Log attività con ID {logAttivitaDto.LogId} non trovato");

            logAttivita.TipoAttivita = logAttivitaDto.TipoAttivita;
            logAttivita.Descrizione = logAttivitaDto.Descrizione;
            logAttivita.DataEsecuzione = logAttivitaDto.DataEsecuzione;
            logAttivita.Dettagli = logAttivitaDto.Dettagli;

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
    }
}