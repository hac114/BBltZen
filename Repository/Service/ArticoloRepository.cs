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

        private ArticoloDTO MapToDTO(Articolo articolo)
        {
            return new ArticoloDTO
            {
                ArticoloId = articolo.ArticoloId,
                Tipo = articolo.Tipo,
                DataCreazione = articolo.DataCreazione,
                DataAggiornamento = articolo.DataAggiornamento
            };
        }

        public async Task<IEnumerable<ArticoloDTO>> GetAllAsync()
        {
            return await _context.Articolo
                .AsNoTracking()
                .Select(a => MapToDTO(a)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<ArticoloDTO?> GetByIdAsync(int articoloId)
        {
            var articolo = await _context.Articolo
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            return articolo != null ? MapToDTO(articolo) : null; // ✅ CHECK NULL + MapToDTO
        }

        public async Task<IEnumerable<ArticoloDTO>> GetByTipoAsync(string tipo)
        {
            return await _context.Articolo
                .AsNoTracking()
                .Where(a => a.Tipo == tipo)
                .Select(a => MapToDTO(a)) // ✅ USA MapToDTO
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetArticoliOrdinabiliAsync()
        {
            return await _context.Articolo
                .Include(a => a.BevandaStandard)
                .Include(a => a.Dolce)
                .Where(a =>
                    (a.Tipo == "BS" && a.BevandaStandard != null) || // ✅ Bevanda Standard sempre disponibile
                    (a.Tipo == "BC" && a.BevandaCustom != null) ||   // ✅ Bevanda Custom sempre disponibile
                    (a.Tipo == "D" && a.Dolce != null && a.Dolce.Disponibile) // ✅ Solo dolci disponibili
                )
                .AsNoTracking()
                .Select(a => MapToDTO(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetDolciDisponibiliAsync()
        {
            return await _context.Articolo
                .Include(a => a.Dolce)
                .Where(a => a.Tipo == "D" && a.Dolce != null && a.Dolce.Disponibile)
                .AsNoTracking()
                .Select(a => MapToDTO(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetBevandeStandardDisponibiliAsync()
        {
            return await _context.Articolo
                .Include(a => a.BevandaStandard)
                .Where(a => a.Tipo == "BS" && a.BevandaStandard != null)
                .AsNoTracking()
                .Select(a => MapToDTO(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<IngredienteDTO>> GetIngredientiDisponibiliPerBevandaCustomAsync()
        {
            return await _context.Ingrediente
                .Include(i => i.Categoria)
                .Where(i => i.Disponibile)
                .AsNoTracking()
                .Select(i => new IngredienteDTO // ✅ CORRETTO: DTO diverso
                {
                    IngredienteId = i.IngredienteId,
                    Nome = i.Ingrediente1,
                    CategoriaId = i.CategoriaId,
                    PrezzoAggiunto = i.PrezzoAggiunto,
                    Disponibile = i.Disponibile,
                    DataInserimento = i.DataInserimento,
                    DataAggiornamento = i.DataAggiornamento
                })
                .OrderBy(i => i.CategoriaId)
                .ThenBy(i => i.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetBevandeCustomBaseAsync()
        {
            return await _context.Articolo
                .Where(a => a.Tipo == "BC" && a.BevandaCustom != null)
                .AsNoTracking()
                .Select(a => MapToDTO(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<ArticoloDTO>> GetAllArticoliCompletoAsync()
        {
            return await _context.Articolo
                .Include(a => a.BevandaStandard)
                .Include(a => a.Dolce)
                .AsNoTracking()
                .Select(a => MapToDTO(a))
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
                .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

            if (articolo != null)
            {
                // ✅ ELIMINA PRIMA LE RELAZIONI (CASCADE MANUALE)
                if (articolo.BevandaStandard != null)
                    _context.BevandaStandard.Remove(articolo.BevandaStandard);

                if (articolo.Dolce != null)
                    _context.Dolce.Remove(articolo.Dolce);

                // ✅ BevandaCustom viene eliminata automaticamente per CASCADE
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
