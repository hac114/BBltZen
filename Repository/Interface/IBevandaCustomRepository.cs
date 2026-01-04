using DTO;

namespace Repository.Interface
{
    public interface IBevandaCustomRepository
    {
        Task<PaginatedResponseDTO<BevandaCustomDTO>> GetAllAsync(int page = 1, int pageSize = 10);                
        Task<SingleResponseDTO<BevandaCustomDTO>> GetByPersCustomIdAsync(int persCustomId);        

        Task<SingleResponseDTO<BevandaCustomDTO>> AddAsync(BevandaCustomDTO bevandaCustomDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(BevandaCustomDTO bevandaCustomDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId);

        Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId);        
        Task<SingleResponseDTO<bool>> ExistsByPersCustomIdAsync(int persCustomId);

        Task<SingleResponseDTO<BevandaCustomCardDTO>> GetByIdAsync(int articoloId);
        Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetAllOrderedByDimensioneAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetAllOrderedByPersonalizzazioneAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardProdottiAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<BevandaCustomCardDTO>> GetCardProdottoByIdAsync(int articoloId);
        Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardPersonalizzazioneAsync(string nomePersonalizzazione, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaCustomCardDTO>> GetCardDimensioneBicchiereAsync(string descrizioneBicchiere, int page = 1, int pageSize = 10);
        
        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountDimensioneBicchiereAsync(string descrizionBicchiere);
        Task<SingleResponseDTO<int>> CountPersonalizzazioneAsync(string nomePersonalizzazione);
    }
}