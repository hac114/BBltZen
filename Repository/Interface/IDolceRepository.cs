using DTO;

namespace Repository.Interface
{
    public interface IDolceRepository
    {
        Task<DolceDTO> AddAsync(DolceDTO entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<DolceDTO>> GetAllAsync();
        Task<DolceDTO?> GetByIdAsync(int id);
        Task<IEnumerable<DolceDTO>> GetByPrioritaAsync(int priorita);
        Task<IEnumerable<DolceDTO>> GetDisponibiliAsync();
        Task<bool> ToggleDisponibilitaAsync(int id, bool disponibile);
        Task UpdateAsync(DolceDTO entity);
    }
}