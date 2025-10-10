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

        public async Task<UnitaDiMisuraDTO?> GetByIdAsync(int id)
        {
            return await _context.UnitaDiMisura
                .Where(u => u.UnitaMisuraId == id)
                .Select(u => new UnitaDiMisuraDTO
                {
                    UnitaMisuraId = u.UnitaMisuraId,
                    Sigla = u.Sigla,
                    Descrizione = u.Descrizione
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<UnitaDiMisuraDTO>> GetAllAsync()
        {
            return await _context.UnitaDiMisura
                .Select(u => new UnitaDiMisuraDTO
                {
                    UnitaMisuraId = u.UnitaMisuraId,
                    Sigla = u.Sigla,
                    Descrizione = u.Descrizione
                })
                .ToListAsync();
        }

        public async Task AddAsync(UnitaDiMisuraDTO unitaDto)
        {
            var unita = new UnitaDiMisura
            {
                Sigla = unitaDto.Sigla,
                Descrizione = unitaDto.Descrizione
            };

            _context.UnitaDiMisura.Add(unita);
            await _context.SaveChangesAsync();

            // Aggiorna l'ID generato
            unitaDto.UnitaMisuraId = unita.UnitaMisuraId;
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
    }
}
