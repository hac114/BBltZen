using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class VwStatisticheOrdiniAvanzateRepository : IVwStatisticheOrdiniAvanzateRepository, IVwStatisticheOrdiniAvanzateRepository
    {
        private readonly BubbleTeaContext _context;

        public VwStatisticheOrdiniAvanzateRepository(BubbleTeaContext context)
        {
            _context = context;

        }
        public async Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetAllAsync()
        {
            return await _context.VwStatisticheOrdiniAvanzate
                .Select(v => new VwStatisticheOrdiniAvanzateDTO
                {
                    // Map all properties from the view entity to DTO
                    OrdineId = v.OrdineId,
                    MinutiInStato = v.MinutiInStato,
                    SogliaAttenzione = v.SogliaAttenzione,
                    SogliaCritico = v.SogliaCritico,
                    LivelloAllerta = v.LivelloAllerta,
                    MessaggioAllerta = v.MessaggioAllerta,
                    // Add other properties as needed
                })
                .ToListAsync();
        }

        public async Task<VwStatisticheOrdiniAvanzateDTO> GetByOrdineIdAsync(int ordineId)
        {
            var viewEntity = await _context.VwStatisticheOrdiniAvanzate
                .FirstOrDefaultAsync(v => v.OrdineId == ordineId);

            if (viewEntity == null) return null;

            return new VwStatisticheOrdiniAvanzateDTO
            {
                OrdineId = viewEntity.OrdineId,
                MinutiInStato = viewEntity.MinutiInStato,
                SogliaAttenzione = viewEntity.SogliaAttenzione,
                SogliaCritico = viewEntity.SogliaCritico,
                LivelloAllerta = viewEntity.LivelloAllerta,
                MessaggioAllerta = viewEntity.MessaggioAllerta,
                // Map other properties as needed
            };
        }

        public async Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetByLivelloAllertaAsync(string livelloAllerta)
        {
            return await _context.VwStatisticheOrdiniAvanzate
                .Where(v => v.LivelloAllerta == livelloAllerta)
                .Select(v => new VwStatisticheOrdiniAvanzateDTO
                {
                    OrdineId = v.OrdineId,
                    MinutiInStato = v.MinutiInStato,
                    SogliaAttenzione = v.SogliaAttenzione,
                    SogliaCritico = v.SogliaCritico,
                    LivelloAllerta = v.LivelloAllerta,
                    MessaggioAllerta = v.MessaggioAllerta,
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VwStatisticheOrdiniAvanzateDTO>> GetOrdiniInRitardoAsync()
        {
            return await _context.VwStatisticheOrdiniAvanzate
                .Where(v => v.LivelloAllerta == "Critico" || v.LivelloAllerta == "Attenzione")
                .Select(v => new VwStatisticheOrdiniAvanzateDTO
                {
                    OrdineId = v.OrdineId,
                    MinutiInStato = v.MinutiInStato,
                    SogliaAttenzione = v.SogliaAttenzione,
                    SogliaCritico = v.SogliaCritico,
                    LivelloAllerta = v.LivelloAllerta,
                    MessaggioAllerta = v.MessaggioAllerta,
                    // Map other properties as needed
                })
                .ToListAsync();
        }
    }
}

