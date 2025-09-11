using DTO;

namespace Repository.Interface
{
    public interface IUtentiRepository
    {
        Task AddAsync(UtentiDTO utenti);
        Task DeleteAsync(int utenteId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ExistsAsync(int utenteId);
        Task<IEnumerable<UtentiDTO>> GetAllAsync();
        Task<IEnumerable<UtentiDTO>> GetAttiviAsync();
        Task<UtentiDTO> GetByEmailAsync(string email);
        Task<UtentiDTO> GetByIdAsync(int utenteId);
        Task<IEnumerable<UtentiDTO>> GetByTipoUtenteAsync(string tipoUtente);
        Task UpdateAsync(UtentiDTO utente);
    }
}