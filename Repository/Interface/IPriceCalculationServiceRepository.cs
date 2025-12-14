using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using BBltZen; // ✅ AGGIUNTO

namespace Repository.Interface
{
    public interface IPriceCalculationServiceRepository
    {
        Task<decimal> CalculateBevandaStandardPrice(int bevandaStandardId);
        Task<decimal> CalculateBevandaCustomPrice(int personalizzazioneCustomId);
        Task<decimal> CalculateDolcePrice(int dolceId);
        Task<PriceCalculationServiceDTO> CalculateOrderItemPrice(OrderItem item);
        Task<decimal> CalculateTaxAmount(decimal imponibile, int taxRateId);
        Task<decimal> CalculateImponibile(decimal prezzo, int quantita, int taxRateId);
        Task<decimal> GetTaxRate(int taxRateId);

        // Metodi per cache
        Task ClearCache();
        //Task PreloadCache();

        // ✅ METODI AGGIUNTIVI PER COMPLETEZZA
        Task<BatchCalculationResponseDTO> CalculateBatchPricesAsync(BatchCalculationRequestDTO request);
        Task<bool> ValidateTaxRate(int taxRateId);
    }
}