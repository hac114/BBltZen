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

        public async Task<List<DimensioneBicchiereDTO>> GetAllAsync()
        {
            return await _context.DimensioneBicchiere
                .Select(d => new DimensioneBicchiereDTO
                {
                    DimensioneBicchiereId = d.DimensioneBicchiereId,
                    Sigla = d.Sigla,
                    Descrizione = d.Descrizione,
                    Capienza = d.Capienza,
                    UnitaMisuraId = d.UnitaMisuraId,
                    PrezzoBase = d.PrezzoBase,
                    Moltiplicatore = d.Moltiplicatore
                })
                .ToListAsync();
        }

        public async Task AddAsync(DimensioneBicchiereDTO dimensioneDto)
        {
            var dimensione = MapToEntity(dimensioneDto);
            _context.DimensioneBicchiere.Add(dimensione);
            await _context.SaveChangesAsync();

            dimensioneDto.DimensioneBicchiereId = dimensione.DimensioneBicchiereId;
        }

        public async Task UpdateAsync(DimensioneBicchiereDTO dimensioneDto)
        {
            var dimensione = await _context.DimensioneBicchiere
                .FindAsync(dimensioneDto.DimensioneBicchiereId);

            if (dimensione != null)
            {
                dimensione.Sigla = dimensioneDto.Sigla;
                dimensione.Descrizione = dimensioneDto.Descrizione;
                dimensione.Capienza = dimensioneDto.Capienza;
                dimensione.UnitaMisuraId = dimensioneDto.UnitaMisuraId;
                dimensione.PrezzoBase = dimensioneDto.PrezzoBase;
                dimensione.Moltiplicatore = dimensioneDto.Moltiplicatore;

                await _context.SaveChangesAsync();
            }
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
    }
}