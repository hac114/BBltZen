using DTO;

namespace Repository.Interface
{
    public interface ILogAttivitaRepository
    {
        Task AddAsync(LogAttivitaDTO logAttivitaDto);
        Task DeleteAsync(int logId);
        Task<bool> ExistsAsync(int logId);
        Task<IEnumerable<LogAttivitaDTO>> GetAllAsync();
        Task<LogAttivitaDTO?> GetByIdAsync(int logId);
        Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita);
        Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
        Task UpdateAsync(LogAttivitaDTO logAttivitaDto);
    }
}