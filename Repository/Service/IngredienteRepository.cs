using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<IEnumerable<IngredienteDTO>> GetAllAsync()
        {
            return await _context.Ingrediente
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    CategoriaId = i.CategoriaId,
                    Disponibile = i.Disponibile
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IngredienteDTO> GetByIdAsync(int id)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente == null) return null;

            return new IngredienteDTO
            {
                IngredienteId = ingrediente.IngredienteId,
                //Nome = ingrediente.Nome,
                //Prezzo = ingrediente.Prezzo,
                CategoriaId = ingrediente.CategoriaId,
                Disponibile = ingrediente.Disponibile
                // Map other properties as needed
            };
        }

        public async Task AddAsync(IngredienteDTO ingredienteDto)
        {
            var ingrediente = new Ingrediente
            {
                //Nome = ingredienteDto.Nome,
                //Prezzo = ingredienteDto.Prezzo,
                CategoriaId = ingredienteDto.CategoriaId,
                Disponibile = ingredienteDto.Disponibile
                // Map other properties as needed
            };

            await _context.Ingrediente.AddAsync(ingrediente);
            await _context.SaveChangesAsync();

            // Return the generated ID to the DTO
            ingredienteDto.IngredienteId = ingrediente.IngredienteId;
        }

        public async Task UpdateAsync(IngredienteDTO ingredienteDto)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(ingredienteDto.IngredienteId);
            if (ingrediente == null)
                throw new ArgumentException("Ingrediente not found");

            //ingrediente.Nome = ingredienteDto.Nome;
            //ingrediente.Prezzo = ingredienteDto.Prezzo;
            ingrediente.CategoriaId = ingredienteDto.CategoriaId;
            ingrediente.Disponibile = ingredienteDto.Disponibile;
            // Update other properties as needed

            _context.Ingrediente.Update(ingrediente);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente != null)
            {
                _context.Ingrediente.Remove(ingrediente);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ingrediente.AnyAsync(i => i.IngredienteId == id);
        }

        public async Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId)
        {
            return await _context.Ingrediente
                .Where(i => i.CategoriaId == categoriaId)
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    //Nome = i.Nome,
                    //Prezzo = i.Prezzo,
                    CategoriaId = i.CategoriaId,
                    Disponibile = i.Disponibile
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync()
        {
            return await _context.Ingrediente
                .Where(i => i.Disponibile)
                .Select(i => new IngredienteDTO
                {
                    IngredienteId = i.IngredienteId,
                    //Nome = i.Nome,
                    //Prezzo = i.Prezzo,
                    CategoriaId = i.CategoriaId,
                    Disponibile = i.Disponibile
                    // Map other properties as needed
                })
                .ToListAsync();
        }
    }
}
