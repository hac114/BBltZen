using DTO;

namespace Repository.Interface
{
    public interface IDimensioneBicchiereRepository
    {
        // ✅ CRUD già esistenti
        Task<DimensioneBicchiereDTO> AddAsync(DimensioneBicchiereDTO bicchiereDto); //OK
        Task UpdateAsync(DimensioneBicchiereDTO bicchiereDto); //OK
        Task<bool> DeleteAsync(int bicchiereid); //OK      
        
        Task<DimensioneBicchiereDTO?> GetByIdAsync(int bicchiereId); //OK
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetAllAsync(int page = 1, int pageSize = 10);


        Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetBySiglaAsync(string? sigla, int page = 1, int pageSize = 10); //OK
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetByDescrizioneAsync(string? descrizione, int page = 1, int pageSize = 10); //OK

        // ✅ NUOVI METODI FRONTEND
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetFrontendByIdAsync(int? bicchiereId = null, int page = 1, int pageSize = 10); //OK
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO?>> GetFrontendBySiglaAsync(string? sigla, int page = 1, int pageSize = 10); //OK
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetFrontendByDescrizioneAsync(string? descrizione, int page = 1, int pageSize = 10); //OK
        Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetFrontendAsync(string? sigla, string? descrizione, decimal? capienza, decimal? prezzoBase, decimal? moltiplicatore, int page, int pageSize); //OK

    }
}
