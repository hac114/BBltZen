using DTO;

public interface ILogAttivitaRepository
{
    // ✅ METODI DI LETTURA
    Task<LogAttivitaDTO> AddAsync(LogAttivitaDTO logAttivitaDto);
    Task<bool> ExistsAsync(int logId);

    // ✅ METODI PRINCIPALI CON PARAMETRI OPZIONALI (PATTERN)
    Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByIdAsync(int? id = null, int page = 1, int pageSize = 10);
    Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByTipoAttivitaAsync(string? tipoAttivita = null, int page = 1, int pageSize = 10);
    Task<PaginatedResponseDTO<LogAttivitaDTO>> GetByUtenteIdAsync(int? utenteId = null, int page = 1, int pageSize = 10);

    // ✅ METODI FRONTEND CON PARAMETRI OPZIONALI
    Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByTipoAttivitaPerFrontendAsync(string? tipoAttivita = null, int page = 1, int pageSize = 10);
    Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByTipoUtenteAsync(string? tipoUtente = null, int page = 1, int pageSize = 10);
    Task<PaginatedResponseDTO<LogAttivitaFrontendDTO>> GetByPeriodoPerFrontendAsync(DateTime? dataInizio = null, DateTime? dataFine = null, int page = 1, int pageSize = 10);

    // ✅ METODI UTILITY (mantenuti)
    Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    Task<Dictionary<string, int>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    Task<int> CleanupOldLogsAsync(int giorniRitenzione = 90);
}