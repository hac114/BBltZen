using DTO;

namespace Repository.Interface
{
    public interface INotificheOperativeRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<NotificheOperativeDTO> AddAsync(NotificheOperativeDTO notificaDto);

        Task UpdateAsync(NotificheOperativeDTO notificaDto);
        Task DeleteAsync(int notificaId);
        Task<bool> ExistsAsync(int notificaId);

        // ✅ CORREGGI: GetAll con IEnumerable
        Task<IEnumerable<NotificheOperativeDTO>> GetAllAsync();
        Task<NotificheOperativeDTO?> GetByIdAsync(int notificaId);

        // ✅ METODI DI FILTRO
        Task<IEnumerable<NotificheOperativeDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<NotificheOperativeDTO>> GetByPrioritaAsync(int priorita);
        Task<IEnumerable<NotificheOperativeDTO>> GetByStatoAsync(string stato);
        Task<IEnumerable<NotificheOperativeDTO>> GetPendentiAsync();
        Task<int> GetNumeroNotifichePendentiAsync();

        // ✅ AGGIUNGI: Metodi per statistiche e filtri aggiuntivi
        Task<IEnumerable<NotificheOperativeDTO>> GetByTipoNotificaAsync(string tipoNotifica);
        Task<Dictionary<string, int>> GetStatisticheNotificheAsync();
        Task<int> GetNumeroNotificheByStatoAsync(string stato);
    }
}