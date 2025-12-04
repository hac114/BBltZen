using DTO;

namespace Repository.Interface
{
    public interface ICategoriaIngredienteRepository
    {
        // ✅ CRUD BASE
        Task<CategoriaIngredienteDTO> AddAsync(CategoriaIngredienteDTO categoriaDto);
        Task UpdateAsync(CategoriaIngredienteDTO categoriaDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ LETTURE PAGINATE
        Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<CategoriaIngredienteDTO?> GetByIdAsync(int id);

        // ✅ RICERCHE PAGINATE (PARAMETRI OPZIONALI)
        Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetByNomeAsync(string? categoria = null, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<CategoriaIngredienteFrontendDTO>> GetByNomePerFrontendAsync(string? categoria = null, int page = 1, int pageSize = 10);

        // ✅ UTILITY
        Task<bool> ExistsByNomeAsync(string categoria);
        Task<bool> HasDependenciesAsync(int id);
    }
}