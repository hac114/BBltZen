using DTO;

namespace Repository.Interface
{
    public interface IConfigSoglieTempiRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<ConfigSoglieTempiDTO> AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto);

        Task UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto);
        Task DeleteAsync(int sogliaId);
        Task<bool> ExistsAsync(int sogliaId);

        // ✅ CORREGGI: GetAll con IEnumerable
        Task<IEnumerable<ConfigSoglieTempiDTO>> GetAllAsync();
        Task<ConfigSoglieTempiDTO?> GetByIdAsync(int sogliaId);

        // ✅ METODI DI FILTRO
        Task<ConfigSoglieTempiDTO?> GetByStatoOrdineIdAsync(int statoOrdineId);
        Task<bool> ExistsByStatoOrdineIdAsync(int statoOrdineId, int? excludeSogliaId = null);

        // ✅ AGGIUNGI: Metodi per business logic
        Task<Dictionary<int, ConfigSoglieTempiDTO>> GetSoglieByStatiOrdineAsync(IEnumerable<int> statiOrdineIds);
        Task<bool> ValidateSoglieAsync(int sogliaAttenzione, int sogliaCritico);

        // ✅ NUOVO: Metodo per validazione completa DTO
        Task<(bool IsValid, string? ErrorMessage)> ValidateConfigSoglieAsync(ConfigSoglieTempiDTO configDto);
    }
}