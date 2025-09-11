using DTO;

namespace Repository.Interface
{
    public interface IOrdineRepository
    {
        Task<OrdineDTO> AddAsync(OrdineDTO entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<OrdineDTO>> GetAllAsync();
        Task<OrdineDTO?> GetByIdAsync(int id);
        Task UpdateAsync(OrdineDTO entity);
    }
}