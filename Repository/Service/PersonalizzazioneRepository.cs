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
                .Where(p => !p.IsDeleted)
                .Select(p => new PersonalizzazioneDTO
                {
                    PersonalizzazioneId = p.PersonalizzazioneId,
                    Nome = p.Nome,
                    Descrizione = p.Descrizione,
                    DtCreazione = p.DtCreazione,
                    DtUpdate = p.DtUpdate,
                    IsDeleted = p.IsDeleted
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneDTO>> GetAttiveAsync()
        {
            return await _context.Personalizzazione
                .Where(p => !p.IsDeleted)
                .Select(p => new PersonalizzazioneDTO
                {
                    PersonalizzazioneId = p.PersonalizzazioneId,
                    Nome = p.Nome,
                    Descrizione = p.Descrizione,
                    DtCreazione = p.DtCreazione,
                    DtUpdate = p.DtUpdate,
                    IsDeleted = p.IsDeleted
                })
                .ToListAsync();
        }

        public async Task<PersonalizzazioneDTO?> GetByIdAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == id && !p.IsDeleted);

            if (personalizzazione == null) return null;

            return new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = personalizzazione.Nome,
                Descrizione = personalizzazione.Descrizione,
                DtCreazione = personalizzazione.DtCreazione,
                DtUpdate = personalizzazione.DtUpdate,
                IsDeleted = personalizzazione.IsDeleted
            };
        }

        public async Task AddAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            var personalizzazione = new Personalizzazione
            {
                Nome = personalizzazioneDto.Nome,
                Descrizione = personalizzazioneDto.Descrizione,
                DtCreazione = DateTime.Now,
                DtUpdate = DateTime.Now,
                IsDeleted = false
            };

            _context.Personalizzazione.Add(personalizzazione);
            await _context.SaveChangesAsync();

            personalizzazioneDto.PersonalizzazioneId = personalizzazione.PersonalizzazioneId;
            personalizzazioneDto.DtCreazione = personalizzazione.DtCreazione;
            personalizzazioneDto.DtUpdate = personalizzazione.DtUpdate;
            personalizzazioneDto.IsDeleted = personalizzazione.IsDeleted;
        }

        public async Task UpdateAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            var personalizzazione = await _context.Personalizzazione.FindAsync(personalizzazioneDto.PersonalizzazioneId);
            if (personalizzazione == null)
                throw new ArgumentException("Personalizzazione not found");

            personalizzazione.Nome = personalizzazioneDto.Nome;
            personalizzazione.Descrizione = personalizzazioneDto.Descrizione;
            personalizzazione.DtUpdate = DateTime.Now;

            await _context.SaveChangesAsync();

            personalizzazioneDto.DtUpdate = personalizzazione.DtUpdate;
        }

        public async Task DeleteAsync(int id)
        {
            var personalizzazione = await _context.Personalizzazione.FindAsync(id);
            if (personalizzazione != null)
            {
                personalizzazione.IsDeleted = true;  // SOFT DELETE
                personalizzazione.DtUpdate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Personalizzazione
                .AnyAsync(p => p.PersonalizzazioneId == id && !p.IsDeleted);
        }

        public async Task<bool> ExistsByNameAsync(string nome)
        {
            return await _context.Personalizzazione
                .AnyAsync(p => p.Nome == nome && !p.IsDeleted);
        }
    }
}