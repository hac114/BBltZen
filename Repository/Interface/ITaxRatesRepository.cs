using DTO;

namespace Repository.Interface
{
    public interface ITaxRatesRepository
    {
        // ✅ METODI PAGINATI CRUD
        Task<PaginatedResponseDTO<TaxRatesDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<TaxRatesDTO?> GetByIdAsync(int? taxRateId = null);
        Task<PaginatedResponseDTO<TaxRatesDTO>> GetByAliquotaAsync(decimal? aliquota = null, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<TaxRatesDTO> AddAsync(TaxRatesDTO taxRateDto);
        Task UpdateAsync(TaxRatesDTO taxRateDto);
        Task DeleteAsync(int taxRateId);

        // ✅ METODI VERIFICA
        Task<bool> ExistsAsync(int taxRateId);
        Task<bool> ExistsByAliquotaDescrizioneAsync(decimal aliquota, string descrizione, int? excludeId = null);

        // ✅ METODI FRONTEND PAGINATI
        Task<PaginatedResponseDTO<TaxRatesFrontendDTO>> GetAllPerFrontendAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<TaxRatesFrontendDTO>> GetByAliquotaPerFrontendAsync(decimal? aliquota = null, int page = 1, int pageSize = 10);

        // ✅ METODO PER CONTROLLO DIPENDENZE DELETE
        Task<bool> HasDependenciesAsync(int taxRateId);
    }
}