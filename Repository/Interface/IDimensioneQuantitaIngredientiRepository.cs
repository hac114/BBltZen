using DTO;

namespace Repository.Interface
{
    public interface IDimensioneQuantitaIngredientiRepository
    {
        Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByIdAsync(int dimensioneId);
        Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByBicchiereIdAsync(int bicchiereId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteIdAsync(int personalizzazioneIngredienteId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByBicchiereDescrizioneAsync(string descrizioneBicchiere, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>> AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int dimensioneId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int dimensioneId);
        Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int personalizzazioneIngredienteId, int bicchiereId);

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> GetCountByPersonalizzazioneIngredientiAsync(int personalizzazioneIngredienteId);
    }
}