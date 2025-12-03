using DTO;

namespace Repository.Interface
{
    public interface IUnitaDiMisuraRepository
    {
        // ✅ METODI CRUD BASE
        Task<UnitaDiMisuraDTO> AddAsync(UnitaDiMisuraDTO unitaDto);
        Task UpdateAsync(UnitaDiMisuraDTO unitaDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ LETTURE PAGINATE (MODIFICATE PER NUOVE SPECIFICHE)
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<UnitaDiMisuraDTO?> GetByIdAsync(int id);

        // ✅ RICERCHE PER SIGLA (PARAMETRO OPZIONALE, PAGINATE)
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetBySiglaAsync(string? sigla = null, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>> GetBySiglaPerFrontendAsync(string? sigla = null, int page = 1, int pageSize = 10);

        // ✅ NUOVE RICERCHE PER DESCRIZIONE (PARAMETRO OPZIONALE, PAGINATE)
        Task<PaginatedResponseDTO<UnitaDiMisuraDTO>> GetByDescrizioneAsync(string? descrizione = null, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<UnitaDiMisuraFrontendDTO>> GetByDescrizionePerFrontendAsync(string? descrizione = null, int page = 1, int pageSize = 10);

        // ✅ CONTROLLI UNIVOCITÀ
        Task<bool> SiglaExistsAsync(string sigla);
        Task<bool> SiglaExistsForOtherAsync(int id, string sigla);
        Task<bool> DescrizioneExistsAsync(string descrizione);
        Task<bool> DescrizioneExistsForOtherAsync(int id, string descrizione);

        // ✅ CONTROLLO DIPENDENZE PER DELETE
        Task<bool> HasDependenciesAsync(int id);
    }
}