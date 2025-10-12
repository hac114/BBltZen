﻿using Database;
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

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetAllAsync()
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .Select(dq => new DimensioneQuantitaIngredientiDTO
                {
                    DimensioneId = dq.DimensioneId,
                    PersonalizzazioneIngredienteId = dq.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dq.DimensioneBicchiereId,
                    Moltiplicatore = dq.Moltiplicatore
                })
                .ToListAsync();
        }

        public async Task<DimensioneQuantitaIngredientiDTO?> GetByIdAsync(int dimensioneId, int personalizzazioneIngredienteId)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .FirstOrDefaultAsync(dq => dq.DimensioneId == dimensioneId &&
                                         dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            if (dimensioneQuantita == null) return null;

            return new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = dimensioneQuantita.DimensioneId,
                PersonalizzazioneIngredienteId = dimensioneQuantita.PersonalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneQuantita.DimensioneBicchiereId,
                Moltiplicatore = dimensioneQuantita.Moltiplicatore
            };
        }

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .Where(dq => dq.DimensioneBicchiereId == dimensioneBicchiereId)
                .Select(dq => new DimensioneQuantitaIngredientiDTO
                {
                    DimensioneId = dq.DimensioneId,
                    PersonalizzazioneIngredienteId = dq.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dq.DimensioneBicchiereId,
                    Moltiplicatore = dq.Moltiplicatore
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .Where(dq => dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId)
                .Select(dq => new DimensioneQuantitaIngredientiDTO
                {
                    DimensioneId = dq.DimensioneId,
                    PersonalizzazioneIngredienteId = dq.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dq.DimensioneBicchiereId,
                    Moltiplicatore = dq.Moltiplicatore
                })
                .ToListAsync();
        }

        public async Task<DimensioneQuantitaIngredientiDTO?> GetByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .FirstOrDefaultAsync(dq => dq.DimensioneBicchiereId == dimensioneBicchiereId &&
                                         dq.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

            if (dimensioneQuantita == null) return null;

            return new DimensioneQuantitaIngredientiDTO
            {
                DimensioneId = dimensioneQuantita.DimensioneId,
                PersonalizzazioneIngredienteId = dimensioneQuantita.PersonalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneQuantita.DimensioneBicchiereId,
                Moltiplicatore = dimensioneQuantita.Moltiplicatore
            };
        }

        public async Task AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            var dimensioneQuantita = new DimensioneQuantitaIngredienti
            {
                DimensioneId = dimensioneQuantitaDto.DimensioneId,
                PersonalizzazioneIngredienteId = dimensioneQuantitaDto.PersonalizzazioneIngredienteId,
                DimensioneBicchiereId = dimensioneQuantitaDto.DimensioneBicchiereId,
                Moltiplicatore = dimensioneQuantitaDto.Moltiplicatore
            };

            _context.DimensioneQuantitaIngredienti.Add(dimensioneQuantita);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto)
        {
            var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                .FirstOrDefaultAsync(dq => dq.DimensioneId == dimensioneQuantitaDto.DimensioneId &&
                                         dq.PersonalizzazioneIngredienteId == dimensioneQuantitaDto.PersonalizzazioneIngredienteId);

            if (dimensioneQuantita == null)
                throw new ArgumentException($"DimensioneQuantitaIngredienti non trovata con DimensioneId {dimensioneQuantitaDto.DimensioneId} e PersonalizzazioneIngredienteId {dimensioneQuantitaDto.PersonalizzazioneIngredienteId}");

            dimensioneQuantita.DimensioneBicchiereId = dimensioneQuantitaDto.DimensioneBicchiereId;
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