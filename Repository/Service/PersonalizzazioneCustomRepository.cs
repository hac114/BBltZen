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
    public class PersonalizzazioneCustomRepository : IPersonalizzazioneCustomRepository
    {
        private readonly BubbleTeaContext _context;

        public PersonalizzazioneCustomRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PersonalizzazioneCustomDTO>> GetAllAsync()
        {
            return await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .Select(pc => new PersonalizzazioneCustomDTO
                {
                    PersCustomId = pc.PersCustomId,
                    Nome = pc.Nome,
                    GradoDolcezza = pc.GradoDolcezza,
                    DimensioneBicchiereId = pc.DimensioneBicchiereId,
                    DataCreazione = pc.DataCreazione,
                    DataAggiornamento = pc.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<PersonalizzazioneCustomDTO?> GetByIdAsync(int persCustomId)
        {
            var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .FirstOrDefaultAsync(pc => pc.PersCustomId == persCustomId);

            if (personalizzazioneCustom == null) return null;

            return new PersonalizzazioneCustomDTO
            {
                PersCustomId = personalizzazioneCustom.PersCustomId,
                Nome = personalizzazioneCustom.Nome,
                GradoDolcezza = personalizzazioneCustom.GradoDolcezza,
                DimensioneBicchiereId = personalizzazioneCustom.DimensioneBicchiereId,
                DataCreazione = personalizzazioneCustom.DataCreazione,
                DataAggiornamento = personalizzazioneCustom.DataAggiornamento
            };
        }

        public async Task<IEnumerable<PersonalizzazioneCustomDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            return await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .Where(pc => pc.DimensioneBicchiereId == dimensioneBicchiereId)
                .Select(pc => new PersonalizzazioneCustomDTO
                {
                    PersCustomId = pc.PersCustomId,
                    Nome = pc.Nome,
                    GradoDolcezza = pc.GradoDolcezza,
                    DimensioneBicchiereId = pc.DimensioneBicchiereId,
                    DataCreazione = pc.DataCreazione,
                    DataAggiornamento = pc.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalizzazioneCustomDTO>> GetByGradoDolcezzaAsync(byte gradoDolcezza)
        {
            return await _context.PersonalizzazioneCustom
                .AsNoTracking()
                .Where(pc => pc.GradoDolcezza == gradoDolcezza)
                .Select(pc => new PersonalizzazioneCustomDTO
                {
                    PersCustomId = pc.PersCustomId,
                    Nome = pc.Nome,
                    GradoDolcezza = pc.GradoDolcezza,
                    DimensioneBicchiereId = pc.DimensioneBicchiereId,
                    DataCreazione = pc.DataCreazione,
                    DataAggiornamento = pc.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<PersonalizzazioneCustomDTO> AddAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto) // ✅ CORREGGI: ritorna DTO
        {
            if (personalizzazioneCustomDto == null)
                throw new ArgumentNullException(nameof(personalizzazioneCustomDto));

            var personalizzazioneCustom = new PersonalizzazioneCustom
            {
                Nome = personalizzazioneCustomDto.Nome ?? "Bevanda Custom", // ✅ NOT NULL - valore default
                GradoDolcezza = personalizzazioneCustomDto.GradoDolcezza,
                DimensioneBicchiereId = personalizzazioneCustomDto.DimensioneBicchiereId,
                DataCreazione = DateTime.Now, // ✅ NOT NULL - valore default
                DataAggiornamento = DateTime.Now // ✅ NOT NULL - valore default
            };

            _context.PersonalizzazioneCustom.Add(personalizzazioneCustom);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
            personalizzazioneCustomDto.PersCustomId = personalizzazioneCustom.PersCustomId;
            personalizzazioneCustomDto.DataCreazione = personalizzazioneCustom.DataCreazione;
            personalizzazioneCustomDto.DataAggiornamento = personalizzazioneCustom.DataAggiornamento;

            return personalizzazioneCustomDto; // ✅ AGGIUNGI return
        }

        public async Task UpdateAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            if (personalizzazioneCustomDto == null) // ✅ AGGIUNGI validazione
                throw new ArgumentNullException(nameof(personalizzazioneCustomDto));

            var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                .FirstOrDefaultAsync(pc => pc.PersCustomId == personalizzazioneCustomDto.PersCustomId);

            if (personalizzazioneCustom == null)
                throw new ArgumentException($"PersonalizzazioneCustom con PersCustomId {personalizzazioneCustomDto.PersCustomId} non trovata");

            personalizzazioneCustom.Nome = personalizzazioneCustomDto.Nome ?? "Bevanda Custom"; // ✅ NOT NULL - valore default
            personalizzazioneCustom.GradoDolcezza = personalizzazioneCustomDto.GradoDolcezza;
            personalizzazioneCustom.DimensioneBicchiereId = personalizzazioneCustomDto.DimensioneBicchiereId;
            personalizzazioneCustom.DataAggiornamento = DateTime.Now; // ✅ NOT NULL - aggiornamento automatico

            await _context.SaveChangesAsync();

            personalizzazioneCustomDto.DataAggiornamento = personalizzazioneCustom.DataAggiornamento;
        }

        public async Task DeleteAsync(int persCustomId)
        {
            var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                .FirstOrDefaultAsync(pc => pc.PersCustomId == persCustomId);

            if (personalizzazioneCustom != null)
            {
                _context.PersonalizzazioneCustom.Remove(personalizzazioneCustom);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int persCustomId)
        {
            return await _context.PersonalizzazioneCustom
                .AnyAsync(pc => pc.PersCustomId == persCustomId);
        }
    }
}