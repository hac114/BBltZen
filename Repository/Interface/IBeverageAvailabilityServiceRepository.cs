using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IBeverageAvailabilityServiceRepository
    {
        // Verifica disponibilità
        Task<BeverageAvailabilityDTO> CheckBeverageAvailabilityAsync(int articoloId);
        Task<List<BeverageAvailabilityDTO>> CheckMultipleBeveragesAvailabilityAsync(List<int> articoliIds);
        Task<bool> IsBeverageAvailableAsync(int articoloId);

        // Aggiornamento disponibilità
        Task<AvailabilityUpdateDTO> UpdateBeverageAvailabilityAsync(int articoloId);
        Task<List<AvailabilityUpdateDTO>> UpdateAllBeveragesAvailabilityAsync();
        Task ForceBeverageAvailabilityAsync(int articoloId, bool disponibile, string? motivo = null);

        // Gestione menu dinamico
        Task<MenuAvailabilityDTO> GetMenuAvailabilityStatusAsync();
        Task<List<BeverageAvailabilityDTO>> GetAvailableBeveragesForPrimoPianoAsync(int numeroElementi = 6);
        Task<List<BeverageAvailabilityDTO>> FindSostitutiPrimoPianoAsync(int numeroRichieste = 3);

        // Monitoraggio ingredienti
        Task<List<IngredienteMancanteDTO>> GetIngredientiCriticiAsync();
        Task<int> GetCountBeveragesWithLowStockAsync();

        // Utility
        Task<bool> CanBeverageBeInPrimoPianoAsync(int articoloId);
        Task<List<int>> GetBeveragesAffectedByIngredientAsync(int ingredienteId);
    }
}