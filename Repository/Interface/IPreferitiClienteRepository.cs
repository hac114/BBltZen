using DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IPreferitiClienteRepository
    {
        Task<IEnumerable<PreferitiClienteDTO>> GetAllAsync();
        Task<PreferitiClienteDTO?> GetByIdAsync(int id);
        Task<IEnumerable<PreferitiClienteDTO>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<PreferitiClienteDTO>> GetByBevandaIdAsync(int bevandaId);
        Task<PreferitiClienteDTO?> GetByClienteAndBevandaAsync(int clienteId, int bevandaId);
        Task AddAsync(PreferitiClienteDTO preferitoDto);
        Task UpdateAsync(PreferitiClienteDTO preferitoDto);
        Task DeleteAsync(int id);
        Task DeleteByClienteAndBevandaAsync(int clienteId, int bevandaId);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByClienteAndBevandaAsync(int clienteId, int bevandaId);
        Task<int> GetCountByClienteAsync(int clienteId);
    }
}