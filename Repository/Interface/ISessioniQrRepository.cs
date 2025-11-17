using DTO;

namespace Repository.Interface
{
    public interface ISessioniQrRepository
    {
        Task<SessioniQrDTO> AddAsync(SessioniQrDTO sessioneQrDto); // ✅ CAMBIATO: ritorna DTO
        Task UpdateAsync(SessioniQrDTO sessioneQrDto);
        Task DeleteAsync(Guid sessioneId);
        Task<bool> ExistsAsync(Guid sessioneId);
        Task<IEnumerable<SessioniQrDTO>> GetAllAsync();
        Task<IEnumerable<SessioniQrDTO>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<SessioniQrDTO>> GetByTavoloIdAsync(int tavoloId);
        Task<SessioniQrDTO?> GetByIdAsync(Guid sessioneId); // ✅ CAMBIATO: nullable
        Task<SessioniQrDTO?> GetByQrCodeAsync(string qrCode); // ✅ CAMBIATO: nullable
        Task<SessioniQrDTO?> GetByCodiceSessioneAsync(string codiceSessione); // ✅ CAMBIATO: nullable
        Task<IEnumerable<SessioniQrDTO>> GetNonutilizzateAsync();
        Task<IEnumerable<SessioniQrDTO>> GetScaduteAsync();
        Task<SessioniQrDTO> GeneraSessioneQrAsync(int tavoloId, string frontendUrl);
    }
}