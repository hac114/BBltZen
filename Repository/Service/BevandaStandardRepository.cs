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
    public class BevandaStandardRepository : IBevandaStandardRepository
    {
        private readonly BubbleTeaContext _context;

        public BevandaStandardRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetAllAsync()
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .Include(bs => bs.Articolo)
                .Include(bs => bs.Personalizzazione)
                .Include(bs => bs.DimensioneBicchiere)
                .Select(bs => new BevandaStandardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    PersonalizzazioneId = bs.PersonalizzazioneId,
                    DimensioneBicchiereId = bs.DimensioneBicchiereId,
                    Prezzo = bs.Prezzo,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    DataCreazione = bs.DataCreazione,
                    DataAggiornamento = bs.DataAggiornamento,
                    DimensioneBicchiere = new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = bs.DimensioneBicchiere.DimensioneBicchiereId,
                        Sigla = bs.DimensioneBicchiere.Sigla,
                        Descrizione = bs.DimensioneBicchiere.Descrizione,
                        Capienza = bs.DimensioneBicchiere.Capienza,
                        UnitaMisuraId = bs.DimensioneBicchiere.UnitaMisuraId,
                        PrezzoBase = bs.DimensioneBicchiere.PrezzoBase,
                        Moltiplicatore = bs.DimensioneBicchiere.Moltiplicatore
                    }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetDisponibiliAsync()
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.SempreDisponibile) // SOLO bevande sempre disponibili
                .Include(bs => bs.Articolo)
                .Include(bs => bs.Personalizzazione)
                .Include(bs => bs.DimensioneBicchiere)
                .Select(bs => new BevandaStandardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    PersonalizzazioneId = bs.PersonalizzazioneId,
                    DimensioneBicchiereId = bs.DimensioneBicchiereId,
                    Prezzo = bs.Prezzo,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    DataCreazione = bs.DataCreazione,
                    DataAggiornamento = bs.DataAggiornamento,
                    DimensioneBicchiere = new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = bs.DimensioneBicchiere.DimensioneBicchiereId,
                        Sigla = bs.DimensioneBicchiere.Sigla,
                        Descrizione = bs.DimensioneBicchiere.Descrizione,
                        Capienza = bs.DimensioneBicchiere.Capienza,
                        UnitaMisuraId = bs.DimensioneBicchiere.UnitaMisuraId,
                        PrezzoBase = bs.DimensioneBicchiere.PrezzoBase,
                        Moltiplicatore = bs.DimensioneBicchiere.Moltiplicatore
                    }
                })
                .OrderBy(bs => bs.Priorita)
                .ToListAsync();
        }

        public async Task<BevandaStandardDTO?> GetByIdAsync(int articoloId)
        {
            var bevandaStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Include(bs => bs.Articolo)
                .Include(bs => bs.Personalizzazione)
                .Include(bs => bs.DimensioneBicchiere)
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

            if (bevandaStandard == null) return null;

            return new BevandaStandardDTO
            {
                ArticoloId = bevandaStandard.ArticoloId,
                PersonalizzazioneId = bevandaStandard.PersonalizzazioneId,
                DimensioneBicchiereId = bevandaStandard.DimensioneBicchiereId,
                Prezzo = bevandaStandard.Prezzo,
                ImmagineUrl = bevandaStandard.ImmagineUrl,
                Disponibile = bevandaStandard.Disponibile,
                SempreDisponibile = bevandaStandard.SempreDisponibile,
                Priorita = bevandaStandard.Priorita,
                DataCreazione = bevandaStandard.DataCreazione,
                DataAggiornamento = bevandaStandard.DataAggiornamento,
                DimensioneBicchiere = new DimensioneBicchiereDTO
                {
                    DimensioneBicchiereId = bevandaStandard.DimensioneBicchiere.DimensioneBicchiereId,
                    Sigla = bevandaStandard.DimensioneBicchiere.Sigla,
                    Descrizione = bevandaStandard.DimensioneBicchiere.Descrizione,
                    Capienza = bevandaStandard.DimensioneBicchiere.Capienza,
                    UnitaMisuraId = bevandaStandard.DimensioneBicchiere.UnitaMisuraId,
                    PrezzoBase = bevandaStandard.DimensioneBicchiere.PrezzoBase,
                    Moltiplicatore = bevandaStandard.DimensioneBicchiere.Moltiplicatore
                }
            };
        }

        public async Task<BevandaStandardDTO?> GetByArticoloIdAsync(int articoloId)
        {
            return await GetByIdAsync(articoloId); // Alias per coerenza
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.DimensioneBicchiereId == dimensioneBicchiereId && (bs.Disponibile || bs.SempreDisponibile))
                .Include(bs => bs.Articolo)
                .Include(bs => bs.Personalizzazione)
                .Include(bs => bs.DimensioneBicchiere)
                .Select(bs => new BevandaStandardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    PersonalizzazioneId = bs.PersonalizzazioneId,
                    DimensioneBicchiereId = bs.DimensioneBicchiereId,
                    Prezzo = bs.Prezzo,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    DataCreazione = bs.DataCreazione,
                    DataAggiornamento = bs.DataAggiornamento
                })
                .OrderBy(bs => bs.Priorita)
                .ToListAsync();
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetByPersonalizzazioneAsync(int personalizzazioneId)
        {
            return await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.PersonalizzazioneId == personalizzazioneId && bs.SempreDisponibile) // Solo quelle SEMPRE disponibili
                .Include(bs => bs.Articolo)
                .Include(bs => bs.DimensioneBicchiere)
                .Select(bs => new BevandaStandardDTO
                {
                    ArticoloId = bs.ArticoloId,
                    PersonalizzazioneId = bs.PersonalizzazioneId,
                    DimensioneBicchiereId = bs.DimensioneBicchiereId,
                    Prezzo = bs.Prezzo,
                    ImmagineUrl = bs.ImmagineUrl,
                    Disponibile = bs.Disponibile,
                    SempreDisponibile = bs.SempreDisponibile,
                    Priorita = bs.Priorita,
                    DataCreazione = bs.DataCreazione,
                    DataAggiornamento = bs.DataAggiornamento
                })
                .OrderBy(bs => bs.Priorita)
                .ToListAsync();
        }

        public async Task AddAsync(BevandaStandardDTO bevandaStandardDto)
        {
            var bevandaStandard = new BevandaStandard
            {
                ArticoloId = bevandaStandardDto.ArticoloId,
                PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId,
                DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId,
                Prezzo = bevandaStandardDto.Prezzo,
                ImmagineUrl = bevandaStandardDto.ImmagineUrl,
                Disponibile = bevandaStandardDto.Disponibile,
                SempreDisponibile = bevandaStandardDto.SempreDisponibile,
                Priorita = bevandaStandardDto.Priorita,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.BevandaStandard.Add(bevandaStandard);
            await _context.SaveChangesAsync();

            bevandaStandardDto.DataCreazione = bevandaStandard.DataCreazione;
            bevandaStandardDto.DataAggiornamento = bevandaStandard.DataAggiornamento;
        }

        public async Task UpdateAsync(BevandaStandardDTO bevandaStandardDto)
        {
            var bevandaStandard = await _context.BevandaStandard
                .FirstOrDefaultAsync(bs => bs.ArticoloId == bevandaStandardDto.ArticoloId);

            if (bevandaStandard == null)
                throw new ArgumentException($"BevandaStandard con ArticoloId {bevandaStandardDto.ArticoloId} non trovata");

            bevandaStandard.PersonalizzazioneId = bevandaStandardDto.PersonalizzazioneId;
            bevandaStandard.DimensioneBicchiereId = bevandaStandardDto.DimensioneBicchiereId;
            bevandaStandard.Prezzo = bevandaStandardDto.Prezzo;
            bevandaStandard.ImmagineUrl = bevandaStandardDto.ImmagineUrl;
            bevandaStandard.Disponibile = bevandaStandardDto.Disponibile;
            bevandaStandard.SempreDisponibile = bevandaStandardDto.SempreDisponibile;
            bevandaStandard.Priorita = bevandaStandardDto.Priorita;
            bevandaStandard.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            bevandaStandardDto.DataAggiornamento = bevandaStandard.DataAggiornamento;
        }

        public async Task DeleteAsync(int articoloId)
        {
            var bevandaStandard = await _context.BevandaStandard
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

            if (bevandaStandard != null)
            {
                _context.BevandaStandard.Remove(bevandaStandard);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int articoloId)
        {
            return await _context.BevandaStandard
                .AnyAsync(bs => bs.ArticoloId == articoloId);
        }

        public async Task<bool> ExistsByCombinazioneAsync(int personalizzazioneId, int dimensioneBicchiereId)
        {
            return await _context.BevandaStandard
                .AnyAsync(bs => bs.PersonalizzazioneId == personalizzazioneId &&
                              bs.DimensioneBicchiereId == dimensioneBicchiereId);
        }
    }
}
