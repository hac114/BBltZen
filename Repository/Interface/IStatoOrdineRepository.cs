using DTO;

namespace Repository.Interface
{
    public interface IStatoOrdineRepository
    {
        Task AddAsync(StatoOrdineDTO statoOrdineDto);
        Task DeleteAsync(int statoOrdineId);
        Task<bool> ExistsAsync(int statoOrdineId);
        Task<IEnumerable<StatoOrdineDTO>> GetAllAsync();
        Task<StatoOrdineDTO> GetByIdAsync(int statoOrdineId);
        Task<StatoOrdineDTO> GetByNomeAsync(string nomeStatoOrdine);
        Task<IEnumerable<StatoOrdineDTO>> GetStatiNonTerminaliAsync();
        Task<IEnumerable<StatoOrdineDTO>> GetStatiTerminaliAsync();
        Task UpdateAsync(StatoOrdineDTO statoOrdineDto);
    }
}