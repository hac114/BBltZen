using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneIngredienteRepository
    {
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetAllAsync();
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneIdAsync(int personalizzazioneId);
        Task<IEnumerable<PersonalizzazioneIngredienteDTO>> GetByIngredienteIdAsync(int ingredienteId);
        Task<PersonalizzazioneIngredienteDTO?> GetByIdAsync(int id);
        Task<PersonalizzazioneIngredienteDTO?> GetByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId);
        Task AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);
        Task UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);
        Task DeleteAsync(int id);        
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId);
        Task<int> GetCountByPersonalizzazioneAsync(int personalizzazioneId);
    }
}