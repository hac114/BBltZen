using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IArticoloRepository
    {
        Task<IEnumerable<ArticoloDTO>> GetAllAsync();
        Task<ArticoloDTO?> GetByIdAsync(int articoloId);
        Task<IEnumerable<ArticoloDTO>> GetByTipoAsync(string tipo);
        Task<IEnumerable<ArticoloDTO>> GetArticoliOrdinabiliAsync();
        Task AddAsync(ArticoloDTO articoloDto);
        Task UpdateAsync(ArticoloDTO articoloDto);
        Task DeleteAsync(int articoloId);
        Task<bool> ExistsAsync(int articoloId);
        Task<bool> ExistsByTipoAsync(string tipo, int? excludeArticoloId = null);
        Task<IEnumerable<ArticoloDTO>> GetDolciDisponibiliAsync();
        Task<IEnumerable<ArticoloDTO>> GetBevandeStandardDisponibiliAsync();
        Task<IEnumerable<ArticoloDTO>> GetBevandeCustomBaseAsync();
        Task<IEnumerable<IngredienteDTO>> GetIngredientiDisponibiliPerBevandaCustomAsync();
        Task<IEnumerable<ArticoloDTO>> GetAllArticoliCompletoAsync();
    }
}
