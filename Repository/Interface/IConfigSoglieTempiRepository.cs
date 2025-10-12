using DTO;

namespace Repository.Interface
{
    public interface IConfigSoglieTempiRepository
    {
        Task AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto);
        Task DeleteAsync(int sogliaId);
        Task<bool> ExistsAsync(int sogliaId);
        Task<IEnumerable<ConfigSoglieTempiDTO>> GetAllAsync();
        Task<ConfigSoglieTempiDTO?> GetByIdAsync(int sogliaId);
        Task<ConfigSoglieTempiDTO?> GetByStatoOrdineIdAsync(int statoOrdineId);
        Task UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto);
        Task<bool> ExistsByStatoOrdineIdAsync(int statoOrdineId, int? excludeSogliaId = null);
    }
}