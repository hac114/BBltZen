using DTO;

namespace Repository.Interface
{
    public interface IStatisticheCacheRepository
    {
        Task AddAsync(StatisticheCacheDTO statisticheCacheDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<StatisticheCacheDTO>> GetAllAsync();
        Task<StatisticheCacheDTO?> GetByIdAsync(int id);
        Task<StatisticheCacheDTO?> GetByTipoAndPeriodoAsync(string tipoStatistica, string periodo);
        Task<IEnumerable<StatisticheCacheDTO>> GetByTipoAsync(string tipoStatistica);
        Task UpdateAsync(StatisticheCacheDTO statisticheCacheDto);
        Task AggiornaCacheAsync(string tipoStatistica, string periodo, string metriche);
        Task<bool> IsCacheValidaAsync(string tipoStatistica, string periodo, TimeSpan validita);
    }
}
