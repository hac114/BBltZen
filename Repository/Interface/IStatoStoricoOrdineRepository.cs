using DTO;

namespace Repository.Interface
{
    public interface IStatoStoricoOrdineRepository
    {
        Task AddAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto);
        Task DeleteAsync(int statoStoricoOrdineId);
        Task<bool> ExistsAsync(int statoStoricoOrdineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetAllAsync();
        Task<StatoStoricoOrdineDTO?> GetByIdAsync(int statoStoricoOrdineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetByOrdineIdAsync(int ordineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoCompletoOrdineAsync(int ordineId);
        Task<StatoStoricoOrdineDTO?> GetStatoAttualeOrdineAsync(int ordineId);
        Task UpdateAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto);
        Task<bool> ChiudiStatoAttualeAsync(int ordineId, DateTime fine);
    }
}
