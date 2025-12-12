using DTO;

namespace Repository.Interface
{
    public interface ILogAttivitaRepository
    {
        Task<PaginatedResponseDTO<LogAttivitaDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<LogAttivitaDTO>> GetByIdAsync(int logId);
        Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByUtenteIdAsync(int utenteId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoUtenteAsync(string tipoUtente, int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<int>> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
        Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByDateRangeAsync(DateTime dataInizio, DateTime dataFine, int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<Dictionary<string, int>>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
        Task<SingleResponseDTO<LogAttivitaDTO>> AddAsync(LogAttivitaDTO logAttivitaDto);
        Task<SingleResponseDTO<bool>> ExistsAsync(int logId);
        Task<SingleResponseDTO<int>> CleanupOldLogsAsync(int giorniRitenzione = 90);
    }
}
