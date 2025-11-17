using Database;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBevandaCustomRepository
    {
        Task<IEnumerable<BevandaCustomDTO>> GetAllAsync();
        Task<BevandaCustomDTO?> GetByIdAsync(int bevandaCustomId);
        Task<BevandaCustomDTO?> GetByArticoloIdAsync(int articoloId);
        Task<IEnumerable<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId);
        Task<BevandaCustomDTO> AddAsync(BevandaCustomDTO bevandaCustomDto);
        Task UpdateAsync(BevandaCustomDTO bevandaCustomDto);
        Task DeleteAsync(int bevandaCustomId);
        Task<bool> ExistsAsync(int bevandaCustomId);
        Task<bool> ExistsByArticoloIdAsync(int articoloId);
        Task<bool> ExistsByPersCustomIdAsync(int persCustomId);

    }
}
