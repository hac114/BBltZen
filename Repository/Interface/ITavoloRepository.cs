// Repository/Interface/ITavoloRepository.cs
using DTO;

namespace Repository.Interface
{
    public interface ITavoloRepository
    {
        // ✅ METODI PAGINATI (MODIFICATI)
        Task<PaginatedResponseDTO<TavoloDTO>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<TavoloDTO?> GetByIdAsync(int? tavoloId = null); // ✅ NULLABLE
        Task<TavoloDTO?> GetByNumeroAsync(int? numero = null); // ✅ NULLABLE
        Task<PaginatedResponseDTO<TavoloDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<TavoloDTO>> GetByZonaAsync(string? zona = null, int page = 1, int pageSize = 10);

        // ✅ METODI SCRITTURA
        Task<TavoloDTO> AddAsync(TavoloDTO tavoloDto);
        Task UpdateAsync(TavoloDTO tavoloDto);
        Task DeleteAsync(int tavoloId);

        // ✅ METODI VERIFICA
        Task<bool> ExistsAsync(int tavoloId);
        Task<bool> NumeroExistsAsync(int numero, int? excludeId = null);

        // ✅ METODI FRONTEND PAGINATI (MODIFICATI)        
        Task<PaginatedResponseDTO<TavoloFrontendDTO>> GetDisponibiliPerFrontendAsync(int page = 1, int pageSize = 10);
        Task<PaginatedResponseDTO<TavoloFrontendDTO>> GetByZonaPerFrontendAsync(string? zona = null, int page = 1, int pageSize = 10);
        Task<TavoloFrontendDTO?> GetByNumeroPerFrontendAsync(int? numero = null); // ✅ NULLABLE

        // ✅ METODI TOGGLE
        Task<bool> ToggleDisponibilitaAsync(int tavoloId);
        Task<bool> ToggleDisponibilitaByNumeroAsync(int numero);
    }
}