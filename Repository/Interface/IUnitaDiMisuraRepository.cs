using DTO;

namespace Repository.Interface
{
    public interface IUnitaDiMisuraRepository
    {
        // ✅ CORREGGI: AddAsync deve ritornare DTO
        Task<UnitaDiMisuraDTO> AddAsync(UnitaDiMisuraDTO unitaDto);

        Task UpdateAsync(UnitaDiMisuraDTO unitaDto);
        Task DeleteAsync(int id);

        // ✅ CORREGGI: Metodo esistenza mancante
        Task<bool> ExistsAsync(int id);

        // ✅ CORREGGI: GetById nullable e GetAll con IEnumerable
        Task<UnitaDiMisuraDTO?> GetByIdAsync(int id);
        Task<IEnumerable<UnitaDiMisuraDTO>> GetAllAsync();

        // ✅ AGGIUNGI: Metodi per validazione sigla univoca
        Task<bool> SiglaExistsAsync(string sigla);
        Task<bool> SiglaExistsForOtherAsync(int id, string sigla);
        Task<UnitaDiMisuraDTO?> GetBySiglaAsync(string sigla);
    }
}