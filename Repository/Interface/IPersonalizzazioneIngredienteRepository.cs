using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneIngredienteRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<PersonalizzazioneIngredienteDTO> AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);

        Task UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetAllAsync();
        Task<PersonalizzazioneIngredienteDTO?> GetByIdAsync(int id);

        // ✅ METODI BUSINESS SPECIFICI
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneIdAsync(int personalizzazioneId);
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByIngredienteIdAsync(int ingredienteId);
        Task<PersonalizzazioneIngredienteDTO?> GetByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId);
        Task<bool> ExistsByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId);
        Task<int> GetCountByPersonalizzazioneAsync(int personalizzazioneId);
    }
}