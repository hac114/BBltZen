using DTO;

namespace Repository.Interface
{
    public interface IBevandaStandardRepository
    {
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<BevandaStandardDTO>> GetByIdAsync(int articoloId);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10);        
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllOrderedByDimensioneAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetAllOrderedByPersonalizzazioneAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetByDimensioneBicchiereAsync(int dimensioneBicchiereId, int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetByPersonalizzazioneAsync(int personalizzazioneId, int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<BevandaStandardDTO>> AddAsync(BevandaStandardDTO bevandaStandardDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(BevandaStandardDTO bevandaStandardDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId);
        
        Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId);
        Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int personalizzazioneId, int dimensioneBicchiereId);
        Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(string personalizzazione, string descrizioneBicchiere);

        Task<PaginatedResponseDTO<BevandaStandardCardDTO>> GetCardProdottiAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<BevandaStandardCardDTO>> GetCardProdottoByIdAsync(int articoloId);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetPrimoPianoAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaStandardDTO>> GetSecondoPianoAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<BevandaStandardCardDTO>> GetCardProdottiPrimoPianoAsync(int page = 1, int pageSize = 10);

        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountPrimoPianoAsync();
        Task<SingleResponseDTO<int>> CountSecondoPianoAsync();
        Task<SingleResponseDTO<int>> CountDisponibiliAsync();
        Task<SingleResponseDTO<int>> CountNonDisponibiliAsync();
    }
}