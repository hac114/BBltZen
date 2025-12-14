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
    public class BevandaCustomRepository : IBevandaCustomRepository
    {
        private readonly BubbleTeaContext _context;

        public BevandaCustomRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private BevandaCustomDTO MapToDTO(BevandaCustom bevandaCustom)
        {
            return new BevandaCustomDTO
            {
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                Prezzo = bevandaCustom.Prezzo,
                DataCreazione = bevandaCustom.DataCreazione,
                DataAggiornamento = bevandaCustom.DataAggiornamento
            };
        }

        public async Task<IEnumerable<BevandaCustomDTO>> GetAllAsync()
        {
            return await _context.BevandaCustom
                .AsNoTracking()
                .Select(bc => MapToDTO(bc))
                .ToListAsync();
        }

        public async Task<BevandaCustomDTO?> GetByIdAsync(int id)
        {
            var bevandaCustom = await _context.BevandaCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(bc => bc.ArticoloId == id);

            return bevandaCustom == null ? null : MapToDTO(bevandaCustom);
        }

        public async Task<BevandaCustomDTO?> GetByArticoloIdAsync(int articoloId)
        {
            return await GetByIdAsync(articoloId);
        }

        public async Task<IEnumerable<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId)
        {
            return await _context.BevandaCustom
                .AsNoTracking()
                .Where(bc => bc.PersCustomId == persCustomId)
                .Select(bc => MapToDTO(bc))
                .ToListAsync();
        }

        public async Task<BevandaCustomDTO> AddAsync(BevandaCustomDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // ✅ PATTERN CORRETTO: crea prima Articolo per generare ID automatico
            var articolo = new Articolo
            {
                Tipo = "BEVANDA_CUSTOM",
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Articolo.Add(articolo);
            await _context.SaveChangesAsync();

            // ✅ Crea BevandaCustom con ArticoloId generato automaticamente
            var bevandaCustom = new BevandaCustom
            {
                ArticoloId = articolo.ArticoloId, // ✅ ID GENERATO DAL DB
                PersCustomId = dto.PersCustomId,
                Prezzo = dto.Prezzo,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.BevandaCustom.Add(bevandaCustom);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO con valori dal database (PATTERN STANDARD)
            dto.ArticoloId = bevandaCustom.ArticoloId;
            dto.DataCreazione = bevandaCustom.DataCreazione;
            dto.DataAggiornamento = bevandaCustom.DataAggiornamento;

            return dto; // ✅ RITORNA DTO AGGIORNATO
        }

        public async Task UpdateAsync(BevandaCustomDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var bevandaCustom = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.ArticoloId == dto.ArticoloId);

            if (bevandaCustom == null)
                return; // ✅ SILENT FAIL (PATTERN STANDARD)

            // ✅ Aggiorna solo i campi modificabili (ArticoloId è PK, non si modifica)
            bevandaCustom.PersCustomId = dto.PersCustomId;
            bevandaCustom.Prezzo = dto.Prezzo;
            bevandaCustom.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con valori dal database
            dto.DataAggiornamento = bevandaCustom.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var bevandaCustom = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.ArticoloId == id);

            if (bevandaCustom != null)
            {
                _context.BevandaCustom.Remove(bevandaCustom);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BevandaCustom
                .AnyAsync(bc => bc.ArticoloId == id);
        }

        public async Task<bool> ExistsByArticoloIdAsync(int articoloId)
        {
            return await ExistsAsync(articoloId);
        }

        public async Task<bool> ExistsByPersCustomIdAsync(int persCustomId)
        {
            return await _context.BevandaCustom
                .AnyAsync(bc => bc.PersCustomId == persCustomId);
        }
    }
}
