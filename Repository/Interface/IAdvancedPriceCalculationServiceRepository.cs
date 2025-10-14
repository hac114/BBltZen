using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IAdvancedPriceCalculationServiceRepository
    {
        // Calcoli Prezzi Base
        Task<decimal> CalculateBevandaStandardPriceAsync(int articoloId);
        Task<decimal> CalculateBevandaCustomPriceAsync(int personalizzazioneCustomId);
        Task<decimal> CalculateDolcePriceAsync(int articoloId);

        // Calcoli IVA e Fiscali
        Task<decimal> CalculateTaxAmountAsync(decimal importo, int taxRateId);
        Task<decimal> CalculateImponibileAsync(decimal importoIvato, int taxRateId);
        Task<decimal> GetTaxRateAsync(int taxRateId);

        // Calcoli Complessi
        Task<PriceCalculationResultDTO> CalculateCompletePriceAsync(PriceCalculationRequestDTO request);
        Task<CustomBeverageCalculationDTO> CalculateDetailedCustomBeveragePriceAsync(int personalizzazioneCustomId);
        Task<OrderCalculationSummaryDTO> CalculateCompleteOrderAsync(int ordineId);

        // Calcoli Batch
        Task<List<PriceCalculationResultDTO>> CalculateBatchPricesAsync(List<PriceCalculationRequestDTO> requests);
        Task<Dictionary<int, decimal>> CalculateOrderItemsTotalAsync(List<int> orderItemIds);

        // Utility e Supporto
        Task<bool> ValidatePriceCalculationAsync(int articoloId, string tipoArticolo, decimal prezzoCalcolato);
        Task<decimal> ApplyDiscountAsync(decimal prezzo, decimal percentualeSconto);
        Task<decimal> CalculateShippingCostAsync(decimal subtotal, string metodoSpedizione);

        // Cache e Performance
        Task PreloadCalculationCacheAsync();
        Task ClearCalculationCacheAsync();
        Task<bool> IsCacheValidAsync();
    }
}