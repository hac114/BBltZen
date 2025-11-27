using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TaxRatesRepository : ITaxRatesRepository
    {
        private readonly BubbleTeaContext _context;

        public TaxRatesRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaxRatesDTO>> GetAllAsync()
        {
            return await _context.TaxRates
                .AsNoTracking()
                .Select(t => new TaxRatesDTO
                {
                    TaxRateId = t.TaxRateId,
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    DataCreazione = t.DataCreazione,
                    DataAggiornamento = t.DataAggiornamento
                })
                .ToListAsync();
        }

        public async Task<TaxRatesDTO?> GetByIdAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaxRateId == taxRateId);

            if (taxRate == null) return null;

            return new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = taxRate.Aliquota,
                Descrizione = taxRate.Descrizione,
                DataCreazione = taxRate.DataCreazione,
                DataAggiornamento = taxRate.DataAggiornamento
            };
        }

        public async Task<TaxRatesDTO?> GetByAliquotaAsync(decimal aliquota)
        {
            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Aliquota == aliquota);

            if (taxRate == null) return null;

            return new TaxRatesDTO
            {
                TaxRateId = taxRate.TaxRateId,
                Aliquota = taxRate.Aliquota,
                Descrizione = taxRate.Descrizione,
                DataCreazione = taxRate.DataCreazione,
                DataAggiornamento = taxRate.DataAggiornamento
            };
        }

        public async Task<TaxRatesDTO> AddAsync(TaxRatesDTO taxRateDto) // ✅ CORREGGI: ritorna DTO
        {
            if (taxRateDto == null)
                throw new ArgumentNullException(nameof(taxRateDto));

            var taxRate = new TaxRates
            {
                Aliquota = taxRateDto.Aliquota,
                Descrizione = taxRateDto.Descrizione,
                DataCreazione = DateTime.Now, // ✅ NOT NULL - valore default
                DataAggiornamento = DateTime.Now // ✅ NOT NULL - valore default
            };

            await _context.TaxRates.AddAsync(taxRate);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
            taxRateDto.TaxRateId = taxRate.TaxRateId;
            taxRateDto.DataCreazione = taxRate.DataCreazione;
            taxRateDto.DataAggiornamento = taxRate.DataAggiornamento;

            return taxRateDto; // ✅ AGGIUNGI return
        }

        public async Task UpdateAsync(TaxRatesDTO taxRateDto)
        {
            if (taxRateDto == null) // ✅ AGGIUNGI validazione
                throw new ArgumentNullException(nameof(taxRateDto));

            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(t => t.TaxRateId == taxRateDto.TaxRateId);

            if (taxRate == null)
                throw new ArgumentException($"Tax rate con ID {taxRateDto.TaxRateId} non trovato");

            taxRate.Aliquota = taxRateDto.Aliquota;
            taxRate.Descrizione = taxRateDto.Descrizione;
            taxRate.DataAggiornamento = DateTime.Now; // ✅ NOT NULL - aggiornamento automatico

            await _context.SaveChangesAsync();

            taxRateDto.DataAggiornamento = taxRate.DataAggiornamento;
        }

        public async Task DeleteAsync(int taxRateId)
        {
            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(t => t.TaxRateId == taxRateId);

            if (taxRate != null)
            {
                _context.TaxRates.Remove(taxRate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int taxRateId)
        {
            return await _context.TaxRates
                .AnyAsync(t => t.TaxRateId == taxRateId);
        }

        public async Task<bool> ExistsByAliquotaAsync(decimal aliquota, int? excludeTaxRateId = null)
        {
            var query = _context.TaxRates.Where(t => t.Aliquota == aliquota);

            if (excludeTaxRateId.HasValue)
            {
                query = query.Where(t => t.TaxRateId != excludeTaxRateId.Value);
            }

            return await query.AnyAsync();
        }

        // ✅ AGGIUNGI QUESTI METODI
        public async Task<IEnumerable<TaxRatesFrontendDTO>> GetAllPerFrontendAsync()
        {
            return await _context.TaxRates
                .AsNoTracking()
                .Select(t => new TaxRatesFrontendDTO
                {
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    AliquotaFormattata = $"{t.Aliquota.ToString("0.00", CultureInfo.InvariantCulture)}%" // ✅ FORMATTO FISSO
                })
                .ToListAsync();
        }

        public async Task<TaxRatesFrontendDTO?> GetByAliquotaPerFrontendAsync(decimal aliquota)
        {
            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Aliquota == aliquota);

            if (taxRate == null) return null;

            return new TaxRatesFrontendDTO
            {
                Aliquota = taxRate.Aliquota,
                Descrizione = taxRate.Descrizione,
                AliquotaFormattata = $"{taxRate.Aliquota.ToString("0.00", CultureInfo.InvariantCulture)}%" // ✅ FORMATTO FISSO
            };
        }
    }
}