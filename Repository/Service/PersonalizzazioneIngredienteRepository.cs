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
    public class PersonalizzazioneIngredienteRepository : IPersonalizzazioneIngredienteRepository
    {
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneIngredienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetAllAsync()
        {
            return await _context.PersonalizzazioneIngrediente
                .Include(pi => pi.Personalizzazione)
                .Include(pi => pi.Ingrediente)
                .Include(pi => pi.UnitaMisura)
                .Select(pi => new PersonalizzazioneIngredienteDTO
                {
                    PersonalizzazioneIngredienteId = pi.PersonalizzazioneIngredienteId,
                    PersonalizzazioneId = pi.PersonalizzazioneId,
                    IngredienteId = pi.IngredienteId,
                    Quantita = pi.Quantita,
                    UnitaMisuraId = pi.UnitaMisuraId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneIdAsync(int personalizzazioneId)
        {
            return await _context.PersonalizzazioneIngrediente
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .Include(pi => pi.Ingrediente)
                .Include(pi => pi.UnitaMisura)
                .Select(pi => new PersonalizzazioneIngredienteDTO
                {
                    PersonalizzazioneIngredienteId = pi.PersonalizzazioneIngredienteId,
                    PersonalizzazioneId = pi.PersonalizzazioneId,
                    IngredienteId = pi.IngredienteId,
                    Quantita = pi.Quantita,
                    UnitaMisuraId = pi.UnitaMisuraId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByIngredienteIdAsync(int ingredienteId)
        {
            return await _context.PersonalizzazioneIngrediente
                .Where(pi => pi.IngredienteId == ingredienteId)
                .Include(pi => pi.Personalizzazione)
                .Select(pi => new PersonalizzazioneIngredienteDTO
                {
                    PersonalizzazioneIngredienteId = pi.PersonalizzazioneIngredienteId,
                    PersonalizzazioneId = pi.PersonalizzazioneId,
                    IngredienteId = pi.IngredienteId,
                    Quantita = pi.Quantita,
                    UnitaMisuraId = pi.UnitaMisuraId
                })
                .ToListAsync();
        }

        public async Task<PersonalizzazioneIngredienteDTO?> GetByIdAsync(int id)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AsNoTracking() // 👈 AGGIUNGI QUESTA LINEA
                .Include(pi => pi.Personalizzazione)
                .Include(pi => pi.Ingrediente)
                .Include(pi => pi.UnitaMisura)
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneIngredienteId == id);

            if (personalizzazioneIngrediente == null) return null;

            return new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                PersonalizzazioneId = personalizzazioneIngrediente.PersonalizzazioneId,
                IngredienteId = personalizzazioneIngrediente.IngredienteId,
                Quantita = personalizzazioneIngrediente.Quantita,
                UnitaMisuraId = personalizzazioneIngrediente.UnitaMisuraId
            };
        }

        public async Task<PersonalizzazioneIngredienteDTO?> GetByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .Include(pi => pi.Personalizzazione)
                .Include(pi => pi.Ingrediente)
                .Include(pi => pi.UnitaMisura)
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                                         pi.IngredienteId == ingredienteId);

            if (personalizzazioneIngrediente == null) return null;

            return new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                PersonalizzazioneId = personalizzazioneIngrediente.PersonalizzazioneId,
                IngredienteId = personalizzazioneIngrediente.IngredienteId,
                Quantita = personalizzazioneIngrediente.Quantita,
                UnitaMisuraId = personalizzazioneIngrediente.UnitaMisuraId
            };
        }

        public async Task AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            var personalizzazioneIngrediente = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId,
                IngredienteId = personalizzazioneIngredienteDto.IngredienteId,
                Quantita = personalizzazioneIngredienteDto.Quantita,
                UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId
            };

            _context.PersonalizzazioneIngrediente.Add(personalizzazioneIngrediente);
            await _context.SaveChangesAsync();

            personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId;
        }

        public async Task UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            // CERCA l'entità esistente nel database
            var existing = await _context.PersonalizzazioneIngrediente
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneIngredienteId == personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId);

            if (existing == null)
            {
                throw new ArgumentException($"PersonalizzazioneIngrediente con ID {personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId} non trovato");
            }

            // AGGIORNA i valori
            existing.PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId;
            existing.IngredienteId = personalizzazioneIngredienteDto.IngredienteId;
            existing.Quantita = personalizzazioneIngredienteDto.Quantita;
            existing.UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId;

            // SALVA le modifiche
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente.FindAsync(id);
            if (personalizzazioneIngrediente != null)
            {
                _context.PersonalizzazioneIngrediente.Remove(personalizzazioneIngrediente);  // 👈 DELETE FISICO
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                                         pi.IngredienteId == ingredienteId);

            if (personalizzazioneIngrediente != null)
            {
                _context.PersonalizzazioneIngrediente.Remove(personalizzazioneIngrediente);  // 👈 DELETE FISICO
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.PersonalizzazioneIngredienteId == id);
        }

        public async Task<bool> ExistsByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            return await _context.PersonalizzazioneIngrediente
                .AnyAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                              pi.IngredienteId == ingredienteId);
        }

        public async Task<int> GetCountByPersonalizzazioneAsync(int personalizzazioneId)
        {
            return await _context.PersonalizzazioneIngrediente
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .CountAsync();
        }
    }
}