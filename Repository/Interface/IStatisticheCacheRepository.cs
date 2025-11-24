using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IStatisticheCacheRepository
    {
        // ✅ CORRETTO: AddAsync deve ritornare DTO
        Task<StatisticheCacheDTO> AddAsync(StatisticheCacheDTO statisticheCacheDto);

        Task UpdateAsync(StatisticheCacheDTO statisticheCacheDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // ✅ CORRETTO: GetAll con IEnumerable
        Task<IEnumerable<StatisticheCacheDTO>> GetAllAsync();
        Task<StatisticheCacheDTO?> GetByIdAsync(int id);

        // ✅ METODI BUSINESS SPECIFICI
        Task<StatisticheCacheDTO?> GetByTipoAndPeriodoAsync(string tipoStatistica, string periodo);
        Task<IEnumerable<StatisticheCacheDTO>> GetByTipoAsync(string tipoStatistica);
        Task AggiornaCacheAsync(string tipoStatistica, string periodo, string metriche);
        Task<bool> IsCacheValidaAsync(string tipoStatistica, string periodo, TimeSpan validita);

        // ✅ NUOVI METODI PER STATISTICHE CARRELLO
        Task<StatisticheCarrelloDTO?> GetStatisticheCarrelloByPeriodoAsync(string periodo);
        Task SalvaStatisticheCarrelloAsync(string periodo, StatisticheCarrelloDTO statistiche);
        Task<bool> IsStatisticheCarrelloValideAsync(string periodo, TimeSpan validita);
        Task<IEnumerable<string>> GetPeriodiDisponibiliCarrelloAsync();
    }
}