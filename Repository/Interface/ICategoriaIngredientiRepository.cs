using DTO;

namespace Repository.Interface
{
    public interface ICategoriaIngredienteRepository
    {
        Task<CategoriaIngredienteDTO?> GetByIdAsync(int id);
        Task<List<CategoriaIngredienteDTO>> GetAllAsync();
        Task AddAsync(CategoriaIngredienteDTO categoriaDto);
        Task UpdateAsync(CategoriaIngredienteDTO categoriaDto);
        Task DeleteAsync(int id);
    }
}