using DTO;

public interface ILogAttivitaRepository
{
    // ✅ METODI DI LETTURA
    Task<LogAttivitaDTO> AddAsync(LogAttivitaDTO logAttivitaDto);

    // ❌ RIMUOVI: Task UpdateAsync(LogAttivitaDTO logAttivitaDto);
    
    // ❌ RIMUOVI: Task DeleteAsync(int logId);
    Task<bool> ExistsAsync(int logId);
    Task<IEnumerable<LogAttivitaDTO>> GetAllAsync();
    Task<LogAttivitaDTO?> GetByIdAsync(int logId);

    // ✅ METODI DI FILTRO (restano)
    Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
    Task<IEnumerable<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita);
    Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    Task<IEnumerable<LogAttivitaDTO>> GetByUtenteIdAsync(int utenteId);
    Task<Dictionary<string, int>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    
    // ✅ NUOVI METODI PER FRONTEND
    Task<IEnumerable<LogAttivitaFrontendDTO>> GetAllPerFrontendAsync();
    Task<IEnumerable<LogAttivitaFrontendDTO>> GetByPeriodoPerFrontendAsync(DateTime dataInizio, DateTime dataFine);
    Task<IEnumerable<LogAttivitaFrontendDTO>> GetByTipoAttivitaPerFrontendAsync(string tipoAttivita);

    // ✅ AGGIUNGI CLEANUP
    Task<int> CleanupOldLogsAsync(int giorniRitenzione = 90);

    // ✅ RICERCHE INTELLIGENTI
    Task<IEnumerable<LogAttivitaFrontendDTO>> SearchIntelligenteAsync(string searchTerm);
    Task<IEnumerable<LogAttivitaFrontendDTO>> GetByTipoAttivitaIntelligenteAsync(string tipoAttivita);
}