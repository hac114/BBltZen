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
    public class ConfigSoglieTempiRepository : IConfigSoglieTempiRepository
    {
        private readonly BubbleTeaContext _context;

        public ConfigSoglieTempiRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConfigSoglieTempiDTO>> GetAllAsync()
        {
            return await _context.ConfigSoglieTempi
                .AsNoTracking()
                .Select(c => new ConfigSoglieTempiDTO
                {
                    SogliaId = c.SogliaId,
                    StatoOrdineId = c.StatoOrdineId,
                    SogliaAttenzione = c.SogliaAttenzione,
                    SogliaCritico = c.SogliaCritico,
                    DataAggiornamento = c.DataAggiornamento,
                    UtenteAggiornamento = c.UtenteAggiornamento
                })
                .ToListAsync();
        }

        public async Task<ConfigSoglieTempiDTO?> GetByIdAsync(int sogliaId)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SogliaId == sogliaId);

            if (configSoglieTempi == null) return null;

            return new ConfigSoglieTempiDTO
            {
                SogliaId = configSoglieTempi.SogliaId,
                StatoOrdineId = configSoglieTempi.StatoOrdineId,
                SogliaAttenzione = configSoglieTempi.SogliaAttenzione,
                SogliaCritico = configSoglieTempi.SogliaCritico,
                DataAggiornamento = configSoglieTempi.DataAggiornamento,
                UtenteAggiornamento = configSoglieTempi.UtenteAggiornamento
            };
        }

        public async Task<ConfigSoglieTempiDTO?> GetByStatoOrdineIdAsync(int statoOrdineId)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.StatoOrdineId == statoOrdineId);

            if (configSoglieTempi == null) return null;

            return new ConfigSoglieTempiDTO
            {
                SogliaId = configSoglieTempi.SogliaId,
                StatoOrdineId = configSoglieTempi.StatoOrdineId,
                SogliaAttenzione = configSoglieTempi.SogliaAttenzione,
                SogliaCritico = configSoglieTempi.SogliaCritico,
                DataAggiornamento = configSoglieTempi.DataAggiornamento,
                UtenteAggiornamento = configSoglieTempi.UtenteAggiornamento
            };
        }

        public async Task AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            var configSoglieTempi = new ConfigSoglieTempi
            {
                StatoOrdineId = configSoglieTempiDto.StatoOrdineId,
                SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione,
                SogliaCritico = configSoglieTempiDto.SogliaCritico,
                DataAggiornamento = DateTime.Now,
                UtenteAggiornamento = configSoglieTempiDto.UtenteAggiornamento
            };

            _context.ConfigSoglieTempi.Add(configSoglieTempi);
            await _context.SaveChangesAsync();

            configSoglieTempiDto.SogliaId = configSoglieTempi.SogliaId;
            configSoglieTempiDto.DataAggiornamento = configSoglieTempi.DataAggiornamento;
        }

        public async Task UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .FirstOrDefaultAsync(c => c.SogliaId == configSoglieTempiDto.SogliaId);

            if (configSoglieTempi == null)
                throw new ArgumentException($"Configurazione soglie tempi con ID {configSoglieTempiDto.SogliaId} non trovata");

            configSoglieTempi.StatoOrdineId = configSoglieTempiDto.StatoOrdineId;
            configSoglieTempi.SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione;
            configSoglieTempi.SogliaCritico = configSoglieTempiDto.SogliaCritico;
            configSoglieTempi.DataAggiornamento = DateTime.Now;
            configSoglieTempi.UtenteAggiornamento = configSoglieTempiDto.UtenteAggiornamento;

            await _context.SaveChangesAsync();

            configSoglieTempiDto.DataAggiornamento = configSoglieTempi.DataAggiornamento;
        }

        public async Task DeleteAsync(int sogliaId)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .FirstOrDefaultAsync(c => c.SogliaId == sogliaId);

            if (configSoglieTempi != null)
            {
                _context.ConfigSoglieTempi.Remove(configSoglieTempi);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int sogliaId)
        {
            return await _context.ConfigSoglieTempi
                .AnyAsync(c => c.SogliaId == sogliaId);
        }

        public async Task<bool> ExistsByStatoOrdineIdAsync(int statoOrdineId, int? excludeSogliaId = null)
        {
            var query = _context.ConfigSoglieTempi.Where(c => c.StatoOrdineId == statoOrdineId);

            if (excludeSogliaId.HasValue)
            {
                query = query.Where(c => c.SogliaId != excludeSogliaId.Value);
            }

            return await query.AnyAsync();
        }
    }
}