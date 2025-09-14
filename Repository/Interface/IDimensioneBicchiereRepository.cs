using DTO;

namespace Repository.Interface
{
    public interface IDimensioneBicchiereRepository
    {
        Task<DimensioneBicchiereDTO> GetByIdAsync(int id);
        Task<List<DimensioneBicchiereDTO>> GetAllAsync();
        Task AddAsync(DimensioneBicchiereDTO dimensione);
        Task UpdateAsync(DimensioneBicchiereDTO dimensione);
        Task DeleteAsync(int id);
    }
}