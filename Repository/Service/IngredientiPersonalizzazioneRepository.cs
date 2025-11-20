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

        private IngredientiPersonalizzazioneDTO MapToDTO(IngredientiPersonalizzazione ingredientePers)
        {
            return new IngredientiPersonalizzazioneDTO
            {
                IngredientePersId = ingredientePers.IngredientePersId,
                PersCustomId = ingredientePers.PersCustomId,
                IngredienteId = ingredientePers.IngredienteId,
                DataCreazione = ingredientePers.DataCreazione
            };
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetAllAsync()
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .OrderByDescending(ip => ip.DataCreazione)
                .ThenBy(ip => ip.IngredientePersId)
                .Select(ip => MapToDTO(ip))
                .ToListAsync();
        }

        public async Task<IngredientiPersonalizzazioneDTO?> GetByIdAsync(int ingredientePersId)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientePersId);

            return ingredientePers == null ? null : MapToDTO(ingredientePers);
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Where(ip => ip.PersCustomId == persCustomId)
                .OrderByDescending(ip => ip.DataCreazione)
                .Select(ip => MapToDTO(ip))
                .ToListAsync();
        }

        public async Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId)
        {
            return await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .Where(ip => ip.IngredienteId == ingredienteId)
                .OrderByDescending(ip => ip.DataCreazione)
                .Select(ip => MapToDTO(ip))
                .ToListAsync();
        }

        public async Task<IngredientiPersonalizzazioneDTO?> GetByCombinazioneAsync(int persCustomId, int ingredienteId)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(ip => ip.PersCustomId == persCustomId && ip.IngredienteId == ingredienteId);

            return ingredientePers == null ? null : MapToDTO(ingredientePers);
        }

        public async Task<IngredientiPersonalizzazioneDTO> AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            if (ingredientiPersDto == null)
                throw new ArgumentNullException(nameof(ingredientiPersDto));

            // ✅ VERIFICA UNICITÀ COMBINAZIONE
            if (await ExistsByCombinazioneAsync(ingredientiPersDto.PersCustomId, ingredientiPersDto.IngredienteId))
            {
                throw new ArgumentException($"Esiste già questa combinazione di personalizzazione e ingrediente");
            }

            var ingredientePers = new IngredientiPersonalizzazione
            {
                // ✅ NON impostare IngredientePersId - sarà generato automaticamente
                PersCustomId = ingredientiPersDto.PersCustomId,
                IngredienteId = ingredientiPersDto.IngredienteId,
                DataCreazione = DateTime.UtcNow // ✅ UTC per consistenza
            };

            _context.IngredientiPersonalizzazione.Add(ingredientePers);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            ingredientiPersDto.IngredientePersId = ingredientePers.IngredientePersId;
            ingredientiPersDto.DataCreazione = ingredientePers.DataCreazione;
            return ingredientiPersDto;
        }


        public async Task UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto)
        {
            var ingredientePers = await _context.IngredientiPersonalizzazione
                .FirstOrDefaultAsync(ip => ip.IngredientePersId == ingredientiPersDto.IngredientePersId);

            if (ingredientePers == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ COMBINAZIONE (escludendo il record corrente)
            var existingCombinazione = await _context.IngredientiPersonalizzazione
                .AnyAsync(ip => ip.PersCustomId == ingredientiPersDto.PersCustomId &&
                              ip.IngredienteId == ingredientiPersDto.IngredienteId &&
                              ip.IngredientePersId != ingredientiPersDto.IngredientePersId);

            if (existingCombinazione)
            {
                throw new ArgumentException($"Esiste già un'altra combinazione per questa personalizzazione e ingrediente");
            }

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
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
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
