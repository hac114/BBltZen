using DTO;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IIngredienteRepository
    {        
        Task<PaginatedResponseDTO<IngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<IngredienteDTO>> GetByIdAsync(int ingredienteId);
        Task<PaginatedResponseDTO<IngredienteDTO>> GetByNomeAsync(string ingrediente, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<IngredienteDTO>> GetByCategoriaAsync(string categoria, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<IngredienteDTO>> GetByDisponibilisync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<IngredienteDTO>> GetByNonDisponibilisync(int page = 1, int pageSize = 10);


        Task<SingleResponseDTO<IngredienteDTO>> AddAsync(IngredienteDTO ingredienteDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(IngredienteDTO ingredienteDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int ingredienteId);
                
        Task<SingleResponseDTO<bool>> ExistsAsync(int ingredienteId);
        Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string ingrediente);

        Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int ingredienteId);       

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountDisponibiliAsync();
        Task<SingleResponseDTO<int>> CountNonDisponibiliAsync();

    }
}