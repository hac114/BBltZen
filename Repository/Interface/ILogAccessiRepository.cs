using DTO;

namespace Repository.Interface
{
    public interface ILogAccessiRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<LogAccessiDTO> AddAsync(LogAccessiDTO logAccessiDto);

        Task UpdateAsync(LogAccessiDTO logAccessiDto);
        Task DeleteAsync(int logId);
        Task<bool> ExistsAsync(int logId);

        // ✅ CORREGGI: GetAll con IEnumerable
        Task<IEnumerable<LogAccessiDTO>> GetAllAsync();
        Task<LogAccessiDTO?> GetByIdAsync(int logId);

        // ✅ METODI DI FILTRO
        Task<IEnumerable<LogAccessiDTO>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<LogAccessiDTO>> GetByEsitoAsync(string esito);
        Task<IEnumerable<LogAccessiDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<LogAccessiDTO>> GetByTipoAccessoAsync(string tipoAccesso);
        Task<IEnumerable<LogAccessiDTO>> GetByUtenteIdAsync(int utenteId);
        Task<int> GetNumeroAccessiAsync(DateTime? dataInizio = null, DateTime? dataFine = null);

        // ✅ AGGIUNGI: Metodo per statistiche
        Task<Dictionary<string, int>> GetStatisticheAccessiAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    }
}