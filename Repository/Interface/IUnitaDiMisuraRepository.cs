using DTO;

namespace Repository.Interface
{
    public interface IUnitaDiMisuraRepository
    {
        // ✅ METODI ESISTENTI (CRUD)
        Task<UnitaDiMisuraDTO> AddAsync(UnitaDiMisuraDTO unitaDto);
        Task UpdateAsync(UnitaDiMisuraDTO unitaDto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<UnitaDiMisuraDTO?> GetByIdAsync(int id);
        Task<IEnumerable<UnitaDiMisuraDTO>> GetAllAsync();
        Task<bool> SiglaExistsAsync(string sigla);
        Task<bool> SiglaExistsForOtherAsync(int id, string sigla);
        Task<UnitaDiMisuraDTO?> GetBySiglaAsync(string sigla);

        // ✅ NUOVI METODI PER FRONTEND
        Task<IEnumerable<UnitaDiMisuraFrontendDTO>> GetAllPerFrontendAsync();
        Task<UnitaDiMisuraFrontendDTO?> GetBySiglaPerFrontendAsync(string sigla);
    }
}