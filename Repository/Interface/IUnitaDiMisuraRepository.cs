using DTO;

namespace Repository.Interface
{
    public interface IUnitaDiMisuraRepository
    {
        // ✅ METODI PAGINATI CRUD
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<UnitaDiMisuraDTO>> GetByIdAsync(int unitaId);
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetBySiglaAsync(string sigla, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<SingleResponseDTO<UnitaDiMisuraDTO>> AddAsync(UnitaDiMisuraDTO unitaDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(UnitaDiMisuraDTO unitaDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int unitaDiMisuraId);

        // ✅ METODI VERIFICA
        Task<SingleResponseDTO<bool>> ExistsAsync(int unitaDiMisuraId);
        Task<SingleResponseDTO<bool>> SiglaExistsAsync(string sigla);
        Task<SingleResponseDTO<bool>> DescrizioneExistsAsync(string descrizione);
                
    }
}