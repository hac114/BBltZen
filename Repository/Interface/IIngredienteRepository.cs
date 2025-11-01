using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IIngredienteRepository
    {
        Task AddAsync(IngredienteDTO ingredienteDto);
        Task DeleteAsync(int id); // ✅ CAMBIATO: Ora è HARD DELETE
        Task ToggleDisponibilitaAsync(int id); // ✅ NUOVO: Toggle disponibilità
        Task SetDisponibilitaAsync(int id, bool disponibile); // ✅ NUOVO: Imposta disponibilità
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<IngredienteDTO>> GetAllAsync(); // ✅ Tutti (admin)
        Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId);
        Task<IngredienteDTO?> GetByIdAsync(int id); // ✅ Qualsiasi ingrediente
        Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync(); // ✅ Solo disponibili
        Task UpdateAsync(IngredienteDTO ingredienteDto);
    }
}