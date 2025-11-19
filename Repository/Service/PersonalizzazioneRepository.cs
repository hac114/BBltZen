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
    public class PersonalizzazioneRepository : IPersonalizzazioneRepository
    {
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        // ✅ METODO PRIVATO PER MAPPING (PATTERN STANDARD)
        private PersonalizzazioneDTO MapToDTO(Personalizzazione personalizzazione)
        {
            return new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = personalizzazione.Nome,
                Descrizione = personalizzazione.Descrizione,
                DtCreazione = personalizzazione.DtCreazione
            };
        }

        public async Task<IEnumerable<PersonalizzazioneDTO>> GetAllAsync()
        {
            return await _context.Personalizzazione
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(p => MapToDTO(p))
                .ToListAsync();
        }

        public async Task<PersonalizzazioneDTO?> GetByIdAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == id);

            return personalizzazione == null ? null : MapToDTO(personalizzazione);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Personalizzazione
                .AnyAsync(p => p.PersonalizzazioneId == id);
        }

        public async Task<bool> ExistsByNameAsync(string nome, int? excludeId = null)
        {
            var query = _context.Personalizzazione
                .Where(p => p.Nome == nome);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.PersonalizzazioneId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<PersonalizzazioneDTO> AddAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            if (personalizzazioneDto == null)
                throw new ArgumentNullException(nameof(personalizzazioneDto));

            // ✅ VERIFICA UNICITÀ NOME
            if (await ExistsByNameAsync(personalizzazioneDto.Nome))
                throw new ArgumentException($"Esiste già una personalizzazione con nome '{personalizzazioneDto.Nome}'");

            var personalizzazione = new Personalizzazione
            {
                Nome = personalizzazioneDto.Nome,
                Descrizione = personalizzazioneDto.Descrizione,
                DtCreazione = DateTime.Now
            };

            _context.Personalizzazione.Add(personalizzazione);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            personalizzazioneDto.PersonalizzazioneId = personalizzazione.PersonalizzazioneId;
            personalizzazioneDto.DtCreazione = personalizzazione.DtCreazione;

            return personalizzazioneDto;
        }

        public async Task UpdateAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            var personalizzazione = await _context.Personalizzazione
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == personalizzazioneDto.PersonalizzazioneId);

            if (personalizzazione == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ NOME (escludendo corrente)
            if (await ExistsByNameAsync(personalizzazioneDto.Nome, personalizzazioneDto.PersonalizzazioneId))
                throw new ArgumentException($"Esiste già un'altra personalizzazione con nome '{personalizzazioneDto.Nome}'");

            personalizzazione.Nome = personalizzazioneDto.Nome;
            personalizzazione.Descrizione = personalizzazioneDto.Descrizione;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == id);

            if (personalizzazione != null)
            {
                // ✅ CONTROLLO VINCOLI REFERENZIALI
                bool hasBevandeStandard = await _context.BevandaStandard
                    .AnyAsync(bs => bs.PersonalizzazioneId == id);

                bool hasIngredienti = await _context.PersonalizzazioneIngrediente
                    .AnyAsync(pi => pi.PersonalizzazioneId == id);

                if (hasBevandeStandard || hasIngredienti)
                {
                    throw new InvalidOperationException(
                        "Impossibile eliminare la personalizzazione perché è collegata a bevande standard o ingredienti."
                    );
                }

                _context.Personalizzazione.Remove(personalizzazione);
                await _context.SaveChangesAsync();
            }
        }        
    }
}