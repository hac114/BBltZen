using DTO;

namespace Repository.Interface
{
    public interface IOrderItemRepository
    {
        Task AddAsync(OrderItemDTO orderItem);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<OrderItemDTO>> GetAllAsync();
        Task<IEnumerable<OrderItemDTO>> GetByArticoloIdAsync(int articoloId);
        Task<OrderItemDTO> GetByIdAsync(int id);
        Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int ordineId);
        Task UpdateAsync(OrderItemDTO orderItems);
    }
}