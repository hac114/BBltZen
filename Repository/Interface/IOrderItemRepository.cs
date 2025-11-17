using DTO;

namespace Repository.Interface
{
    public interface IOrderItemRepository
    {
        Task<OrderItemDTO> AddAsync(OrderItemDTO orderItem); // ✅ CAMBIATO: ritorna DTO
        Task UpdateAsync(OrderItemDTO orderItems);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<OrderItemDTO>> GetAllAsync();
        Task<IEnumerable<OrderItemDTO>> GetByArticoloIdAsync(int articoloId);
        Task<OrderItemDTO?> GetByIdAsync(int id); // ✅ CAMBIATO: nullable (corregge CS8613)
        Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int ordineId);
    }
}