using DTO;

namespace Repository.Interface
{
    public interface ILogAttivitaRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<LogAttivitaDTO> AddAsync(LogAttivitaDTO logAttivitaDto);

        Task UpdateAsync(LogAttivitaDTO logAttivitaDto);
        Task DeleteAsync(int logId);
        Task<bool> ExistsAsync(int logId);

        // ✅ CORREGGI: GetAll con IEnumerable
        Task<IEnumerable<LogAttivitaDTO>> GetAllAsync();
        Task<LogAttivitaDTO?> GetByIdAsync(int logId);

        // ✅ METODI DI FILTRO
        Task<IEnumerable<LogAttivitaDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<LogAttivitaDTO>> GetByTipoAttivitaAsync(string tipoAttivita);
        Task<int> GetNumeroAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);

        // ✅ AGGIUNGI: Metodo per statistiche e filtri aggiuntivi
        Task<IEnumerable<LogAttivitaDTO>> GetByUtenteIdAsync(int utenteId);
        Task<Dictionary<string, int>> GetStatisticheAttivitaAsync(DateTime? dataInizio = null, DateTime? dataFine = null);
    }
}