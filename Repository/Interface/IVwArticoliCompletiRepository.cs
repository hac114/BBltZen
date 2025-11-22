using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IVwArticoliCompletiRepository
    {
        // ✅ ALLINEATO AL PATTERN - SOLO LETTURA (vista)
        Task<IEnumerable<VwArticoliCompletiDTO>> GetAllAsync();
        Task<VwArticoliCompletiDTO?> GetByIdAsync(int articoloId);
        Task<bool> ExistsAsync(int articoloId);

        // ✅ METODI SPECIFICI PER LA VISTA
        Task<IEnumerable<VwArticoliCompletiDTO>> GetByTipoAsync(string tipoArticolo);
        Task<IEnumerable<VwArticoliCompletiDTO>> GetByCategoriaAsync(string categoria);
        Task<IEnumerable<VwArticoliCompletiDTO>> GetDisponibiliAsync();
        Task<IEnumerable<VwArticoliCompletiDTO>> SearchByNameAsync(string nome);
        Task<IEnumerable<VwArticoliCompletiDTO>> GetByPriceRangeAsync(decimal prezzoMin, decimal prezzoMax);
        Task<IEnumerable<VwArticoliCompletiDTO>> GetArticoliConIvaAsync();

        // ✅ METODI AGGREGATI
        Task<int> GetCountAsync();
        Task<IEnumerable<string>> GetCategorieAsync();
        Task<IEnumerable<string>> GetTipiArticoloAsync();
    }
}