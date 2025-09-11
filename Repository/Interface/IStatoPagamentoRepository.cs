using DTO;

namespace Repository.Interface
{
    public interface IStatoPagamentoRepository
    {
        Task AddAsync(StatoPagamentoDTO statoPagamentoDto);
        Task DeleteAsync(int statoPagamentoId);
        Task<bool> ExistsAsync(int statoPagamentoId);
        Task<IEnumerable<StatoPagamentoDTO>> GetAllAsync();
        Task<StatoPagamentoDTO> GetByIdAsync(int statoPagamentoId);
        Task<StatoPagamentoDTO> GetByNomeAsync(string nomeStatoPagamento);
        Task UpdateAsync(StatoPagamentoDTO statoPagamentoDto);
    }
}