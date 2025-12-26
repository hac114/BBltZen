using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneIngredienteRepository
    {
        Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<PersonalizzazioneIngredienteDTO>> GetByIdAsync(int personalizzazioneIngredienteId);
        Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneAsync(string nomePersonalizzazione, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetByIngredienteAsync(string ingrediente, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<PersonalizzazioneIngredienteDTO>> AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int personalizzazioneIngredienteId, bool forceDelete = false);

        Task<SingleResponseDTO<bool>> ExistsAsync(int id);
        Task<SingleResponseDTO<bool>> ExistsByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId);

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> GetCountByPersonalizzazioneAsync(string nomePersonalizzazione);
    }
}