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
    public class IngredienteRepository : IIngredienteRepository
    {
        private readonly BubbleTeaContext _context;

        public IngredienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        // ✅ GET ALL: Solo per admin - mostra TUTTI gli ingredienti
        public async Task<IEnumerable<IngredienteDTO>> GetAllAsync()
        {
            return await _context.Ingrediente
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    Nome = i.Ingrediente1,
                    CategoriaId = i.CategoriaId,
                    PrezzoAggiunto = i.PrezzoAggiunto,
                    Disponibile = i.Disponibile,  // ✅ Mostra anche quelli non disponibili
                    DataInserimento = i.DataInserimento,
                    DataAggiornamento = i.DataAggiornamento
                })
                .ToListAsync();
        }

        // ✅ GET BY ID: Cerca per ID indipendentemente dalla disponibilità
        public async Task<IngredienteDTO?> GetByIdAsync(int id)
        {
            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(i => i.IngredienteId == id);  // ✅ Rimossa condizione Disponibile

            if (ingrediente == null) return null;

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

        public async Task AddAsync(IngredienteDTO ingredienteDto)
        {
            var ingrediente = new Ingrediente
            {
                Ingrediente1 = ingredienteDto.Nome,
                CategoriaId = ingredienteDto.CategoriaId,
                PrezzoAggiunto = ingredienteDto.PrezzoAggiunto,
                Disponibile = ingredienteDto.Disponibile,  // ✅ USA il valore dal DTO
                DataInserimento = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            ingredienteDto.IngredienteId = ingrediente.IngredienteId;
            ingredienteDto.Disponibile = ingrediente.Disponibile;
            ingredienteDto.DataInserimento = ingrediente.DataInserimento;
            ingredienteDto.DataAggiornamento = ingrediente.DataAggiornamento;
        }

        public async Task UpdateAsync(IngredienteDTO ingredienteDto)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(ingredienteDto.IngredienteId);
            if (ingrediente == null)
                throw new ArgumentException("Ingrediente not found");

            ingrediente.Ingrediente1 = ingredienteDto.Nome;
            ingrediente.CategoriaId = ingredienteDto.CategoriaId;
            ingrediente.PrezzoAggiunto = ingredienteDto.PrezzoAggiunto;
            ingrediente.Disponibile = ingredienteDto.Disponibile;  // ✅ Aggiorna anche disponibilità
            ingrediente.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            ingredienteDto.DataAggiornamento = ingrediente.DataAggiornamento;
        }

        // ✅ NUOVO: HARD DELETE - Eliminazione definitiva
        public async Task DeleteAsync(int id)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente != null)
            {
                // ✅ Controlla se ci sono dipendenze prima di eliminare
                var hasPersonalizzazioni = await _context.PersonalizzazioneIngrediente
                    .AnyAsync(pi => pi.IngredienteId == id);

                if (hasPersonalizzazioni)
                {
                    throw new InvalidOperationException(
                        "Impossibile eliminare l'ingrediente perché è collegato a personalizzazioni."
                    );
                }

                _context.Ingrediente.Remove(ingrediente);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ NUOVO: Toggle disponibilità
        public async Task ToggleDisponibilitaAsync(int id)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente != null)
            {
                ingrediente.Disponibile = !ingrediente.Disponibile;  // ✅ Inverte la disponibilità
                ingrediente.DataAggiornamento = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        // ✅ NUOVO: Imposta disponibilità specifica
        public async Task SetDisponibilitaAsync(int id, bool disponibile)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente != null)
            {
                ingrediente.Disponibile = disponibile;  // ✅ Imposta disponibilità specifica
                ingrediente.DataAggiornamento = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ingrediente.AnyAsync(i => i.IngredienteId == id);  // ✅ Rimossa condizione Disponibile
        }

        public async Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId)
        {
            return await _context.Ingrediente
                .Where(i => i.CategoriaId == categoriaId)
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    Nome = i.Ingrediente1,
                    CategoriaId = i.CategoriaId,
                    PrezzoAggiunto = i.PrezzoAggiunto,
                    Disponibile = i.Disponibile,
                    DataInserimento = i.DataInserimento,
                    DataAggiornamento = i.DataAggiornamento
                })
                .ToListAsync();
        }

        // ✅ GET DISPONIBILI: Solo ingredienti DISPONIBILI
        public async Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync()
        {
            return await _context.Ingrediente
                .Where(i => i.Disponibile)  // ✅ Solo disponibili
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    Nome = i.Ingrediente1,
                    CategoriaId = i.CategoriaId,
                    PrezzoAggiunto = i.PrezzoAggiunto,
                    Disponibile = i.Disponibile,
                    DataInserimento = i.DataInserimento,
                    DataAggiornamento = i.DataAggiornamento
                })
                .ToListAsync();
        }
    }
}