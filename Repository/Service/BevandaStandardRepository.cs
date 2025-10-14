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
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .ToListAsync();

            var articoli = await _context.Articolo
                .Where(a => bevandeStandard.Select(bs => bs.ArticoloId).Contains(a.ArticoloId))
                .ToDictionaryAsync(a => a.ArticoloId);

            var personalizzazioni = await _context.Personalizzazione
                .Where(p => bevandeStandard.Select(bs => bs.PersonalizzazioneId).Contains(p.PersonalizzazioneId))
                .ToDictionaryAsync(p => p.PersonalizzazioneId);

            var dimensioniBicchieri = await _context.DimensioneBicchiere
                .Where(d => bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            return bevandeStandard.Select(bs => new BevandaStandardDTO
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
                DimensioneBicchiere = dimensioniBicchieri.TryGetValue(bs.DimensioneBicchiereId, out var dimensione)
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore
                    }
                    : null
            }).ToList();
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetDisponibiliAsync()
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.SempreDisponibile) // SOLO bevande sempre disponibili
                .ToListAsync();

            var dimensioniBicchieri = await _context.DimensioneBicchiere
                .Where(d => bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            return bevandeStandard.Select(bs => new BevandaStandardDTO
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
                DimensioneBicchiere = dimensioniBicchieri.TryGetValue(bs.DimensioneBicchiereId, out var dimensione)
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore
                    }
                    : null
            })
            .OrderBy(bs => bs.Priorita)
            .ToList();
        }

        public async Task<BevandaStandardDTO?> GetByIdAsync(int articoloId)
        {
            var bevandaStandard = await _context.BevandaStandard
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId);

            if (bevandaStandard == null) return null;

            // Carica esplicitamente la dimensione bicchiere se necessario
            var dimensioneBicchiere = await _context.DimensioneBicchiere
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == bevandaStandard.DimensioneBicchiereId);

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
                DimensioneBicchiere = dimensioneBicchiere != null
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensioneBicchiere.DimensioneBicchiereId,
                        Sigla = dimensioneBicchiere.Sigla,
                        Descrizione = dimensioneBicchiere.Descrizione,
                        Capienza = dimensioneBicchiere.Capienza,
                        UnitaMisuraId = dimensioneBicchiere.UnitaMisuraId,
                        PrezzoBase = dimensioneBicchiere.PrezzoBase,
                        Moltiplicatore = dimensioneBicchiere.Moltiplicatore
                    }
                    : null
            };
        }

        public async Task<BevandaStandardDTO?> GetByArticoloIdAsync(int articoloId)
        {
            return await GetByIdAsync(articoloId); // Alias per coerenza
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.DimensioneBicchiereId == dimensioneBicchiereId && (bs.Disponibile || bs.SempreDisponibile))
                .ToListAsync();

            var dimensioniBicchieri = await _context.DimensioneBicchiere
                .Where(d => bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            return bevandeStandard.Select(bs => new BevandaStandardDTO
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
                DimensioneBicchiere = dimensioniBicchieri.TryGetValue(bs.DimensioneBicchiereId, out var dimensione)
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore
                    }
                    : null
            })
            .OrderBy(bs => bs.Priorita)
            .ToList();
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetByPersonalizzazioneAsync(int personalizzazioneId)
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.PersonalizzazioneId == personalizzazioneId && bs.SempreDisponibile) // Solo quelle SEMPRE disponibili
                .ToListAsync();

            var dimensioniBicchieri = await _context.DimensioneBicchiere
                .Where(d => bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            return bevandeStandard.Select(bs => new BevandaStandardDTO
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
                DimensioneBicchiere = dimensioniBicchieri.TryGetValue(bs.DimensioneBicchiereId, out var dimensione)
                    ? new DimensioneBicchiereDTO
                    {
                        DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                        Sigla = dimensione.Sigla,
                        Descrizione = dimensione.Descrizione,
                        Capienza = dimensione.Capienza,
                        UnitaMisuraId = dimensione.UnitaMisuraId,
                        PrezzoBase = dimensione.PrezzoBase,
                        Moltiplicatore = dimensione.Moltiplicatore
                    }
                    : null
            })
            .OrderBy(bs => bs.Priorita)
            .ToList();
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