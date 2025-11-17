using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Service
{
    public class DimensioneBicchiereRepository : IDimensioneBicchiereRepository
    {
        private readonly BubbleTeaContext _context;

        public DimensioneBicchiereRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<DimensioneBicchiereDTO?> GetByIdAsync(int id)
        {
            var dimensione = await _context.DimensioneBicchiere
                .Where(d => d.DimensioneBicchiereId == id)
                .FirstOrDefaultAsync();

            if (dimensione == null) return null;

            return MapToDTO(dimensione);
        }

        public async Task<IEnumerable<DimensioneBicchiereDTO>> GetAllAsync() // ✅ CAMBIATO: IEnumerable invece di List
        {
            return await _context.DimensioneBicchiere
                .Select(d => MapToDTO(d)) // ✅ USA MapToDTO invece di mapping inline
                .ToListAsync();
        }

        public async Task<DimensioneBicchiereDTO> AddAsync(DimensioneBicchiereDTO dimensioneDto) // ✅ CAMBIATO: ritorna DTO
        {
            if (dimensioneDto == null)
                throw new ArgumentNullException(nameof(dimensioneDto));

            var dimensione = MapToEntity(dimensioneDto);
            _context.DimensioneBicchiere.Add(dimensione);
            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con ID generato
            dimensioneDto.DimensioneBicchiereId = dimensione.DimensioneBicchiereId;

            return dimensioneDto; // ✅ IMPORTANTE: ritorna il DTO
        }

        public async Task UpdateAsync(DimensioneBicchiereDTO dimensioneDto)
        {
            var dimensione = await _context.DimensioneBicchiere
                .FindAsync(dimensioneDto.DimensioneBicchiereId);

            if (dimensione == null)
                throw new ArgumentException($"Dimensione bicchiere con ID {dimensioneDto.DimensioneBicchiereId} non trovata");

            // ✅ MANTIENI LA LOGICA ORIGINALE
            dimensione.Sigla = dimensioneDto.Sigla;
            dimensione.Descrizione = dimensioneDto.Descrizione;
            dimensione.Capienza = dimensioneDto.Capienza;
            dimensione.UnitaMisuraId = dimensioneDto.UnitaMisuraId;
            dimensione.PrezzoBase = dimensioneDto.PrezzoBase;
            dimensione.Moltiplicatore = dimensioneDto.Moltiplicatore;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dimensione = await _context.DimensioneBicchiere.FindAsync(id);
            if (dimensione != null)
            {
                _context.DimensioneBicchiere.Remove(dimensione);
                await _context.SaveChangesAsync();
            }
        }

        private DimensioneBicchiereDTO MapToDTO(DimensioneBicchiere dimensione)
        {
            return new DimensioneBicchiereDTO
            {
                DimensioneBicchiereId = dimensione.DimensioneBicchiereId,
                Sigla = dimensione.Sigla,
                Descrizione = dimensione.Descrizione,
                Capienza = dimensione.Capienza,
                UnitaMisuraId = dimensione.UnitaMisuraId,
                PrezzoBase = dimensione.PrezzoBase,
                Moltiplicatore = dimensione.Moltiplicatore
            };
        }

        private DimensioneBicchiere MapToEntity(DimensioneBicchiereDTO dto)
        {
            return new DimensioneBicchiere
            {
                DimensioneBicchiereId = dto.DimensioneBicchiereId,
                Sigla = dto.Sigla,
                Descrizione = dto.Descrizione,
                Capienza = dto.Capienza,
                UnitaMisuraId = dto.UnitaMisuraId,
                PrezzoBase = dto.PrezzoBase,
                Moltiplicatore = dto.Moltiplicatore
            };
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.DimensioneBicchiereId == id);
        }

        // ✅ METODI AGGIUNTIVI CHE POTRESTI VOLER AGGIUNGERE
        public async Task<DimensioneBicchiereDTO?> GetBySiglaAsync(string sigla)
        {
            var dimensione = await _context.DimensioneBicchiere
                .FirstOrDefaultAsync(d => d.Sigla == sigla);
            return dimensione == null ? null : MapToDTO(dimensione);
        }

        public async Task<bool> SiglaExistsAsync(string sigla)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.Sigla == sigla);
        }

        public async Task<bool> SiglaExistsForOtherAsync(int id, string sigla)
        {
            return await _context.DimensioneBicchiere
                .AnyAsync(d => d.DimensioneBicchiereId != id && d.Sigla == sigla);
        }
    }
}