using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBevandaStandardRepository
    {
        Task<IEnumerable<BevandaStandardDTO>> GetAllAsync();
        Task<IEnumerable<BevandaStandardDTO>> GetDisponibiliAsync();
        Task<BevandaStandardDTO?> GetByIdAsync(int articoloId);
        Task<BevandaStandardDTO?> GetByArticoloIdAsync(int articoloId);
        Task<IEnumerable<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId);
        Task<IEnumerable<BevandaStandardDTO>> GetByPersonalizzazioneAsync(int personalizzazioneId);
        Task AddAsync(BevandaStandardDTO bevandaStandardDto);
        Task UpdateAsync(BevandaStandardDTO bevandaStandardDto);
        Task DeleteAsync(int articoloId);
        Task<bool> ExistsAsync(int articoloId);
        Task<bool> ExistsByCombinazioneAsync(int personalizzazioneId, int dimensioneBicchiereId);
    }
}