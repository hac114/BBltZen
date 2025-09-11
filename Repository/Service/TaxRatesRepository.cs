using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TaxRatesRepository
    {
        private readonly BubbleTeaContext _context;
        public TaxRatesRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<TaxRatesDTO>> GetAllAsync()
        {
            return await _context.TaxRates
                .Select(t => new TaxRatesDTO
                {
                    TaxRateId = t.TaxRateId,
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    
                    // Map all other properties from TaxRate entity to TaxRatesDTO
                })
                .ToListAsync();
        }

        public async Task<TaxRatesDTO> GetByIdAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates.FindAsync(taxRateId);
            if (taxRate == null) return null;

            return new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = taxRate.Aliquota,
                Descrizione = taxRate.Descrizione,
                
                // Map all other properties
            };
        }

        public async Task<TaxRatesDTO> GetByAliquotaAsync(decimal aliquota)
        {
            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(t => t.Aliquota == aliquota);

            if (taxRate == null) return null;

            return new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = taxRate.Aliquota,
                Descrizione = taxRate.Descrizione,
                
                // Map all other properties
            };
        }

        public async Task AddAsync(TaxRatesDTO taxRateDto)
        {
            var taxRate = new TaxRates
            {
                Aliquota = taxRateDto.Aliquota,
                Descrizione = taxRateDto.Descrizione,
                
                // Map all other properties from DTO to entity
            };

            await _context.TaxRates.AddAsync(taxRate);
            await _context.SaveChangesAsync();

            // Return the generated ID to the DTO
            taxRateDto.TaxRateId = taxRate.TaxRateId;
        }

        public async Task UpdateAsync(TaxRatesDTO taxRateDto)
        {
            var taxRate = await _context.TaxRates.FindAsync(taxRateDto.TaxRateId);
            if (taxRate == null)
                throw new ArgumentException("Tax rate not found");

            taxRate.Aliquota = taxRateDto.Aliquota;
            taxRate.Descrizione = taxRateDto.Descrizione;
            
            // Update all other properties

            _context.TaxRates.Update(taxRate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates.FindAsync(taxRateId);
            if (taxRate != null)
            {
                _context.TaxRates.Remove(taxRate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int taxRateId)
        {
            return await _context.TaxRates.AnyAsync(t => t.TaxRateId == taxRateId);
        }
    }
}
