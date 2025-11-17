using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IClienteRepository
    {
        Task<IEnumerable<ClienteDTO>> GetAllAsync();
        Task<ClienteDTO?> GetByIdAsync(int id);
        Task<ClienteDTO?> GetByTavoloIdAsync(int tavoloId);
        Task<ClienteDTO> AddAsync(ClienteDTO clienteDto);
        Task UpdateAsync(ClienteDTO clienteDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByTavoloIdAsync(int tavoloId);
    }
}