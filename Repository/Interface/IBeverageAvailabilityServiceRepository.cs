using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBeverageAvailabilityServiceRepository
    {
        // ✅ VERIFICA DISPONIBILITÀ - ALLINEATO
        Task<BeverageAvailabilityDTO> CheckBeverageAvailabilityAsync(int articoloId);
        Task<IEnumerable<BeverageAvailabilityDTO>> CheckMultipleBeveragesAvailabilityAsync(List<int> articoliIds);
        Task<bool> IsBeverageAvailableAsync(int articoloId);

        // ✅ AGGIORNAMENTO DISPONIBILITÀ - ALLINEATO  
        Task<AvailabilityUpdateDTO> UpdateBeverageAvailabilityAsync(int articoloId);
        Task<IEnumerable<AvailabilityUpdateDTO>> UpdateAllBeveragesAvailabilityAsync();
        Task ForceBeverageAvailabilityAsync(int articoloId, bool disponibile, string? motivo = null);

        // ✅ GESTIONE MENU - ALLINEATO
        Task<MenuAvailabilityDTO> GetMenuAvailabilityStatusAsync();
        Task<IEnumerable<BeverageAvailabilityDTO>> GetAvailableBeveragesForPrimoPianoAsync(int numeroElementi = 6);
        Task<IEnumerable<BeverageAvailabilityDTO>> FindSostitutiPrimoPianoAsync(int numeroRichieste = 3);

        // ✅ MONITORAGGIO INGREDIENTI - ALLINEATO
        Task<IEnumerable<IngredienteMancanteDTO>> GetIngredientiCriticiAsync();
        Task<int> GetCountBeveragesWithLowStockAsync();

        // ✅ UTILITY - ALLINEATO
        Task<bool> CanBeverageBeInPrimoPianoAsync(int articoloId);
        Task<IEnumerable<int>> GetBeveragesAffectedByIngredientAsync(int ingredienteId);

        // ✅ AGGIUNTO PER COMPLETEZZA PATTERN
        Task<bool> ExistsAsync(int articoloId);
    }
}