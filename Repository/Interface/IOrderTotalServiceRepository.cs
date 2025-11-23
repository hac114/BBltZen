using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IOrderTotalServiceRepository
    {
        // ✅ CALCOLO TOTALI - ALLINEATO
        Task<OrderTotalDTO> CalculateOrderTotalAsync(int orderId);
        Task<OrderUpdateTotalDTO> UpdateOrderTotalAsync(int orderId);

        // ✅ GESTIONE IVA E TASSE - ALLINEATO
        Task<decimal> CalculateItemTaxAsync(int orderItemId);
        Task<decimal> GetTaxRateAsync(int taxRateId);

        // ✅ UTILITY - ALLINEATO
        Task<bool> ValidateOrderForCalculationAsync(int orderId);
        Task<decimal> RecalculateOrderTotalFromScratchAsync(int orderId);
        Task<IEnumerable<int>> GetOrdersWithInvalidTotalsAsync();

        // ✅ AGGIUNTO PER COMPLETEZZA PATTERN
        Task<bool> ExistsAsync(int orderId);
    }
}