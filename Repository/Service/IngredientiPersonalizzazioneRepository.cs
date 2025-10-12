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
    public class IngredientiPersonalizzazioneRepository : IIngredientiPersonalizzazioneRepository
    {
        private readonly BubbleTeaContext _context;

        public IngredientiPersonalizzazioneRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetAllAsync()
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Select(ip => new IngredientiPersonalizzazioneDTO
                {
                    IngredientePersId = ip.IngredientePersId,
                    PersCustomId = ip.PersCustomId,
                    IngredienteId = ip.IngredienteId,
                    DataCreazione = ip.DataCreazione
                })
                .ToListAsync();
        }

        public async Task<IngredientiPersonalizzazioneDTO?> GetByIdAsync(int ingredientePersId)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientePersId);

            if (ingredientePers == null) return null;

            return new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = ingredientePers.IngredientePersId,
                PersCustomId = ingredientePers.PersCustomId,
                IngredienteId = ingredientePers.IngredienteId,
                DataCreazione = ingredientePers.DataCreazione
            };
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Where(ip => ip.PersCustomId == persCustomId)
                .Select(ip => new IngredientiPersonalizzazioneDTO
                {
                    IngredientePersId = ip.IngredientePersId,
                    PersCustomId = ip.PersCustomId,
                    IngredienteId = ip.IngredienteId,
                    DataCreazione = ip.DataCreazione
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Where(ip => ip.IngredienteId == ingredienteId)
                .Select(ip => new IngredientiPersonalizzazioneDTO
                {
                    IngredientePersId = ip.IngredientePersId,
                    PersCustomId = ip.PersCustomId,
                    IngredienteId = ip.IngredienteId,
                    DataCreazione = ip.DataCreazione
                })
                .ToListAsync();
        }

        public async Task<IngredientiPersonalizzazioneDTO?> GetByCombinazioneAsync(int persCustomId, int ingredienteId)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.PersCustomId == persCustomId && ip.IngredienteId == ingredienteId);

            if (ingredientePers == null) return null;

            return new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = ingredientePers.IngredientePersId,
                PersCustomId = ingredientePers.PersCustomId,
                IngredienteId = ingredientePers.IngredienteId,
                DataCreazione = ingredientePers.DataCreazione
            };
        }

        public async Task AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            var ingredientePers = new IngredientiPersonalizzazione
            {
                IngredientePersId = ingredientiPersDto.IngredientePersId,
                PersCustomId = ingredientiPersDto.PersCustomId,
                IngredienteId = ingredientiPersDto.IngredienteId,
                DataCreazione = DateTime.Now
            };

            _context.IngredientiPersonalizzazione.Add(ingredientePers);
            await _context.SaveChangesAsync();

            ingredientiPersDto.DataCreazione = ingredientePers.DataCreazione;
        }

        public async Task UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientiPersDto.IngredientePersId);

            if (ingredientePers == null)
                throw new ArgumentException($"IngredientiPersonalizzazione con IngredientePersId {ingredientiPersDto.IngredientePersId} non trovata");

            ingredientePers.PersCustomId = ingredientiPersDto.PersCustomId;
            ingredientePers.IngredienteId = ingredientiPersDto.IngredienteId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int ingredientePersId)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientePersId);

            if (ingredientePers != null)
            {
                _context.IngredientiPersonalizzazione.Remove(ingredientePers);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int ingredientePersId)
        {
            return await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.IngredientePersId == ingredientePersId);
        }

        public async Task<bool> ExistsByCombinazioneAsync(int persCustomId, int ingredienteId)
        {
            return await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.PersCustomId == persCustomId && ip.IngredienteId == ingredienteId);
        }
    }
}
