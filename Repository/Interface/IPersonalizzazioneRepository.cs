using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneRepository
    {
        Task<IEnumerable<PersonalizzazioneDTO>> GetAllAsync();
        Task<IEnumerable<PersonalizzazioneDTO>> GetAttiveAsync();
        Task<PersonalizzazioneDTO?> GetByIdAsync(int id);
        Task AddAsync(PersonalizzazioneDTO personalizzazioneDto);
        Task UpdateAsync(PersonalizzazioneDTO personalizzazioneDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string nome);
    }
}
