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
                .Where(bs => bs.SempreDisponibile)
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
            return await GetByIdAsync(articoloId);
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId)
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.DimensioneBicchiereId == dimensioneBicchiereId && bs.SempreDisponibile)
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
                .Where(bs => bs.PersonalizzazioneId == personalizzazioneId && bs.SempreDisponibile)
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

        // ✅ METODI PER CARD PRODOTTO
        public async Task<IEnumerable<BevandaStandardCardDTO>> GetCardProdottiAsync()
        {
            // 1. Recupera solo bevande SEMPRE disponibili
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.SempreDisponibile)
                .ToListAsync();

            var risultati = new List<BevandaStandardCardDTO>();

            foreach (var bevanda in bevandeStandard)
            {
                // 2. Carica dati correlati SEPARATAMENTE (senza Include)
                var personalizzazione = await _context.Personalizzazione
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneId == bevanda.PersonalizzazioneId);

                // 3. Carica ingredienti della personalizzazione
                var ingredienti = await GetIngredientiByPersonalizzazioneAsync(bevanda.PersonalizzazioneId);

                // 4. Calcola prezzi per dimensioni
                var prezziPerDimensioni = await CalcolaPrezziPerDimensioniAsync(bevanda);

                var cardDto = new BevandaStandardCardDTO
                {
                    ArticoloId = bevanda.ArticoloId,
                    Nome = personalizzazione?.Nome ?? "Bevanda Standard",
                    Descrizione = personalizzazione?.Descrizione,
                    ImmagineUrl = bevanda.ImmagineUrl,
                    Disponibile = bevanda.Disponibile,
                    SempreDisponibile = bevanda.SempreDisponibile,
                    Priorita = bevanda.Priorita,
                    PrezziPerDimensioni = prezziPerDimensioni,
                    Ingredienti = ingredienti
                };

                risultati.Add(cardDto);
            }

            return risultati.OrderByDescending(b => b.Priorita).ThenBy(b => b.Nome);
        }

        public async Task<BevandaStandardCardDTO?> GetCardProdottoByIdAsync(int articoloId)
        {
            // 1. Recupera bevanda (solo se SEMPRE disponibile)
            var bevanda = await _context.BevandaStandard
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.ArticoloId == articoloId && bs.SempreDisponibile);

            if (bevanda == null) return null;

            // 2. Carica dati correlati SEPARATAMENTE
            var personalizzazione = await _context.Personalizzazione
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonalizzazioneId == bevanda.PersonalizzazioneId);

            // 3. Carica ingredienti
            var ingredienti = await GetIngredientiByPersonalizzazioneAsync(bevanda.PersonalizzazioneId);

            // 4. Calcola prezzi
            var prezziPerDimensioni = await CalcolaPrezziPerDimensioniAsync(bevanda);

            return new BevandaStandardCardDTO
            {
                ArticoloId = bevanda.ArticoloId,
                Nome = personalizzazione?.Nome ?? "Bevanda Standard",
                Descrizione = personalizzazione?.Descrizione,
                ImmagineUrl = bevanda.ImmagineUrl,
                Disponibile = bevanda.Disponibile,
                SempreDisponibile = bevanda.SempreDisponibile,
                Priorita = bevanda.Priorita,
                PrezziPerDimensioni = prezziPerDimensioni,
                Ingredienti = ingredienti
            };
        }

        // ✅ METODI PRIVATI DI SUPPORTO
        private async Task<List<string>> GetIngredientiByPersonalizzazioneAsync(int personalizzazioneId)
        {
            var ingredienti = await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .Where(pi => pi.PersonalizzazioneId == personalizzazioneId)
                .Join(_context.Ingrediente,
                    pi => pi.IngredienteId,
                    i => i.IngredienteId,
                    (pi, i) => i.Ingrediente1)
                .Where(nome => !string.IsNullOrEmpty(nome))
                .ToListAsync();

            return ingredienti;
        }

        private async Task<List<PrezzoDimensioneDTO>> CalcolaPrezziPerDimensioniAsync(BevandaStandard bevanda)
        {
            var dimensione = await _context.DimensioneBicchiere
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DimensioneBicchiereId == bevanda.DimensioneBicchiereId);

            if (dimensione == null)
                return new List<PrezzoDimensioneDTO>();

            var taxRateId = 1; // IVA standard
            var aliquotaIva = await GetAliquotaIvaAsync(taxRateId);

            // Calcola prezzo con moltiplicatore dimensione
            var prezzoBase = bevanda.Prezzo * dimensione.Moltiplicatore;
            var prezzoIva = CalcolaIva(prezzoBase, aliquotaIva);
            var prezzoTotale = prezzoBase + prezzoIva;

            return new List<PrezzoDimensioneDTO>
            {
                new PrezzoDimensioneDTO
                {
                    DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                    Sigla = dimensione.Sigla,
                    Descrizione = $"{dimensione.Descrizione} {dimensione.Capienza}ml",
                    PrezzoNetto = Math.Round(prezzoBase, 2),
                    PrezzoIva = Math.Round(prezzoIva, 2),
                    PrezzoTotale = Math.Round(prezzoTotale, 2),
                    AliquotaIva = aliquotaIva
                }
            };
        }

        private async Task<decimal> GetAliquotaIvaAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.TaxRateId == taxRateId);

            return taxRate?.Aliquota ?? 22.00m;
        }

        private decimal CalcolaIva(decimal prezzoNetto, decimal aliquotaIva)
        {
            return prezzoNetto * (aliquotaIva / 100);
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetPrimoPianoAsync()
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.Disponibile && bs.SempreDisponibile)
                .OrderByDescending(bs => bs.Priorita) // ORDINA PRIMA del ToListAsync!
                .ThenBy(bs => bs.ArticoloId) // Ordinamento secondario per stabilità
                .ToListAsync();

            var dimensioniBicchieri = await _context.DimensioneBicchiere
                .Where(d => bevandeStandard.Select(bs => bs.DimensioneBicchiereId).Contains(d.DimensioneBicchiereId))
                .ToDictionaryAsync(d => d.DimensioneBicchiereId);

            // RIMUOVI l'OrderByDescending qui sotto, è già stato applicato sopra
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
            .ToList(); // Solo ToList, niente OrderBy qui
        }

        public async Task<IEnumerable<BevandaStandardCardDTO>> GetCardProdottiPrimoPianoAsync()
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => bs.Disponibile && bs.SempreDisponibile) // Primo piano + visibili
                .ToListAsync();

            var risultati = new List<BevandaStandardCardDTO>();

            foreach (var bevanda in bevandeStandard)
            {
                var personalizzazione = await _context.Personalizzazione
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneId == bevanda.PersonalizzazioneId);

                var ingredienti = await GetIngredientiByPersonalizzazioneAsync(bevanda.PersonalizzazioneId);
                var prezziPerDimensioni = await CalcolaPrezziPerDimensioniAsync(bevanda);

                var cardDto = new BevandaStandardCardDTO
                {
                    ArticoloId = bevanda.ArticoloId,
                    Nome = personalizzazione?.Nome ?? "Bevanda Standard",
                    Descrizione = personalizzazione?.Descrizione,
                    ImmagineUrl = bevanda.ImmagineUrl,
                    Disponibile = bevanda.Disponibile,
                    SempreDisponibile = bevanda.SempreDisponibile,
                    Priorita = bevanda.Priorita,
                    PrezziPerDimensioni = prezziPerDimensioni,
                    Ingredienti = ingredienti
                };

                risultati.Add(cardDto);
            }

            return risultati.OrderByDescending(b => b.Priorita).ThenBy(b => b.Nome);
        }

        public async Task<IEnumerable<BevandaStandardDTO>> GetSecondoPianoAsync()
        {
            var bevandeStandard = await _context.BevandaStandard
                .AsNoTracking()
                .Where(bs => !bs.Disponibile && bs.SempreDisponibile) // Disponibile = false (secondo piano)
                .OrderByDescending(bs => bs.Priorita) // Stesso ordinamento del primo piano
                .ThenBy(bs => bs.ArticoloId) // Ordinamento secondario per stabilità
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
            .ToList(); // Solo ToList, niente OrderBy qui (già applicato sopra)
        }
    }
}