using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneCustomRepository
    {
        Task<IEnumerable<PersonalizzazioneCustomDTO>> GetAllAsync();
        Task<PersonalizzazioneCustomDTO?> GetByIdAsync(int persCustomId);
        Task<IEnumerable<PersonalizzazioneCustomDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId);
        Task<IEnumerable<PersonalizzazioneCustomDTO>> GetByGradoDolcezzaAsync(byte gradoDolcezza);
        Task<PersonalizzazioneCustomDTO> AddAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto);
        Task UpdateAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto);
        Task DeleteAsync(int persCustomId);
        Task<bool> ExistsAsync(int persCustomId);
    }
}