using DTO;

namespace Repository.Interface
{
    public interface IOrdineRepository
    {
        Task<OrdineDTO> AddAsync(OrdineDTO entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<OrdineDTO>> GetAllAsync();
        Task<OrdineDTO?> GetByIdAsync(int id);
        Task UpdateAsync(OrdineDTO entity);
        Task<IEnumerable<OrdineDTO>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<OrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId);
        Task<IEnumerable<OrdineDTO>> GetByStatoPagamentoIdAsync(int statoPagamentoId);
        Task<IEnumerable<OrdineDTO>> GetBySessioneIdAsync(Guid sessioneId);
        Task<IEnumerable<OrdineDTO>> GetOrdiniConSessioneAsync();
        Task<IEnumerable<OrdineDTO>> GetOrdiniSenzaSessioneAsync();
    }
}