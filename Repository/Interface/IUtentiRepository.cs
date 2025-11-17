using DTO;

namespace Repository.Interface
{
    public interface IUtentiRepository
    {
        Task<UtentiDTO> AddAsync(UtentiDTO utenti); // ✅ CAMBIATO: ritorna DTO
        Task UpdateAsync(UtentiDTO utente);
        Task DeleteAsync(int utenteId);
        Task<bool> ExistsAsync(int utenteId);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<UtentiDTO>> GetAllAsync();
        Task<IEnumerable<UtentiDTO>> GetAttiviAsync();
        Task<UtentiDTO?> GetByEmailAsync(string email); // ✅ CAMBIATO: nullable
        Task<UtentiDTO?> GetByIdAsync(int utenteId); // ✅ CAMBIATO: nullable
        Task<IEnumerable<UtentiDTO>> GetByTipoUtenteAsync(string tipoUtente);
    }
}