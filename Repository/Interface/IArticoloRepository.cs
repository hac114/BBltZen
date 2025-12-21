using DTO;

namespace Repository.Interface
{
    public interface IArticoloRepository
    {
        Task<PaginatedResponseDTO<ArticoloDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<ArticoloDTO>> GetByIdAsync(int articoloId);
        Task<PaginatedResponseDTO<ArticoloDTO>> GetByTipoAsync(string tipo, int page = 1, int pageSize = 10);        

        Task<SingleResponseDTO<ArticoloDTO>> AddAsync(ArticoloDTO articoloDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(ArticoloDTO articoloDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId);
        
        Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId);
        Task<SingleResponseDTO<bool>> ExistsByTipoAsync(string descrizioneTipo);        
    }
}
