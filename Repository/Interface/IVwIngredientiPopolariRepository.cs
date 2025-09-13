using DTO;

namespace Repository.Interface
{
    public interface IVwIngredientiPopolariRepository
    {
        Task<IEnumerable<VwIngredientiPopolariDTO>> GetAllAsync();
        Task<IEnumerable<VwIngredientiPopolariDTO>> GetByCategoriaAsync(string categoria);
        Task<VwIngredientiPopolariDTO> GetByIngredienteIdAsync(int ingredienteId);
        Task<IEnumerable<VwIngredientiPopolariDTO>> GetTopNAsync(int topN);
    }
}