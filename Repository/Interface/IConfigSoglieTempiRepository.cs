using DTO;

namespace Repository.Interface
{
    public interface IConfigSoglieTempiRepository
    {
        Task<PaginatedResponseDTO<ConfigSoglieTempiDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<ConfigSoglieTempiDTO>> GetByIdAsync(int sogliaId);
        Task<SingleResponseDTO<ConfigSoglieTempiDTO>> GetByStatoOrdineAsync(string statoOrdine);
        Task<SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>> GetSoglieByStatiOrdineAsync(IEnumerable<int> statiOrdineIds);       

        Task<SingleResponseDTO<ConfigSoglieTempiDTO>> AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int sogliaId, string utenteRichiedente);
        
        Task<SingleResponseDTO<bool>> ExistsAsync(int sogliaId);         
        Task<SingleResponseDTO<bool>> ExistsByStatoOrdine(string statoOrdine);
        
        
        
    }
}