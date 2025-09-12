using DTO;

namespace Repository.Service
{
    public interface INotificheOperativeRepository
    {
        Task AddAsync(NotificheOperativeDTO notificaDto);
        Task DeleteAsync(int notificaId);
        Task<bool> ExistsAsync(int notificaId);
        Task<IEnumerable<NotificheOperativeDTO>> GetAllAsync();
        Task<NotificheOperativeDTO> GetByIdAsync(int notificaId);
        Task<IEnumerable<NotificheOperativeDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine);
        Task<IEnumerable<NotificheOperativeDTO>> GetByPrioritaAsync(int priorita);
        Task<IEnumerable<NotificheOperativeDTO>> GetByStatoAsync(string stato);
        Task<IEnumerable<NotificheOperativeDTO>> GetPendentiAsync();
        Task UpdateAsync(NotificheOperativeDTO notificaDto);
    }
}