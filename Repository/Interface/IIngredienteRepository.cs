using DTO;

namespace Repository.Interface
{
    public interface IIngredienteRepository
    {
        Task AddAsync(IngredienteDTO ingredienteDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<IngredienteDTO>> GetAllAsync();
        Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId);
        Task<IngredienteDTO?> GetByIdAsync(int id);
        Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync();
        Task UpdateAsync(IngredienteDTO ingredienteDto);
    }
}