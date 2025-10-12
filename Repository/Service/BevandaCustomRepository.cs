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
    public class BevandaCustomRepository : IBevandaCustomRepository
    {
        private readonly BubbleTeaContext _context;

        public BevandaCustomRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BevandaCustomDTO>> GetAllAsync()
        {
            return await _context.BevandaCustom
                .AsNoTracking()
                .Select(bc => new BevandaCustomDTO
                {
                    BevandaCustomId = bc.BevandaCustomId,
                    ArticoloId = bc.ArticoloId,
                    PersCustomId = bc.PersCustomId,
                    Prezzo = bc.Prezzo,
                    DataCreazione = bc.DataCreazione,
                    DataAggiornamento = bc.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<BevandaCustomDTO?> GetByIdAsync(int bevandaCustomId)
        {
            var bevandaCustom = await _context.BevandaCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(bc => bc.BevandaCustomId == bevandaCustomId);

            if (bevandaCustom == null) return null;

            return new BevandaCustomDTO
            {
                BevandaCustomId = bevandaCustom.BevandaCustomId,
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                Prezzo = bevandaCustom.Prezzo,
                DataCreazione = bevandaCustom.DataCreazione,
                DataAggiornamento = bevandaCustom.DataAggiornamento
            };
        }

        public async Task<BevandaCustomDTO?> GetByArticoloIdAsync(int articoloId)
        {
            var bevandaCustom = await _context.BevandaCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(bc => bc.ArticoloId == articoloId);

            if (bevandaCustom == null) return null;

            return new BevandaCustomDTO
            {
                BevandaCustomId = bevandaCustom.BevandaCustomId,
                ArticoloId = bevandaCustom.ArticoloId,
                PersCustomId = bevandaCustom.PersCustomId,
                Prezzo = bevandaCustom.Prezzo,
                DataCreazione = bevandaCustom.DataCreazione,
                DataAggiornamento = bevandaCustom.DataAggiornamento
            };
        }

        public async Task<IEnumerable<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId)
        {
            return await _context.BevandaCustom
                .AsNoTracking()
                .Where(bc => bc.PersCustomId == persCustomId)
                .Select(bc => new BevandaCustomDTO
                {
                    BevandaCustomId = bc.BevandaCustomId,
                    ArticoloId = bc.ArticoloId,
                    PersCustomId = bc.PersCustomId,
                    Prezzo = bc.Prezzo,
                    DataCreazione = bc.DataCreazione,
                    DataAggiornamento = bc.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task AddAsync(BevandaCustomDTO bevandaCustomDto)
        {
            var bevandaCustom = new BevandaCustom
            {
                BevandaCustomId = bevandaCustomDto.BevandaCustomId,
                ArticoloId = bevandaCustomDto.ArticoloId,
                PersCustomId = bevandaCustomDto.PersCustomId,
                Prezzo = bevandaCustomDto.Prezzo,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.BevandaCustom.Add(bevandaCustom);
            await _context.SaveChangesAsync();

            bevandaCustomDto.DataCreazione = bevandaCustom.DataCreazione;
            bevandaCustomDto.DataAggiornamento = bevandaCustom.DataAggiornamento;
        }

        public async Task UpdateAsync(BevandaCustomDTO bevandaCustomDto)
        {
            var bevandaCustom = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.BevandaCustomId == bevandaCustomDto.BevandaCustomId);

            if (bevandaCustom == null)
                throw new ArgumentException($"BevandaCustom con BevandaCustomId {bevandaCustomDto.BevandaCustomId} non trovata");

            bevandaCustom.ArticoloId = bevandaCustomDto.ArticoloId;
            bevandaCustom.PersCustomId = bevandaCustomDto.PersCustomId;
            bevandaCustom.Prezzo = bevandaCustomDto.Prezzo;
            bevandaCustom.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            bevandaCustomDto.DataAggiornamento = bevandaCustom.DataAggiornamento;
        }

        public async Task DeleteAsync(int bevandaCustomId)
        {
            var bevandaCustom = await _context.BevandaCustom
                .FirstOrDefaultAsync(bc => bc.BevandaCustomId == bevandaCustomId);

            if (bevandaCustom != null)
            {
                _context.BevandaCustom.Remove(bevandaCustom);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int bevandaCustomId)
        {
            return await _context.BevandaCustom
                .AnyAsync(bc => bc.BevandaCustomId == bevandaCustomId);
        }

        public async Task<bool> ExistsByArticoloIdAsync(int articoloId)
        {
            return await _context.BevandaCustom
                .AnyAsync(bc => bc.ArticoloId == articoloId);
        }

        public async Task<bool> ExistsByPersCustomIdAsync(int persCustomId)
        {
            return await _context.BevandaCustom
                .AnyAsync(bc => bc.PersCustomId == persCustomId);
        }
    }
}
