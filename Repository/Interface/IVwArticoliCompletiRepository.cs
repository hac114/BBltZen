using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IVwArticoliCompletiRepository
    {
        Task<List<VwArticoliCompletiDTO>> GetAllAsync();
        Task<VwArticoliCompletiDTO?> GetByIdAsync(int articoloId);
        Task<List<VwArticoliCompletiDTO>> GetByTipoAsync(string tipoArticolo);
        Task<List<VwArticoliCompletiDTO>> GetByCategoriaAsync(string categoria);
        Task<List<VwArticoliCompletiDTO>> GetDisponibiliAsync();
        Task<List<VwArticoliCompletiDTO>> SearchByNameAsync(string nome);
        Task<List<VwArticoliCompletiDTO>> GetByPriceRangeAsync(decimal prezzoMin, decimal prezzoMax);
        Task<List<VwArticoliCompletiDTO>> GetArticoliConIvaAsync();
        Task<int> GetCountAsync();
        Task<List<string>> GetCategorieAsync();
        Task<List<string>> GetTipiArticoloAsync();
    }
}