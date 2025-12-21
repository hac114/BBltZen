using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPersonalizzazioneRepository
    {
        Task<PaginatedResponseDTO<PersonalizzazioneDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<PersonalizzazioneDTO>> GetByIdAsync(int personalizzazioneId);
        Task<PaginatedResponseDTO<PersonalizzazioneDTO>> GetByNomeAsync(string nome, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<PersonalizzazioneDTO>> AddAsync(PersonalizzazioneDTO personalizzazioneDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneDTO personalizzazioneDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int personalizzazioneId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int personalizzazioneId);        
        Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string nome);
    }
}