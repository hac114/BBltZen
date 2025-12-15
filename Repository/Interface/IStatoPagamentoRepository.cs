using DTO;

namespace Repository.Interface
{
    public interface IStatoPagamentoRepository
    {        
        Task<PaginatedResponseDTO<StatoPagamentoDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<StatoPagamentoDTO>> GetByIdAsync(int statoPagamentoId);
        Task<SingleResponseDTO<StatoPagamentoDTO>> GetByNomeAsync(string nomeStatoPagamento);

        Task<SingleResponseDTO<StatoPagamentoDTO>> AddAsync(StatoPagamentoDTO statoPagamentoDto);
        Task<SingleResponseDTO<bool>> UpdateAsync (StatoPagamentoDTO statoPagamentoDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int statoPagamentoId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int statoPagamentoId);
        Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string statoPagamento);
    }
}