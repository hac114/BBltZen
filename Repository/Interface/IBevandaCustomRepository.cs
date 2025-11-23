using Database;
using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBevandaCustomRepository
    {
        Task<IEnumerable<BevandaCustomDTO>> GetAllAsync();
        Task<BevandaCustomDTO?> GetByIdAsync(int id);           // ✅ "id" generico = ArticoloId
        Task<BevandaCustomDTO?> GetByArticoloIdAsync(int articoloId);
        Task<IEnumerable<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId);
        Task<BevandaCustomDTO> AddAsync(BevandaCustomDTO dto);
        Task UpdateAsync(BevandaCustomDTO dto);
        Task DeleteAsync(int id);                               // ✅ "id" generico = ArticoloId
        Task<bool> ExistsAsync(int id);                         // ✅ "id" generico = ArticoloId
        Task<bool> ExistsByArticoloIdAsync(int articoloId);
        Task<bool> ExistsByPersCustomIdAsync(int persCustomId);
    }
}