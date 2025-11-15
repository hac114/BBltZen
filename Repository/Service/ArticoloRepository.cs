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
    public class ArticoloRepository : IArticoloRepository
    {
        private readonly BubbleTeaContext _context;

        public ArticoloRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ArticoloDTO>> GetAllAsync()
        {
            return await _context.Articolo
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<ArticoloDTO?> GetByIdAsync(int articoloId)
        {
            var articolo = await _context.Articolo
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            if (articolo == null) return null;

            return new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = articolo.Tipo,
                DataCreazione = articolo.DataCreazione,
                DataAggiornamento = articolo.DataAggiornamento
            };
        }

        public async Task<IEnumerable<ArticoloDTO>> GetByTipoAsync(string tipo)
        {
            return await _context.Articolo
                .AsNoTracking()
                .Where(a => a.Tipo == tipo)
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetArticoliOrdinabiliAsync()
        {
            // ✅ SOLUZIONE COMPATIBILE: USA SINGOLA QUERY CON OR invece di UNION + INCLUDE
            // Elimina il problema InMemory ma mantiene le relazioni per la produzione

            return await _context.Articolo
                .Include(a => a.BevandaStandard)  // ✅ MANTIENI per produzione
                .Include(a => a.BevandaCustom)    // ✅ MANTIENI per produzione  
                .Include(a => a.Dolce)            // ✅ MANTIENI per produzione
                .Where(a =>
                    // ✅ BEVANDE STANDARD - Solo se SempreDisponibile = true
                    (a.Tipo == "BS" && a.BevandaStandard != null && a.BevandaStandard.SempreDisponibile) ||

                    // ✅ BEVANDE CUSTOM - Sempre ordinabili
                    (a.Tipo == "BC" && a.BevandaCustom != null && a.BevandaCustom.Any()) ||

                    // ✅ DOLCI - Solo se disponibili
                    (a.Tipo == "D" && a.Dolce != null && a.Dolce.Disponibile)
                )
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                    // ✅ Aggiungi altri campi necessari per il frontend
                    //Nome = a.Tipo == "BS" ? a.BevandaStandard.Personalizzazione.Nome :
                    //       a.Tipo == "D" ? a.Dolce.Nome :
                    //       "Bevanda Personalizzata",
                    //Prezzo = a.Tipo == "BS" ? a.BevandaStandard.Prezzo :
                    //        a.Tipo == "D" ? a.Dolce.Prezzo :
                    //        a.Tipo == "BC" ? a.BevandaCustom.First().Prezzo : 0m,
                    //Descrizione = a.Tipo == "BS" ? a.BevandaStandard.Personalizzazione.Descrizione :
                    //             a.Tipo == "D" ? a.Dolce.Descrizione :
                    //             "Crea la tua bevanda personalizzata",
                    //ImmagineUrl = a.Tipo == "BS" ? a.BevandaStandard.ImmagineUrl :
                    //             a.Tipo == "D" ? a.Dolce.ImmagineUrl :
                    //             null
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetDolciDisponibiliAsync()
        {
            return await _context.Articolo
                .Include(a => a.Dolce)
                .Where(a => a.Tipo == "D" &&
                           a.Dolce != null &&
                           a.Dolce.Disponibile)
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento,
                    //Nome = a.Dolce.Nome,
                    //Prezzo = a.Dolce.Prezzo,
                    //Descrizione = a.Dolce.Descrizione,
                    //ImmagineUrl = a.Dolce.ImmagineUrl
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetBevandeStandardDisponibiliAsync()
        {
            return await _context.Articolo
                .Include(a => a.BevandaStandard)
                .Where(a => a.Tipo == "BS" &&
                           a.BevandaStandard != null &&
                           a.BevandaStandard.SempreDisponibile)
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento,
                    //Nome = a.BevandaStandard.Personalizzazione.Nome,
                    //Prezzo = a.BevandaStandard.Prezzo,
                    //Descrizione = a.BevandaStandard.Personalizzazione.Descrizione,
                    //PersonalizzazioneId = a.BevandaStandard.PersonalizzazioneId,
                    //ImmagineUrl = a.BevandaStandard.ImmagineUrl,
                    //DimensioneBicchiereId = a.BevandaStandard.DimensioneBicchiereId,
                    //DimensioneBicchiere = a.BevandaStandard.DimensioneBicchiere.Nome // Se serve
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<IngredienteDTO>> GetIngredientiDisponibiliPerBevandaCustomAsync()
        {
            return await _context.Ingrediente
                .Include(i => i.Categoria)
                .Where(i => i.Disponibile)
                .AsNoTracking()
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    Nome = i.Ingrediente1,
                    CategoriaId = i.CategoriaId,
                    PrezzoAggiunto = i.PrezzoAggiunto,
                    Disponibile = i.Disponibile,
                    DataInserimento = i.DataInserimento,
                    DataAggiornamento = i.DataAggiornamento
                    // ✅ Se non hai la proprietà Categoria nel DTO, la omettiamo
                })
                .OrderBy(i => i.CategoriaId)
                .ThenBy(i => i.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetBevandeCustomBaseAsync()
        {
            return await _context.Articolo
                .Include(a => a.BevandaCustom)
                .Where(a => a.Tipo == "BC" &&
                           a.BevandaCustom != null &&
                           a.BevandaCustom.Any())
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento,
                    //Nome = "Bevanda Personalizzata",
                    //Prezzo = a.BevandaCustom.First().Prezzo, // Prezzo base
                    //Descrizione = "Crea la tua bevanda personalizzata"
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetAllArticoliCompletoAsync()
        {
            return await _context.Articolo
                .Where(a => a.BevandaStandard == null || a.BevandaStandard.Personalizzazione != null) // ✅ Filtro sicurezza
                .Include(a => a.BevandaStandard)
                    .ThenInclude(bs => bs.Personalizzazione) // ✅ Ora è sicuro
                .Include(a => a.BevandaCustom)
                .Include(a => a.Dolce)
                .AsNoTracking()
                .Select(a => new ArticoloDTO
                {
                    ArticoloId = a.ArticoloId,
                    Tipo = a.Tipo,
                    DataCreazione = a.DataCreazione,
                    DataAggiornamento = a.DataAggiornamento
                    //Nome = a.Tipo == "BS" ? a.BevandaStandard.Personalizzazione.Nome :
                    //       a.Tipo == "D" ? a.Dolce.Nome :
                    //       "Bevanda Personalizzata",
                    //Prezzo = a.Tipo == "BS" ? a.BevandaStandard.Prezzo :
                    //        a.Tipo == "D" ? a.Dolce.Prezzo :
                    //        a.Tipo == "BC" ? a.BevandaCustom.First().Prezzo : 0m,
                    //Disponibile = a.Tipo == "BS" ? a.BevandaStandard.Disponibile :
                    //             a.Tipo == "D" ? a.Dolce.Disponibile :
                    //             true, // BC sempre disponibile
                    //SempreDisponibile = a.Tipo == "BS" ? a.BevandaStandard.SempreDisponibile : true
                })
                .ToListAsync();
        }

        public async Task<ArticoloDTO> AddAsync(ArticoloDTO articoloDto) // ✅ CORREGGI: ritorna DTO
        {
            if (articoloDto == null)
                throw new ArgumentNullException(nameof(articoloDto));

            var articolo = new Articolo
            {
                Tipo = articoloDto.Tipo,
                DataCreazione = DateTime.Now, // ✅ NOT NULL - valore default
                DataAggiornamento = DateTime.Now // ✅ NOT NULL - valore default
            };

            await _context.Articolo.AddAsync(articolo);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
            articoloDto.ArticoloId = articolo.ArticoloId;
            articoloDto.DataCreazione = articolo.DataCreazione;
            articoloDto.DataAggiornamento = articolo.DataAggiornamento;

            return articoloDto; // ✅ AGGIUNGI return
        }

        public async Task UpdateAsync(ArticoloDTO articoloDto)
        {
            var articolo = await _context.Articolo
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloDto.ArticoloId);

            if (articolo == null)
                throw new ArgumentException($"Articolo con ArticoloId {articoloDto.ArticoloId} non trovato");

            articolo.Tipo = articoloDto.Tipo;
            articolo.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            articoloDto.DataAggiornamento = articolo.DataAggiornamento;
        }

        public async Task DeleteAsync(int articoloId)
        {
            var articolo = await _context.Articolo
                .Include(a => a.BevandaStandard)
                .Include(a => a.Dolce)
                .Include(a => a.BevandaCustom)
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            if (articolo != null)
            {
                // ✅ ELIMINA PRIMA LE RELAZIONI (CASCADE MANUALE)
                if (articolo.BevandaStandard != null)
                    _context.BevandaStandard.Remove(articolo.BevandaStandard);

                if (articolo.Dolce != null)
                    _context.Dolce.Remove(articolo.Dolce);

                if (articolo.BevandaCustom != null && articolo.BevandaCustom.Any())
                    _context.BevandaCustom.RemoveRange(articolo.BevandaCustom);

                _context.Articolo.Remove(articolo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int articoloId)
        {
            return await _context.Articolo
                .AnyAsync(a => a.ArticoloId == articoloId);
        }

        public async Task<bool> ExistsByTipoAsync(string tipo, int? excludeArticoloId = null)
        {
            var query = _context.Articolo.Where(a => a.Tipo == tipo);

            if (excludeArticoloId.HasValue)
            {
                query = query.Where(a => a.ArticoloId != excludeArticoloId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
