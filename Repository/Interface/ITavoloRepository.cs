using DTO;

namespace Repository.Interface
{
    public interface ITavoloRepository
    {
        // ✅ METODI PAGINATI (MODIFICATI)
        Task<PaginatedResponseDTO<TavoloDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SingleResponseDTO<TavoloDTO>> GetByIdAsync(int tavoloId);
        Task<SingleResponseDTO<TavoloDTO>> GetByNumeroAsync(int numero); 
        Task<PaginatedResponseDTO<TavoloDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10); 
        Task<PaginatedResponseDTO<TavoloDTO>> GetOccupatiAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<TavoloDTO>> GetByZonaAsync(string zona, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<SingleResponseDTO<TavoloDTO>> AddAsync(TavoloDTO tavoloDto);
        Task<SingleResponseDTO<bool>> UpdateAsync(TavoloDTO tavoloDto);
        Task<SingleResponseDTO<bool>> DeleteAsync(int tavoloId);

        // ✅ METODI VERIFICA
        Task<SingleResponseDTO<bool>> ExistsAsync(int tavoloId);
        Task<SingleResponseDTO<bool>> NumeroExistsAsync(int numero, int? excludeId = null);

        // ✅ METODI TOGGLE
        Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int tavoloId);
        Task<SingleResponseDTO<bool>> ToggleDisponibilitaByNumeroAsync(int numero);

        // ✅ METODI CONTEGGI RAPIDI
        Task<SingleResponseDTO<int>> CountAsync();
        Task<SingleResponseDTO<int>> CountDisponibiliAsync();
        Task<SingleResponseDTO<int>> CountOccupatiAsync();
    }
}