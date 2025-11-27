using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Service
{
    public class UnitaDiMisuraRepository : IUnitaDiMisuraRepository
    {
        private readonly BubbleTeaContext _context;

        public UnitaDiMisuraRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        private UnitaDiMisuraDTO MapToDTO(UnitaDiMisura unita)
        {
            return new UnitaDiMisuraDTO
            {
                UnitaMisuraId = unita.UnitaMisuraId,
                Sigla = unita.Sigla,
                Descrizione = unita.Descrizione
            };
        }

        public async Task<UnitaDiMisuraDTO?> GetByIdAsync(int id)
        {
            var unita = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.UnitaMisuraId == id);

            return unita == null ? null : MapToDTO(unita);
        }

        public async Task<IEnumerable<UnitaDiMisuraDTO>> GetAllAsync()
        {
            return await _context.UnitaDiMisura
                .Select(u => MapToDTO(u))
                .ToListAsync();
        }

        public async Task<UnitaDiMisuraDTO> AddAsync(UnitaDiMisuraDTO unitaDto)
        {
            if (unitaDto == null)
                throw new ArgumentNullException(nameof(unitaDto));

            var unita = new UnitaDiMisura
            {
                Sigla = unitaDto.Sigla,
                Descrizione = unitaDto.Descrizione
            };

            _context.UnitaDiMisura.Add(unita);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO
            unitaDto.UnitaMisuraId = unita.UnitaMisuraId;
            return unitaDto; // ✅ IMPORTANTE: ritorna il DTO
        }

        public async Task UpdateAsync(UnitaDiMisuraDTO unitaDto)
        {
            var unita = await _context.UnitaDiMisura
                .FindAsync(unitaDto.UnitaMisuraId);

            if (unita != null)
            {
                unita.Sigla = unitaDto.Sigla;
                unita.Descrizione = unitaDto.Descrizione;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var unita = await _context.UnitaDiMisura.FindAsync(id);
            if (unita != null)
            {
                _context.UnitaDiMisura.Remove(unita);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ METODO ESISTENZA MANCANTE
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.UnitaMisuraId == id);
        }

        // ✅ METODI PER SIGLA UNIVOCA
        public async Task<bool> SiglaExistsAsync(string sigla)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.Sigla == sigla);
        }

        public async Task<bool> SiglaExistsForOtherAsync(int id, string sigla)
        {
            return await _context.UnitaDiMisura
                .AnyAsync(u => u.UnitaMisuraId != id && u.Sigla == sigla);
        }

        public async Task<UnitaDiMisuraDTO?> GetBySiglaAsync(string sigla)
        {
            var unita = await _context.UnitaDiMisura
                .FirstOrDefaultAsync(u => u.Sigla == sigla);

            return unita == null ? null : MapToDTO(unita);
        }

        public async Task<IEnumerable<UnitaDiMisuraFrontendDTO>> GetAllPerFrontendAsync()
        {
            return await _context.UnitaDiMisura
                .AsNoTracking()
                .Select(u => new UnitaDiMisuraFrontendDTO
                {
                    Sigla = u.Sigla,
                    Descrizione = u.Descrizione
                })
                .ToListAsync();
        }

        public async Task<UnitaDiMisuraFrontendDTO?> GetBySiglaPerFrontendAsync(string sigla)
        {
            var unita = await _context.UnitaDiMisura
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Sigla == sigla);

            if (unita == null) return null;

            return new UnitaDiMisuraFrontendDTO
            {
                Sigla = unita.Sigla,
                Descrizione = unita.Descrizione
            };
        }
    }
}
