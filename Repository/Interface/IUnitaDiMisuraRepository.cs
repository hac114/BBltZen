using DTO;

namespace Repository.Interface
{
    public interface IUnitaDiMisuraRepository
    {
        Task<UnitaDiMisuraDTO?> GetByIdAsync(int id);
        Task<List<UnitaDiMisuraDTO>> GetAllAsync();
        Task AddAsync(UnitaDiMisuraDTO unitaDto);
        Task UpdateAsync(UnitaDiMisuraDTO unitaDto);
        Task DeleteAsync(int id);
    }
}