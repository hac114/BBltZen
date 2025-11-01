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

        public async Task<IEnumerable<PersonalizzazioneDTO>> GetAllAsync()
        {
            return await _context.Personalizzazione
                .Select(p => new PersonalizzazioneDTO
                {
                    PersonalizzazioneId = p.PersonalizzazioneId,
                    Nome = p.Nome,
                    Descrizione = p.Descrizione,
                    DtCreazione = p.DtCreazione
                })
                .ToListAsync();
        }

        public async Task<PersonalizzazioneDTO?> GetByIdAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == id);

            if (personalizzazione == null) return null;

            return new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = personalizzazione.Nome,
                Descrizione = personalizzazione.Descrizione,
                DtCreazione = personalizzazione.DtCreazione
            };
        }

        public async Task AddAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            var personalizzazione = new Personalizzazione
            {
                Nome = personalizzazioneDto.Nome,
                Descrizione = personalizzazioneDto.Descrizione,
                DtCreazione = DateTime.Now
            };

            _context.Personalizzazione.Add(personalizzazione);
            await _context.SaveChangesAsync();

            personalizzazioneDto.PersonalizzazioneId = personalizzazione.PersonalizzazioneId;
            personalizzazioneDto.DtCreazione = personalizzazione.DtCreazione;
        }

        public async Task UpdateAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            var personalizzazione = await _context.Personalizzazione.FindAsync(personalizzazioneDto.PersonalizzazioneId);
            if (personalizzazione == null)
                throw new ArgumentException("Personalizzazione not found");

            personalizzazione.Nome = personalizzazioneDto.Nome;
            personalizzazione.Descrizione = personalizzazioneDto.Descrizione;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione.FindAsync(id);
            if (personalizzazione != null)
            {
                // ✅ Controlla TUTTE le possibili dipendenze
                var hasBevandeStandard = await _context.BevandaStandard
                    .AnyAsync(bs => bs.PersonalizzazioneId == id);

                var hasIngredienti = await _context.PersonalizzazioneIngrediente
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

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Personalizzazione
                .AnyAsync(p => p.PersonalizzazioneId == id);
        }

        public async Task<bool> ExistsByNameAsync(string nome)
        {
            return await _context.Personalizzazione
                .AnyAsync(p => p.Nome == nome);
        }
    }
}