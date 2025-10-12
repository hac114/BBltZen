using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IIngredientiPersonalizzazioneRepository
    {
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetAllAsync();
        Task<IngredientiPersonalizzazioneDTO?> GetByIdAsync(int ingredientePersId);
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId);
        Task<IEnumerable<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId);
        Task<IngredientiPersonalizzazioneDTO?> GetByCombinazioneAsync(int persCustomId, int ingredienteId);
        Task AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);
        Task UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);
        Task DeleteAsync(int ingredientePersId);
        Task<bool> ExistsAsync(int ingredientePersId);
        Task<bool> ExistsByCombinazioneAsync(int persCustomId, int ingredienteId);
    }
}