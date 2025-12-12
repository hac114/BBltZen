using DTO;

namespace Repository.Interface
{
    public interface ITaxRatesRepository
    {
        // ✅ METODI PAGINATI CRUD
        Task<PaginatedResponseDTO<TaxRatesDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<TaxRatesDTO>> GetByIdAsync(int taxRateId);
        Task<PaginatedResponseDTO<TaxRatesDTO>> GetByAliquotaAsync(decimal aliquota, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<TaxRatesDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<SingleResponseDTO<TaxRatesDTO>> AddAsync(TaxRatesDTO taxRateDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(TaxRatesDTO taxRateDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int taxRateId);

        // ✅ METODI VERIFICA
        Task<SingleResponseDTO<bool>> ExistsAsync(int taxRateId);
        Task<SingleResponseDTO<bool>> ExistsByAliquotaAsync(decimal aliquota);
        Task<SingleResponseDTO<bool>> ExistsByAliquotaDescrizioneAsync(decimal aliquota, string descrizione);        
    }
}