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
    public class VwIngredientiPopolariRepository : IVwIngredientiPopolariRepository
    {
        private readonly BubbleTeaContext _context;

        public VwIngredientiPopolariRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<VwIngredientiPopolariDTO>> GetAllAsync()
        {
            return await _context.VwIngredientiPopolari
                .OrderByDescending(v => v.NumeroSelezioni)
                .Select(v => new VwIngredientiPopolariDTO
                {
                    IngredienteId = v.IngredienteId,
                    NomeIngrediente = v.NomeIngrediente,
                    Categoria = v.Categoria,
                    NumeroSelezioni = v.NumeroSelezioni,
                    NumeroOrdiniContenenti = v.NumeroOrdiniContenenti,
                    PercentualeTotale = v.PercentualeTotale
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VwIngredientiPopolariDTO>> GetTopNAsync(int topN)
        {
            return await _context.VwIngredientiPopolari
                .OrderByDescending(v => v.NumeroSelezioni)
                .Take(topN)
                .Select(v => new VwIngredientiPopolariDTO
                {
                    IngredienteId = v.IngredienteId,
                    NomeIngrediente = v.NomeIngrediente,
                    Categoria = v.Categoria,
                    NumeroSelezioni = v.NumeroSelezioni,
                    NumeroOrdiniContenenti = v.NumeroOrdiniContenenti,
                    PercentualeTotale = v.PercentualeTotale
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<VwIngredientiPopolariDTO>> GetByCategoriaAsync(string categoria)
        {
            return await _context.VwIngredientiPopolari
                .Where(v => v.Categoria == categoria)
                .OrderByDescending(v => v.NumeroSelezioni)
                .Select(v => new VwIngredientiPopolariDTO
                {
                    IngredienteId = v.IngredienteId,
                    NomeIngrediente = v.NomeIngrediente,
                    Categoria = v.Categoria,
                    NumeroSelezioni = v.NumeroSelezioni,
                    NumeroOrdiniContenenti = v.NumeroOrdiniContenenti,
                    PercentualeTotale = v.PercentualeTotale
                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<VwIngredientiPopolariDTO> GetByIngredienteIdAsync(int ingredienteId)
        {
            var viewEntity = await _context.VwIngredientiPopolari
                .FirstOrDefaultAsync(v => v.IngredienteId == ingredienteId);

            if (viewEntity == null) return null;

            return new VwIngredientiPopolariDTO
            {
                IngredienteId = viewEntity.IngredienteId,
                NomeIngrediente = viewEntity.NomeIngrediente,
                Categoria = viewEntity.Categoria,
                NumeroSelezioni = viewEntity.NumeroSelezioni,
                NumeroOrdiniContenenti = viewEntity.NumeroOrdiniContenenti,
                PercentualeTotale = viewEntity.PercentualeTotale
                // Map other properties as needed
            };
        }
    }
}
