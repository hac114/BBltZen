using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IOrderTotalServiceRepository
    {
        // Calcolo totali
        Task<OrderTotalDTO> CalculateOrderTotalAsync(int orderId);
        Task<OrderUpdateTotalDTO> UpdateOrderTotalAsync(int orderId);

        // Gestione IVA e tasse
        Task<decimal> CalculateItemTaxAsync(int orderItemId);
        Task<decimal> GetTaxRateAsync(int taxRateId);

        // Utility
        Task<bool> ValidateOrderForCalculationAsync(int orderId);
        Task<decimal> RecalculateOrderTotalFromScratchAsync(int orderId);
        Task<List<int>> GetOrdersWithInvalidTotalsAsync();
    }
}