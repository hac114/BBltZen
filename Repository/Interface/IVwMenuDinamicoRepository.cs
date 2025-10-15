using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IVwMenuDinamicoRepository
    {
        Task<List<VwMenuDinamicoDTO>> GetMenuCompletoAsync();
        Task<List<VwMenuDinamicoDTO>> GetPrimoPianoAsync(int numeroElementi = 6);
        Task<List<VwMenuDinamicoDTO>> GetBevandeDisponibiliAsync();
        Task<List<VwMenuDinamicoDTO>> GetBevandePerCategoriaAsync(string categoria);
        Task<List<VwMenuDinamicoDTO>> GetBevandePerPrioritaAsync(int prioritaMinima, int prioritaMassima);
        Task<List<VwMenuDinamicoDTO>> GetBevandeConScontoAsync();
        Task<VwMenuDinamicoDTO?> GetBevandaByIdAsync(int id, string tipo);
        Task<List<string>> GetCategorieDisponibiliAsync();
        Task<List<VwMenuDinamicoDTO>> SearchBevandeAsync(string searchTerm);
        Task<int> GetCountBevandeDisponibiliAsync();
    }
}