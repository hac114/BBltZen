using DTO;

namespace Repository.Interface
{
    public interface IDimensioneBicchiereRepository
    {
        Task<DimensioneBicchiereDTO> AddAsync(DimensioneBicchiereDTO dimensione); // ✅ CAMBIATO: ritorna DTO
        Task UpdateAsync(DimensioneBicchiereDTO dimensione);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id); // ✅ AGGIUNTO: metodo mancante
        Task<IEnumerable<DimensioneBicchiereDTO>> GetAllAsync(); // ✅ CAMBIATO: IEnumerable invece di List
        Task<DimensioneBicchiereDTO?> GetByIdAsync(int id); // ✅ GIÀ CORRETTO: nullable
        Task<bool> SiglaExistsAsync(string sigla);
        Task<bool> SiglaExistsForOtherAsync(int id, string sigla);
        Task<DimensioneBicchiereDTO?> GetBySiglaAsync(string sigla);
    }
}