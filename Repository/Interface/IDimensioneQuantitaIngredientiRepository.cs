using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IDimensioneQuantitaIngredientiRepository
    {
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetAllAsync();
        Task<DimensioneQuantitaIngredientiDTO?> GetByIdAsync(int dimensioneId, int personalizzazioneIngredienteId);
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId);
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId);
        Task<DimensioneQuantitaIngredientiDTO?> GetByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId);
        Task AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto);
        Task UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto);
        Task DeleteAsync(int dimensioneId, int personalizzazioneIngredienteId);
        Task<bool> ExistsAsync(int dimensioneId, int personalizzazioneIngredienteId);
        Task<bool> ExistsByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId);
    }
}
