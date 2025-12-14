using BBltZen;
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

        private ConfigSoglieTempiDTO MapToDTO(ConfigSoglieTempi configSoglieTempi)
        {
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

        public async Task<IEnumerable<ConfigSoglieTempiDTO>> GetAllAsync()
        {
            return await _context.ConfigSoglieTempi
                .AsNoTracking()
                .OrderBy(c => c.StatoOrdineId)
                .Select(c => MapToDTO(c))
                .ToListAsync();
        }

        public async Task<ConfigSoglieTempiDTO?> GetByIdAsync(int sogliaId)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SogliaId == sogliaId);

            return configSoglieTempi == null ? null : MapToDTO(configSoglieTempi);
        }

        public async Task<ConfigSoglieTempiDTO?> GetByStatoOrdineIdAsync(int statoOrdineId)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.StatoOrdineId == statoOrdineId);

            return configSoglieTempi == null ? null : MapToDTO(configSoglieTempi);
        }

        public async Task<ConfigSoglieTempiDTO> AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            if (configSoglieTempiDto == null)
                throw new ArgumentNullException(nameof(configSoglieTempiDto));

            // ✅ VALIDAZIONE BUSINESS LOGIC CON NUOVO METODO
            var validation = await ValidateConfigSoglieAsync(configSoglieTempiDto);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage);

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

            // ✅ AGGIORNA DTO CON ID GENERATO
            configSoglieTempiDto.SogliaId = configSoglieTempi.SogliaId;
            configSoglieTempiDto.DataAggiornamento = configSoglieTempi.DataAggiornamento;

            return configSoglieTempiDto; // ✅ IMPORTANTE: ritorna DTO
        }

        public async Task UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            var configSoglieTempi = await _context.ConfigSoglieTempi
                .FirstOrDefaultAsync(c => c.SogliaId == configSoglieTempiDto.SogliaId);

            if (configSoglieTempi == null)
                return; // ✅ SILENT FAIL - Non lanciare eccezione

            // ✅ VALIDAZIONE BUSINESS LOGIC CON NUOVO METODO
            var validation = await ValidateConfigSoglieAsync(configSoglieTempiDto);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage);

            // ✅ AGGIORNA SOLO SE ESISTE
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

        public async Task<Dictionary<int, ConfigSoglieTempiDTO>> GetSoglieByStatiOrdineAsync(IEnumerable<int> statiOrdineIds)
        {
            var soglie = await _context.ConfigSoglieTempi
                .AsNoTracking()
                .Where(c => statiOrdineIds.Contains(c.StatoOrdineId))
                .Select(c => MapToDTO(c))
                .ToListAsync();

            return soglie.ToDictionary(s => s.StatoOrdineId, s => s);
        }

        public Task<bool> ValidateSoglieAsync(int sogliaAttenzione, int sogliaCritico)
        {
            bool isValid = sogliaCritico > sogliaAttenzione && sogliaAttenzione >= 0 && sogliaCritico >= 0;
            return Task.FromResult(isValid);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateConfigSoglieAsync(ConfigSoglieTempiDTO configDto)
        {
            if (configDto == null)
                return (false, "Configurazione non valida");

            // ✅ VALIDAZIONE SOGLIE NUMERICHE
            if (configDto.SogliaAttenzione < 0 || configDto.SogliaCritico < 0)
                return (false, "Le soglie non possono essere negative");

            // ✅ VALIDAZIONE BUSINESS LOGIC
            if (configDto.SogliaCritico <= configDto.SogliaAttenzione)
                return (false, "La soglia critica deve essere maggiore della soglia di attenzione");

            // ✅ VALIDAZIONE RANGE RAGIONEVOLE
            if (configDto.SogliaAttenzione > 1000 || configDto.SogliaCritico > 1000)
                return (false, "Le soglie non possono superare 1000 minuti");

            // ✅ VERIFICA UNICITÀ STATO ORDINE
            if (configDto.SogliaId == 0) // Nuova configurazione
            {
                if (await ExistsByStatoOrdineIdAsync(configDto.StatoOrdineId))
                    return (false, $"Esiste già una configurazione per lo stato ordine {configDto.StatoOrdineId}");
            }
            else // Configurazione esistente
            {
                if (await ExistsByStatoOrdineIdAsync(configDto.StatoOrdineId, configDto.SogliaId))
                    return (false, $"Esiste già un'altra configurazione per lo stato ordine {configDto.StatoOrdineId}");
            }

            return (true, null);
        }
    }
}