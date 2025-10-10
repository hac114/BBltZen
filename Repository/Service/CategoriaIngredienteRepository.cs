using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Service
{
    public class CategoriaIngredienteRepository : ICategoriaIngredienteRepository
    {
        private readonly BubbleTeaContext _context;

        public CategoriaIngredienteRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<CategoriaIngredienteDTO?> GetByIdAsync(int id)
        {
            return await _context.CategoriaIngrediente
                .Where(c => c.CategoriaId == id)
                .Select(c => new CategoriaIngredienteDTO
                {
                    CategoriaId = c.CategoriaId,
                    Categoria = c.Categoria
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CategoriaIngredienteDTO>> GetAllAsync()
        {
            return await _context.CategoriaIngrediente
                .Select(c => new CategoriaIngredienteDTO
                {
                    CategoriaId = c.CategoriaId,
                    Categoria = c.Categoria
                })
                .ToListAsync();
        }

        public async Task AddAsync(CategoriaIngredienteDTO categoriaDto)
        {
            var categoria = new CategoriaIngrediente
            {
                Categoria = categoriaDto.Categoria
            };

            _context.CategoriaIngrediente.Add(categoria);
            await _context.SaveChangesAsync();

            categoriaDto.CategoriaId = categoria.CategoriaId;
        }

        public async Task UpdateAsync(CategoriaIngredienteDTO categoriaDto)
        {
            var categoria = await _context.CategoriaIngrediente
                .FindAsync(categoriaDto.CategoriaId);

            if (categoria != null)
            {
                categoria.Categoria = categoriaDto.Categoria;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var categoria = await _context.CategoriaIngrediente.FindAsync(id);
            if (categoria != null)
            {
                _context.CategoriaIngrediente.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }
    }
}