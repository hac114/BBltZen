using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class IngredienteRepository : IIngredienteRepository
    {
        private readonly BubbleTeaContext _context;

        public IngredienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        // ✅ METODO PRIVATO PER MAPPING (PATTERN STANDARD)
        private IngredienteDTO MapToDTO(Ingrediente ingrediente)
        {
            return new IngredienteDTO
            {
                IngredienteId = ingrediente.IngredienteId,
                Nome = ingrediente.Ingrediente1,
                CategoriaId = ingrediente.CategoriaId,
                PrezzoAggiunto = ingrediente.PrezzoAggiunto,
                Disponibile = ingrediente.Disponibile,
                DataInserimento = ingrediente.DataInserimento,
                DataAggiornamento = ingrediente.DataAggiornamento
            };
        }

        // ✅ GET ALL: Solo per admin - mostra TUTTI gli ingredienti
        public async Task<IEnumerable<IngredienteDTO>> GetAllAsync()
        {
            return await _context.Ingrediente
                .AsNoTracking()
                .OrderBy(i => i.Ingrediente1)
                .Select(i => MapToDTO(i))
                .ToListAsync();
        }

        // ✅ GET BY ID: Cerca per ID indipendentemente dalla disponibilità
        public async Task<IngredienteDTO?> GetByIdAsync(int id)
        {
            var ingrediente = await _context.Ingrediente
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.IngredienteId == id);

            return ingrediente == null ? null : MapToDTO(ingrediente);
        }

        // ✅ CORRETTO: AddAsync ritorna DTO con ID aggiornato
        public async Task<IngredienteDTO> AddAsync(IngredienteDTO ingredienteDto)
        {
            if (ingredienteDto == null)
                throw new ArgumentNullException(nameof(ingredienteDto));

            // ✅ VERIFICA UNICITÀ NOME
            if (await _context.Ingrediente.AnyAsync(i => i.Ingrediente1 == ingredienteDto.Nome))
                throw new ArgumentException($"Esiste già un ingrediente con nome '{ingredienteDto.Nome}'");

            var ingrediente = new Ingrediente
            {
                Ingrediente1 = ingredienteDto.Nome,
                CategoriaId = ingredienteDto.CategoriaId,
                PrezzoAggiunto = ingredienteDto.PrezzoAggiunto,
                Disponibile = ingredienteDto.Disponibile,
                DataInserimento = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO E RITORNALO
            ingredienteDto.IngredienteId = ingrediente.IngredienteId;
            ingredienteDto.DataInserimento = ingrediente.DataInserimento;
            ingredienteDto.DataAggiornamento = ingrediente.DataAggiornamento;

            return ingredienteDto;
        }

        public async Task UpdateAsync(IngredienteDTO ingredienteDto)
        {
            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == ingredienteDto.IngredienteId);

            if (ingrediente == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ NOME (escludendo corrente)
            if (await _context.Ingrediente.AnyAsync(i =>
                i.Ingrediente1 == ingredienteDto.Nome &&
                i.IngredienteId != ingredienteDto.IngredienteId))
            {
                throw new ArgumentException($"Esiste già un altro ingrediente con nome '{ingredienteDto.Nome}'");
            }

            ingrediente.Ingrediente1 = ingredienteDto.Nome;
            ingrediente.CategoriaId = ingredienteDto.CategoriaId;
            ingrediente.PrezzoAggiunto = ingredienteDto.PrezzoAggiunto;
            ingrediente.Disponibile = ingredienteDto.Disponibile;
            ingrediente.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        // ✅ HARD DELETE con controllo vincoli referenziali
        public async Task DeleteAsync(int id)
        {
            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == id);

            if (ingrediente != null)
            {
                // ✅ CONTROLLO VINCOLI REFERENZIALI
                bool hasPersonalizzazioni = await _context.PersonalizzazioneIngrediente
                    .AnyAsync(pi => pi.IngredienteId == id);

                if (hasPersonalizzazioni)
                {
                    throw new InvalidOperationException(
                        "Impossibile eliminare l'ingrediente perché è utilizzato in personalizzazioni esistenti."
                    );
                }

                _context.Ingrediente.Remove(ingrediente);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ TOGGLE DISPONIBILITÀ
        public async Task ToggleDisponibilitaAsync(int id)
        {
            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == id);

            if (ingrediente != null)
            {
                ingrediente.Disponibile = !ingrediente.Disponibile;
                ingrediente.DataAggiornamento = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ IMPOSTA DISPONIBILITÀ SPECIFICA
        public async Task SetDisponibilitaAsync(int id, bool disponibile)
        {
            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == id);

            if (ingrediente != null)
            {
                ingrediente.Disponibile = disponibile;
                ingrediente.DataAggiornamento = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ingrediente
                .AnyAsync(i => i.IngredienteId == id);
        }

        // ✅ GET BY CATEGORIA
        public async Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId)
        {
            return await _context.Ingrediente
                .AsNoTracking()
                .Where(i => i.CategoriaId == categoriaId)
                .OrderBy(i => i.Ingrediente1)
                .Select(i => MapToDTO(i))
                .ToListAsync();
        }

        // ✅ GET DISPONIBILI: Solo ingredienti DISPONIBILI
        public async Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync()
        {
            return await _context.Ingrediente
                .AsNoTracking()
                .Where(i => i.Disponibile)
                .OrderBy(i => i.Ingrediente1)
                .Select(i => MapToDTO(i))
                .ToListAsync();
        }

        // ✅ METODO AGGIUNTIVO: Verifica esistenza per nome
        public async Task<bool> ExistsByNomeAsync(string nome, int? excludeId = null)
        {
            var query = _context.Ingrediente
                .Where(i => i.Ingrediente1 == nome);

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.IngredienteId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}