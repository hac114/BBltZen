using DTO;

namespace Repository.Interface
{
    public interface IDimensioneBicchiereRepository
    {
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<DimensioneBicchiereDTO>> GetByIdAsync(int bicchiereId); 
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetBySiglaAsync(string sigla, int page = 1, int pageSize = 10); 
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10); 

        Task<SingleResponseDTO<DimensioneBicchiereDTO>> AddAsync(DimensioneBicchiereDTO bicchiereDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(DimensioneBicchiereDTO bicchiereDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int bicchiereId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int bicchiereId);
        Task<SingleResponseDTO<bool>> ExistsSiglaAsync(string sigla);
        Task<SingleResponseDTO<bool>> ExistsDescrizioneAsync(string descrizione);


    }
}
