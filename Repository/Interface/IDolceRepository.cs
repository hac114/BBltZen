using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IDolceRepository
    {
        Task<IEnumerable<DolceDTO>> GetAllAsync();
        Task<DolceDTO?> GetByIdAsync(int articoloId);
        Task<IEnumerable<DolceDTO>> GetDisponibiliAsync();
        Task<IEnumerable<DolceDTO>> GetByPrioritaAsync(int priorita);
        Task AddAsync(DolceDTO dolceDto);
        Task UpdateAsync(DolceDTO dolceDto);
        Task DeleteAsync(int articoloId);
        Task<bool> ExistsAsync(int articoloId);
        Task<bool> ExistsByArticoloIdAsync(int articoloId);
    }
}