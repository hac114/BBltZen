using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IVwMenuDinamicoRepository
    {
        // ✅ ALLINEATO AL PATTERN - SOLO LETTURA (vista)
        Task<IEnumerable<VwMenuDinamicoDTO>> GetMenuCompletoAsync();
        Task<IEnumerable<VwMenuDinamicoDTO>> GetPrimoPianoAsync(int numeroElementi = 6);
        Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandeDisponibiliAsync();
        Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandePerCategoriaAsync(string categoria);
        Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandePerPrioritaAsync(int prioritaMinima, int prioritaMassima);
        Task<IEnumerable<VwMenuDinamicoDTO>> GetBevandeConScontoAsync();
        Task<VwMenuDinamicoDTO?> GetBevandaByIdAsync(int id, string tipo);
        Task<IEnumerable<string>> GetCategorieDisponibiliAsync();
        Task<IEnumerable<VwMenuDinamicoDTO>> SearchBevandeAsync(string searchTerm);
        Task<int> GetCountBevandeDisponibiliAsync();

        // ✅ AGGIUNTO PER COMPLETEZZA PATTERN
        Task<bool> ExistsAsync(int id, string tipo);
    }
}