using DTO;

namespace Repository.Interface
{
    public interface IIngredientiPersonalizzazioneRepository
    {
        Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> GetByIdAsync(int ingredientePersId);
        Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetByPersCustomIdAsync(int persCustomId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<IngredientiPersonalizzazioneDTO>> GetByIngredienteIdAsync(int ingredienteId, int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> GetByCombinazioneAsync(int persCustomId, int ingredienteId);

        Task<SingleResponseDTO<IngredientiPersonalizzazioneDTO>> AddAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(IngredientiPersonalizzazioneDTO ingredientiPersDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int ingredientePersId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int ingredientePersId);        
        Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int persCustomId, int ingredienteId);

        Task<SingleResponseDTO<int>> CountAsync();        
    }
}