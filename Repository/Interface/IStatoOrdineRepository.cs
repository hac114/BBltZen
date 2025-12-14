using DTO;

namespace Repository.Interface
{
    public interface IStatoOrdineRepository
    {
        Task<PaginatedResponseDTO<StatoOrdineDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<StatoOrdineDTO>> GetByIdAsync(int statoOrdineId);
        Task<SingleResponseDTO<StatoOrdineDTO>> GetByNomeAsync(string nomeStatoOrdine);
        Task<PaginatedResponseDTO<StatoOrdineDTO>> GetStatiNonTerminaliAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<StatoOrdineDTO>> GetStatiTerminaliAsync(int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<StatoOrdineDTO>> AddAsync(StatoOrdineDTO statoOrdineDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(StatoOrdineDTO statoOrdineDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int statoOrdineId);
                
        Task<SingleResponseDTO<bool>> ExistsAsync(int statoOrdineId);
        Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string statoOrdine);        
    }
}