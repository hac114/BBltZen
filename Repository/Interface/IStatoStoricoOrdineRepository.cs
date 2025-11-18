using DTO;

namespace Repository.Interface
{
    public interface IStatoStoricoOrdineRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<StatoStoricoOrdineDTO> AddAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto);

        Task UpdateAsync(StatoStoricoOrdineDTO statoStoricoOrdineDto);
        Task DeleteAsync(int statoStoricoOrdineId);
        Task<bool> ExistsAsync(int statoStoricoOrdineId);

        // ✅ CORREGGI: GetAll con IEnumerable
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetAllAsync();
        Task<StatoStoricoOrdineDTO?> GetByIdAsync(int statoStoricoOrdineId);

        // ✅ METODI DI FILTRO
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetByOrdineIdAsync(int ordineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoCompletoOrdineAsync(int ordineId);
        Task<StatoStoricoOrdineDTO?> GetStatoAttualeOrdineAsync(int ordineId);
        Task<bool> ChiudiStatoAttualeAsync(int ordineId, DateTime fine);

        // ✅ AGGIUNGI: Metodi per statistiche e business logic
        Task<bool> OrdineHasStatoAsync(int ordineId, int statoOrdineId);
        Task<DateTime?> GetDataInizioStatoAsync(int ordineId, int statoOrdineId);
        Task<int> GetNumeroStatiByOrdineAsync(int ordineId);
        Task<IEnumerable<StatoStoricoOrdineDTO>> GetStoricoByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
    }
}