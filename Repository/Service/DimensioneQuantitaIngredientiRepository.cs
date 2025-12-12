using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class DimensioneQuantitaIngredientiRepository : IDimensioneQuantitaIngredientiRepository
    {
        private readonly BubbleTeaContext _context;

        public DimensioneQuantitaIngredientiRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private DimensioneQuantitaIngredientiDTO MapToDTO(DimensioneQuantitaIngredienti dimensioneQuantita)
        {
            return new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = dimensioneQuantita.DimensioneId,
                PersonalizzazioneIngredienteId = dimensioneQuantita.PersonalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneQuantita.DimensioneBicchiereId,
                Moltiplicatore = dimensioneQuantita.Moltiplicatore
            };
        }

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetAllAsync()
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .OrderBy(dq => dq.DimensioneId)
                .ThenBy(dq => dq.PersonalizzazioneIngredienteId)
                .Select(dq => MapToDTO(dq))
                .ToListAsync();
        }

        public async Task<DimensioneQuantitaIngredientiDTO?> GetByIdAsync(int dimensioneId, int personalizzazioneIngredienteId)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .FirstOrDefaultAsync(dq => dq.DimensioneId == dimensioneId &&
                                         dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            return dimensioneQuantita == null ? null : MapToDTO(dimensioneQuantita);
        }

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .Where(dq => dq.DimensioneBicchiereId == dimensioneBicchiereId)
                .OrderBy(dq => dq.DimensioneId)
                .Select(dq => MapToDTO(dq))
                .ToListAsync();
        }

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .Where(dq => dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId)
                .OrderBy(dq => dq.DimensioneId) // ✅ AGGIUNTO OrderBy
                .Select(dq => MapToDTO(dq)) // ✅ USA MapToDTO invece di mapping inline
                .ToListAsync();
        }

        public async Task<DimensioneQuantitaIngredientiDTO?> GetByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .FirstOrDefaultAsync(dq => dq.DimensioneBicchiereId == dimensioneBicchiereId &&
                                         dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            return dimensioneQuantita == null ? null : MapToDTO(dimensioneQuantita);
        }

        public async Task<DimensioneQuantitaIngredientiDTO> AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            if (dimensioneQuantitaDto == null)
                throw new ArgumentNullException(nameof(dimensioneQuantitaDto));

            // ✅ VERIFICA UNICITÀ
            if (await ExistsByCombinazioneAsync(dimensioneQuantitaDto.DimensioneBicchiereId, dimensioneQuantitaDto.PersonalizzazioneIngredienteId))
            {
                throw new ArgumentException($"Esiste già una combinazione per questa dimensione bicchiere e personalizzazione ingrediente");
            }

            var dimensioneQuantita = new DimensioneQuantitaIngredienti
            {
                // ✅ NON impostare DimensioneId - sarà generato automaticamente
                PersonalizzazioneIngredienteId = dimensioneQuantitaDto.PersonalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneQuantitaDto.DimensioneBicchiereId,
                Moltiplicatore = dimensioneQuantitaDto.Moltiplicatore
            };

            _context.DimensioneQuantitaIngredienti.Add(dimensioneQuantita);
            await _context.SaveChangesAsync();

            // ✅ CORREZIONE: Aggiorna il DTO con l'ID generato
            dimensioneQuantitaDto.DimensioneId = dimensioneQuantita.DimensioneId;
            return dimensioneQuantitaDto;
        }

        public async Task UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .FirstOrDefaultAsync(dq => dq.DimensioneId == dimensioneQuantitaDto.DimensioneId);

            if (dimensioneQuantita == null)
                return; // ✅ SILENT FAIL

            // ✅ CORREZIONE: Verifica unicità ESCLUDENDO solo per DimensioneId
            var existingCombinazione = await _context.DimensioneQuantitaIngredienti
                .AnyAsync(dq => dq.DimensioneBicchiereId == dimensioneQuantitaDto.DimensioneBicchiereId &&
                              dq.PersonalizzazioneIngredienteId == dimensioneQuantitaDto.PersonalizzazioneIngredienteId &&
                              dq.DimensioneId != dimensioneQuantitaDto.DimensioneId); // ✅ Solo questa condizione

            if (existingCombinazione)
            {
                throw new ArgumentException($"Esiste già un'altra combinazione per questa dimensione bicchiere e personalizzazione ingrediente");
            }

            // ✅ AGGIORNA I CAMPI MODIFICABILI
            dimensioneQuantita.DimensioneBicchiereId = dimensioneQuantitaDto.DimensioneBicchiereId;
            dimensioneQuantita.PersonalizzazioneIngredienteId = dimensioneQuantitaDto.PersonalizzazioneIngredienteId;
            dimensioneQuantita.Moltiplicatore = dimensioneQuantitaDto.Moltiplicatore;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int dimensioneId, int personalizzazioneIngredienteId)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .FirstOrDefaultAsync(dq => dq.DimensioneId == dimensioneId &&
                                         dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            if (dimensioneQuantita != null)
            {
                _context.DimensioneQuantitaIngredienti.Remove(dimensioneQuantita);
                await _context.SaveChangesAsync();
            }
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
        }

        public async Task<bool> ExistsAsync(int dimensioneId, int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AnyAsync(dq => dq.DimensioneId == dimensioneId &&
                              dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
        }

        public async Task<bool> ExistsByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AnyAsync(dq => dq.DimensioneBicchiereId == dimensioneBicchiereId &&
                              dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
        }
    }
}