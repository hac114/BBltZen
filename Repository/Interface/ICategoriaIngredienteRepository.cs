using DTO;

namespace Repository.Interface
{
    public interface ICategoriaIngredienteRepository
    {
        // ✅ METODI PAGINATI CRUD
        Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<CategoriaIngredienteDTO>> GetByIdAsync(int categoriaId);
        Task<PaginatedResponseDTO<CategoriaIngredienteDTO>> GetByNomeAsync(string categoria, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<SingleResponseDTO<CategoriaIngredienteDTO>> AddAsync(CategoriaIngredienteDTO categoriaDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(CategoriaIngredienteDTO categoriaDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int categoriaId);

        // ✅ METODI VERIFICA        
        Task<SingleResponseDTO<bool>> ExistsAsync(int categoriaId);
        Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string categoria);        
    }
}