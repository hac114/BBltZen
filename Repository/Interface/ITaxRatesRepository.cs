using DTO;

namespace Repository.Interface
{
    public interface ITaxRatesRepository
    {
        // ✅ METODI ESISTENTI...
        Task<TaxRatesDTO> AddAsync(TaxRatesDTO taxRateDto);
        Task DeleteAsync(int taxRateId);
        Task<bool> ExistsAsync(int taxRateId);
        Task<IEnumerable<TaxRatesDTO>> GetAllAsync();
        Task<TaxRatesDTO?> GetByAliquotaAsync(decimal aliquota);
        Task<TaxRatesDTO?> GetByIdAsync(int taxRateId);
        Task UpdateAsync(TaxRatesDTO taxRateDto);
        Task<bool> ExistsByAliquotaAsync(decimal aliquota, int? excludeTaxRateId = null);

        // ✅ NUOVI METODI PER FRONTEND
        Task<IEnumerable<TaxRatesFrontendDTO>> GetAllPerFrontendAsync();
        Task<TaxRatesFrontendDTO?> GetByAliquotaPerFrontendAsync(decimal aliquota);
    }
}