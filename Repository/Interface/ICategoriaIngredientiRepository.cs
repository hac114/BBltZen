using DTO;

namespace Repository.Interface
{
    public interface ICategoriaIngredienteRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<CategoriaIngredienteDTO> AddAsync(CategoriaIngredienteDTO categoriaDto);

        Task UpdateAsync(CategoriaIngredienteDTO categoriaDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<CategoriaIngredienteDTO>> GetAllAsync();
        Task<CategoriaIngredienteDTO?> GetByIdAsync(int id);

        // ✅ AGGIUNGI: Metodo per verificare esistenza per nome
        Task<bool> ExistsByNomeAsync(string categoria);

        // ✅ AGGIUNGI METODI FRONTEND
        Task<IEnumerable<CategoriaIngredienteFrontendDTO>> GetAllPerFrontendAsync();
        Task<CategoriaIngredienteFrontendDTO?> GetByNomePerFrontendAsync(string categoria);
    }
}