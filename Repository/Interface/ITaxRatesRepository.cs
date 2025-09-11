using DTO;

namespace Repository.Interface
{
    public interface ITaxRatesRepository
    {
        Task AddAsync(TaxRatesDTO taxRateDto);
        Task DeleteAsync(int taxRateId);
        Task<bool> ExistsAsync(int taxRateId);
        Task<IEnumerable<TaxRatesDTO>> GetAllAsync();
        Task<TaxRatesDTO> GetByAliquotaAsync(decimal aliquota);
        Task<TaxRatesDTO> GetByIdAsync(int taxRateId);
        Task UpdateAsync(TaxRatesDTO taxRateDto);
    }
}