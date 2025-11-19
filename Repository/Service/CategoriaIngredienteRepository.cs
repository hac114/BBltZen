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

        private CategoriaIngredienteDTO MapToDTO(CategoriaIngrediente categoria)
        {
            return new CategoriaIngredienteDTO
            {
                CategoriaId = categoria.CategoriaId,
                Categoria = categoria.Categoria
            };
        }

        public async Task<CategoriaIngredienteDTO?> GetByIdAsync(int id)
        {
            var categoria = await _context.CategoriaIngrediente
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            return categoria == null ? null : MapToDTO(categoria);
        }

        public async Task<IEnumerable<CategoriaIngredienteDTO>> GetAllAsync()
        {
            return await _context.CategoriaIngrediente
                .AsNoTracking()
                .OrderBy(c => c.Categoria)
                .Select(c => MapToDTO(c))
                .ToListAsync();
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.CategoriaIngrediente
                .AnyAsync(c => c.CategoriaId == id);
        }

        public async Task<bool> ExistsByNomeAsync(string categoria, int? excludeId = null)
        {
            var query = _context.CategoriaIngrediente
                .Where(c => c.Categoria == categoria);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoriaId != excludeId.Value);
            }

            return await query.AnyAsync();
        }    

        public async Task<CategoriaIngredienteDTO> AddAsync(CategoriaIngredienteDTO categoriaDto)
        {
            if (categoriaDto == null)
                throw new ArgumentNullException(nameof(categoriaDto));

            // ✅ VERIFICA UNICITÀ NOME
            if (await ExistsByNomeAsync(categoriaDto.Categoria))
                throw new ArgumentException($"Esiste già una categoria con nome '{categoriaDto.Categoria}'");

            var categoria = new CategoriaIngrediente
            {
                Categoria = categoriaDto.Categoria
            };

            _context.CategoriaIngrediente.Add(categoria);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO
            categoriaDto.CategoriaId = categoria.CategoriaId;
            return categoriaDto; // ✅ RITORNA DTO
        }

        public async Task UpdateAsync(CategoriaIngredienteDTO categoriaDto)
        {
            var categoria = await _context.CategoriaIngrediente
                .FirstOrDefaultAsync(c => c.CategoriaId == categoriaDto.CategoriaId);

            if (categoria == null)
                return; // ✅ SILENT FAIL

            // ✅ VERIFICA UNICITÀ NOME (escludendo corrente)
            if (await ExistsByNomeAsync(categoriaDto.Categoria, categoriaDto.CategoriaId))
                throw new ArgumentException($"Esiste già un'altra categoria con nome '{categoriaDto.Categoria}'");

            categoria.Categoria = categoriaDto.Categoria;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var categoria = await _context.CategoriaIngrediente
                .FirstOrDefaultAsync(c => c.CategoriaId == id);

            if (categoria != null)
            {
                _context.CategoriaIngrediente.Remove(categoria);
                await _context.SaveChangesAsync();
            }
        }
    }
}