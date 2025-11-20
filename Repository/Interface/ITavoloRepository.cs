using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ITavoloRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<TavoloDTO> AddAsync(TavoloDTO tavoloDto);

        Task UpdateAsync(TavoloDTO tavoloDto);
        Task DeleteAsync(int tavoloId);
        Task<bool> ExistsAsync(int tavoloId);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<TavoloDTO>> GetAllAsync();
        Task<TavoloDTO?> GetByIdAsync(int tavoloId);

        // ✅ METODI BUSINESS SPECIFICI
        Task<TavoloDTO?> GetByNumeroAsync(int numero);
        Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona);
        Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync();
        Task<bool> NumeroExistsAsync(int numero, int? excludeId = null);
    }
}