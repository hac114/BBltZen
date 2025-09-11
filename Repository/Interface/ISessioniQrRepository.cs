using DTO;

namespace Repository.Interface
{
    public interface ISessioniQrRepository
    {
        Task AddAsync(SessioniQrDTO sessioneQrDto);
        Task DeleteAsync(Guid sessioneId);
        Task<bool> ExistsAsync(Guid sessioneId);
        Task<IEnumerable<SessioniQrDTO>> GetAllAsync();
        Task<IEnumerable<SessioniQrDTO>> GetByClienteIdAsync(int clienteId);
        Task<SessioniQrDTO> GetByIdAsync(Guid sessioneId);
        Task<SessioniQrDTO> GetByQrCodeAsync(string qrCode);
        Task<IEnumerable<SessioniQrDTO>> GetNonutilizzateAsync();
        Task<IEnumerable<SessioniQrDTO>> GetScaduteAsync();
        Task UpdateAsync(SessioniQrDTO sessioneQrDto);
    }
}