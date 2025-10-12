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
    public class LogAccessiRepository : ILogAccessiRepository
    {
        private readonly BubbleTeaContext _context;

        public LogAccessiRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetAllAsync()
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<LogAccessiDTO?> GetByIdAsync(int logId)
        {
            var logAccessi = await _context.LogAccessi
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (logAccessi == null) return null;

            return new LogAccessiDTO
            {
                LogId = logAccessi.LogId,
                UtenteId = logAccessi.UtenteId,
                ClienteId = logAccessi.ClienteId,
                TipoAccesso = logAccessi.TipoAccesso,
                Esito = logAccessi.Esito,
                IpAddress = logAccessi.IpAddress,
                UserAgent = logAccessi.UserAgent,
                DataCreazione = logAccessi.DataCreazione,
                Dettagli = logAccessi.Dettagli
            };
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetByUtenteIdAsync(int utenteId)
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .Where(l => l.UtenteId == utenteId)
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .Where(l => l.ClienteId == clienteId)
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetByTipoAccessoAsync(string tipoAccesso)
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .Where(l => l.TipoAccesso == tipoAccesso)
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetByEsitoAsync(string esito)
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .Where(l => l.Esito == esito)
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<LogAccessiDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.LogAccessi
                .AsNoTracking()
                .Where(l => l.DataCreazione >= dataInizio && l.DataCreazione <= dataFine)
                .OrderByDescending(l => l.DataCreazione)
                .Select(l => new LogAccessiDTO
                {
                    LogId = l.LogId,
                    UtenteId = l.UtenteId,
                    ClienteId = l.ClienteId,
                    TipoAccesso = l.TipoAccesso,
                    Esito = l.Esito,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    DataCreazione = l.DataCreazione,
                    Dettagli = l.Dettagli
                })
                .ToListAsync();
        }

        public async Task AddAsync(LogAccessiDTO logAccessiDto)
        {
            var logAccessi = new LogAccessi
            {
                UtenteId = logAccessiDto.UtenteId,
                ClienteId = logAccessiDto.ClienteId,
                TipoAccesso = logAccessiDto.TipoAccesso,
                Esito = logAccessiDto.Esito,
                IpAddress = logAccessiDto.IpAddress,
                UserAgent = logAccessiDto.UserAgent,
                DataCreazione = DateTime.Now,
                Dettagli = logAccessiDto.Dettagli
            };

            _context.LogAccessi.Add(logAccessi);
            await _context.SaveChangesAsync();

            logAccessiDto.LogId = logAccessi.LogId;
            logAccessiDto.DataCreazione = logAccessi.DataCreazione;
        }

        public async Task UpdateAsync(LogAccessiDTO logAccessiDto)
        {
            var logAccessi = await _context.LogAccessi
                .FirstOrDefaultAsync(l => l.LogId == logAccessiDto.LogId);

            if (logAccessi == null)
                throw new ArgumentException($"Log accessi con ID {logAccessiDto.LogId} non trovato");

            logAccessi.UtenteId = logAccessiDto.UtenteId;
            logAccessi.ClienteId = logAccessiDto.ClienteId;
            logAccessi.TipoAccesso = logAccessiDto.TipoAccesso;
            logAccessi.Esito = logAccessiDto.Esito;
            logAccessi.IpAddress = logAccessiDto.IpAddress;
            logAccessi.UserAgent = logAccessiDto.UserAgent;
            logAccessi.Dettagli = logAccessiDto.Dettagli;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int logId)
        {
            var logAccessi = await _context.LogAccessi
                .FirstOrDefaultAsync(l => l.LogId == logId);

            if (logAccessi != null)
            {
                _context.LogAccessi.Remove(logAccessi);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int logId)
        {
            return await _context.LogAccessi
                .AnyAsync(l => l.LogId == logId);
        }

        public async Task<int> GetNumeroAccessiAsync(DateTime? dataInizio = null, DateTime? dataFine = null)
        {
            var query = _context.LogAccessi.AsQueryable();

            if (dataInizio.HasValue)
            {
                query = query.Where(l => l.DataCreazione >= dataInizio.Value);
            }

            if (dataFine.HasValue)
            {
                query = query.Where(l => l.DataCreazione <= dataFine.Value);
            }

            return await query.CountAsync();
        }
    }
}