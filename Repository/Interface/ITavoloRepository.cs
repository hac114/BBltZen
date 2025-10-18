using DTO;

namespace Repository.Interface
{
    public interface ITavoloRepository
    {
        Task AddAsync(TavoloDTO tavoloDto);
        Task DeleteAsync(int tavoloId);
        Task<bool> ExistsAsync(int tavoloId);
        Task<IEnumerable<TavoloDTO>> GetAllAsync();
        Task<TavoloDTO> GetByIdAsync(int tavoloId);
        Task<TavoloDTO> GetByNumeroAsync(int numero);
        Task<IEnumerable<TavoloDTO>> GetByZonaAsync(string zona);
        Task<IEnumerable<TavoloDTO>> GetDisponibiliAsync();
        Task<bool> NumeroExistsAsync(int numero, int? excludeId = null);
        Task UpdateAsync(TavoloDTO tavoloDto);
    }
}