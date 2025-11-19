using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IIngredienteRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<IngredienteDTO> AddAsync(IngredienteDTO ingredienteDto);

        Task UpdateAsync(IngredienteDTO ingredienteDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<IngredienteDTO>> GetAllAsync();
        Task<IngredienteDTO?> GetByIdAsync(int id);

        // ✅ METODI BUSINESS SPECIFICI
        Task<IEnumerable<IngredienteDTO>> GetByCategoriaAsync(int categoriaId);
        Task<IEnumerable<IngredienteDTO>> GetDisponibiliAsync();
        Task ToggleDisponibilitaAsync(int id);
        Task SetDisponibilitaAsync(int id, bool disponibile);
    }
}