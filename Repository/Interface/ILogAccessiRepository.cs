using DTO;

namespace Repository.Interface
{
    public interface ILogAccessiRepository
    {
        Task AddAsync(LogAccessiDTO logAccessiDto);
        Task DeleteAsync(int logId);
        Task<bool> ExistsAsync(int logId);
        Task<IEnumerable<LogAccessiDTO>> GetAllAsync();
        Task<LogAccessiDTO?> GetByIdAsync(int logId);
        Task<IEnumerable<LogAccessiDTO>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<LogAccessiDTO>> GetByEsitoAsync(string esito);
        Task<IEnumerable<LogAccessiDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<LogAccessiDTO>> GetByTipoAccessoAsync(string tipoAccesso);
        Task<IEnumerable<LogAccessiDTO>> GetByUtenteIdAsync(int utenteId);
        Task<int> GetNumeroAccessiAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
        Task UpdateAsync(LogAccessiDTO logAccessiDto);
    }
}