using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface ITavoloRepository
    {
        // ✅ METODI ESISTENTI CRUD...
        Task<TavoloDTO> AddAsync(TavoloDTO tavoloDto);
        Task UpdateAsync(TavoloDTO tavoloDto);
        Task DeleteAsync(int tavoloId);
        Task<bool> ExistsAsync(int tavoloId);
        Task<IEnumerable<TavoloDTO>> GetAllAsync();
        Task<TavoloDTO?> GetByIdAsync(int tavoloId);
        Task<TavoloDTO?> GetByNumeroAsync(int numero);
        Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona);
        Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync();
        Task<bool> NumeroExistsAsync(int numero, int? excludeId = null);

        // ✅ NUOVI METODI PER FRONTEND
        Task<IEnumerable<TavoloFrontendDTO>> GetAllPerFrontendAsync();
        Task<IEnumerable<TavoloFrontendDTO>> GetDisponibiliPerFrontendAsync();
        Task<IEnumerable<TavoloFrontendDTO>> GetByZonaPerFrontendAsync(string zona);
        Task<TavoloFrontendDTO?> GetByNumeroPerFrontendAsync(int numero);

        // ✅ METODI BUSINESS        
        Task<bool> ToggleDisponibilitaAsync(int tavoloId);
        Task<bool> ToggleDisponibilitaByNumeroAsync(int numero);
    }
}