using DTO;

namespace Repository.Interface
{
    public interface IDolceRepository
    {
        Task<PaginatedResponseDTO<DolceDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<DolceDTO>> GetByIdAsync(int articoloId);

        Task<PaginatedResponseDTO<DolceDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<DolceDTO>> GetNonDisponibiliAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<DolceDTO>> GetByPrioritaAsync(int priorita = 1, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<DolceDTO>> AddAsync(DolceDTO dolceDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(DolceDTO dolceDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId);

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountDisponibiliAsync();
        Task<SingleResponseDTO<int>> CountNonDisponibiliAsync();

        Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int articoloId);
    }
}