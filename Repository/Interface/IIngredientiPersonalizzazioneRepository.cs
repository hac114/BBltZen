using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IIngredientiPersonalizzazioneRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<IngredientiPersonalizzazioneDTO> AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);

        Task UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);
        Task DeleteAsync(int ingredientePersId);
        Task<bool> ExistsAsync(int ingredientePersId);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetAllAsync();
        Task<IngredientiPersonalizzazioneDTO?> GetByIdAsync(int ingredientePersId);

        // ✅ METODI BUSINESS SPECIFICI
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId);
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId);
        Task<IngredientiPersonalizzazioneDTO?> GetByCombinazioneAsync(int persCustomId, int ingredienteId);
        Task<bool> ExistsByCombinazioneAsync(int persCustomId, int ingredienteId);
    }
}