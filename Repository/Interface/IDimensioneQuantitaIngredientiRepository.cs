using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IDimensioneQuantitaIngredientiRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<DimensioneQuantitaIngredientiDTO> AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto);

        Task UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaDto);
        Task DeleteAsync(int dimensioneId, int personalizzazioneIngredienteId);
        Task<bool> ExistsAsync(int dimensioneId, int personalizzazioneIngredienteId);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetAllAsync();
        Task<DimensioneQuantitaIngredientiDTO?> GetByIdAsync(int dimensioneId, int personalizzazioneIngredienteId);

        // ✅ METODI BUSINESS SPECIFICI
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId);
        Task<IEnumerable<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId);
        Task<DimensioneQuantitaIngredientiDTO?> GetByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId);
        Task<bool> ExistsByCombinazioneAsync(int dimensioneBicchiereId, int personalizzazioneIngredienteId);
    }
}