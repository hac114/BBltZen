using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<PersonalizzazioneDTO> AddAsync(PersonalizzazioneDTO personalizzazioneDto);

        Task UpdateAsync(PersonalizzazioneDTO personalizzazioneDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<PersonalizzazioneDTO>> GetAllAsync();
        Task<PersonalizzazioneDTO?> GetByIdAsync(int id);

        // ✅ METODI BUSINESS
        Task<bool> ExistsByNameAsync(string nome, int? excludeId = null);
    }
}