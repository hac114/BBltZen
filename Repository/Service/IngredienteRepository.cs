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

        public async Task<IEnumerable<IngredienteDTO>> GetAllAsync()
        {
            return await _context.Ingrediente
                .Where(i => i.Disponibile)  // 👈 SOFT-DELETE FILTER
                .Include(i => i.Categoria)
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

        public async Task<IngredienteDTO?> GetByIdAsync(int id)
        {
            // PRIMA verifica se esiste e è disponibile
            var ingrediente = await _context.Ingrediente
                .Where(i => i.IngredienteId == id && i.Disponibile)
                .FirstOrDefaultAsync();

            if (ingrediente == null) return null;

            // POI carica la categoria separatamente se necessario
            await _context.Entry(ingrediente)
                .Reference(i => i.Categoria)
                .LoadAsync();

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
                Disponibile = ingredienteDto.Disponibile,
                DataInserimento = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            _context.Ingrediente.Add(ingrediente);
            await _context.SaveChangesAsync();

            ingredienteDto.IngredienteId = ingrediente.IngredienteId;
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
            ingrediente.Disponibile = ingredienteDto.Disponibile;
            ingrediente.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            ingredienteDto.DataAggiornamento = ingrediente.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente != null)
            {
                ingrediente.Disponibile = false;  // 👈 SOFT DELETE
                ingrediente.DataAggiornamento = DateTime.Now;
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
                .Include(i => i.Categoria)
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

        public async Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync()
        {
            return await _context.Ingrediente
                .Where(i => i.Disponibile)
                .Include(i => i.Categoria)
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