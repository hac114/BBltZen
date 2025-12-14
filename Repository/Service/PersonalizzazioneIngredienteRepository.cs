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
    public class PersonalizzazioneIngredienteRepository : IPersonalizzazioneIngredienteRepository
    {
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneIngredienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private PersonalizzazioneIngredienteDTO MapToDTO(PersonalizzazioneIngrediente personalizzazioneIngrediente)
        {
            return new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                PersonalizzazioneId = personalizzazioneIngrediente.PersonalizzazioneId,
                IngredienteId = personalizzazioneIngrediente.IngredienteId,
                Quantita = personalizzazioneIngrediente.Quantita,
                UnitaMisuraId = personalizzazioneIngrediente.UnitaMisuraId
            };
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetAllAsync()
        {
            return await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Select(pi => MapToDTO(pi))
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneIdAsync(int personalizzazioneId)
        {
            return await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .OrderBy(pi => pi.IngredienteId)
                .Select(pi => MapToDTO(pi)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByIngredienteIdAsync(int ingredienteId)
        {
            return await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Where(pi => pi.IngredienteId == ingredienteId)
                .OrderBy(pi => pi.PersonalizzazioneId)
                .Select(pi => MapToDTO(pi)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<PersonalizzazioneIngredienteDTO?> GetByIdAsync(int id)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneIngredienteId == id);

            return personalizzazioneIngrediente == null ? null : MapToDTO(personalizzazioneIngrediente);
        }

        public async Task<PersonalizzazioneIngredienteDTO?> GetByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                                         pi.IngredienteId == ingredienteId);

            return personalizzazioneIngrediente == null ? null : MapToDTO(personalizzazioneIngrediente); // ✅ USA MapToDTO
        }

        public async Task<PersonalizzazioneIngredienteDTO> AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            if (personalizzazioneIngredienteDto == null)
                throw new ArgumentNullException(nameof(personalizzazioneIngredienteDto));

            // ✅ VERIFICA UNICITÀ
            if (await ExistsByPersonalizzazioneAndIngredienteAsync(
                personalizzazioneIngredienteDto.PersonalizzazioneId,
                personalizzazioneIngredienteDto.IngredienteId))
            {
                throw new ArgumentException("Esiste già un'associazione per questa personalizzazione e ingrediente");
            }

            var personalizzazioneIngrediente = new PersonalizzazioneIngrediente
            {
                PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId,
                IngredienteId = personalizzazioneIngredienteDto.IngredienteId,
                Quantita = personalizzazioneIngredienteDto.Quantita,
                UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId
            };

            _context.PersonalizzazioneIngrediente.Add(personalizzazioneIngrediente);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId;
            return personalizzazioneIngredienteDto;
        }

        public async Task UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            var existing = await _context.PersonalizzazioneIngrediente
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneIngredienteId == personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId);

            if (existing == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ (escludendo corrente)
            if (await _context.PersonalizzazioneIngrediente.AnyAsync(pi =>
                pi.PersonalizzazioneId == personalizzazioneIngredienteDto.PersonalizzazioneId &&
                pi.IngredienteId == personalizzazioneIngredienteDto.IngredienteId &&
                pi.PersonalizzazioneIngredienteId != personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId))
            {
                throw new ArgumentException("Esiste già un'altra associazione per questa personalizzazione e ingrediente");
            }

            existing.PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId;
            existing.IngredienteId = personalizzazioneIngredienteDto.IngredienteId;
            existing.Quantita = personalizzazioneIngredienteDto.Quantita;
            existing.UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .FirstOrDefaultAsync(pi => pi.PersonalizzazioneIngredienteId == id);

            if (personalizzazioneIngrediente != null)
            {
                _context.PersonalizzazioneIngrediente.Remove(personalizzazioneIngrediente);
                await _context.SaveChangesAsync();
            }
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
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