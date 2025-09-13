using DTO;

namespace Repository.Interface
{
    public interface IVwStatisticheOrdiniAvanzateRepository
    {
        Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetAllAsync();
        Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetByLivelloAllertaAsync(string livelloAllerta);
        Task<VwStatisticheOrdiniAvanzateDTO> GetByOrdineIdAsync(int ordineId);
        Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetOrdiniInRitardoAsync();
    }
}