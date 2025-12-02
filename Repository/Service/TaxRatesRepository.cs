using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Repository.Service.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TaxRatesRepository(BubbleTeaContext context) : ITaxRatesRepository
    {
        private readonly BubbleTeaContext _context = context;

        public async Task<PaginatedResponseDTO<TaxRatesDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.TaxRates
                .AsNoTracking()
                .OrderBy(t => t.Aliquota);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => new TaxRatesDTO
                {
                    TaxRateId = t.TaxRateId,
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    DataCreazione = t.DataCreazione,
                    DataAggiornamento = t.DataAggiornamento
                })
                .ToListAsync();

            return new PaginatedResponseDTO<TaxRatesDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TaxRatesDTO?> GetByIdAsync(int? taxRateId = null)
        {
            // ✅ SE NULL, RESTITUISCE NULL (il controller gestirà la lista)
            if (!taxRateId.HasValue)
                return null;

            if (taxRateId <= 0)
                return null;

            var taxRate = await _context.TaxRates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaxRateId == taxRateId.Value);

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

        public async Task<PaginatedResponseDTO<TaxRatesDTO>> GetByAliquotaAsync(decimal? aliquota = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.TaxRates.AsQueryable();

            // ✅ FILTRA SOLO SE ALIQUOTA SPECIFICATA
            if (aliquota.HasValue)
            {
                query = query.Where(t => t.Aliquota == aliquota.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Aliquota)
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => new TaxRatesDTO
                {
                    TaxRateId = t.TaxRateId,
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    DataCreazione = t.DataCreazione,
                    DataAggiornamento = t.DataAggiornamento
                })
                .ToListAsync();

            return new PaginatedResponseDTO<TaxRatesDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TaxRatesDTO> AddAsync(TaxRatesDTO taxRateDto)
        {
            ArgumentNullException.ThrowIfNull(taxRateDto);

            // ✅ VALIDAZIONE SICUREZZA INPUT
            if (!SecurityHelper.IsValidInput(taxRateDto.Descrizione, 100))
                throw new ArgumentException("Descrizione non valida");

            // ✅ CONTROLLO DUPLICATI (aliquota + descrizione)
            if (await ExistsByAliquotaDescrizioneAsync(taxRateDto.Aliquota, taxRateDto.Descrizione))
                throw new ArgumentException($"Esiste già un'aliquota {taxRateDto.Aliquota}% con descrizione '{taxRateDto.Descrizione}'");

            var taxRate = new TaxRates
            {
                Aliquota = taxRateDto.Aliquota,
                Descrizione = taxRateDto.Descrizione,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now
            };

            await _context.TaxRates.AddAsync(taxRate);
            await _context.SaveChangesAsync();

            taxRateDto.TaxRateId = taxRate.TaxRateId;
            taxRateDto.DataCreazione = taxRate.DataCreazione;
            taxRateDto.DataAggiornamento = taxRate.DataAggiornamento;

            return taxRateDto;
        }

        public async Task UpdateAsync(TaxRatesDTO taxRateDto)
        {
            ArgumentNullException.ThrowIfNull(taxRateDto);

            // ✅ VALIDAZIONE SICUREZZA INPUT
            if (!SecurityHelper.IsValidInput(taxRateDto.Descrizione, 100))
                throw new ArgumentException("Descrizione non valida");

            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(t => t.TaxRateId == taxRateDto.TaxRateId);

            if (taxRate == null)
                throw new ArgumentException($"Tax rate con ID {taxRateDto.TaxRateId} non trovato");

            // ✅ CONTROLLO DUPLICATI (aliquota + descrizione) ESCLUDENDO QUESTO ID
            if (await ExistsByAliquotaDescrizioneAsync(taxRateDto.Aliquota, taxRateDto.Descrizione, taxRateDto.TaxRateId))
                throw new ArgumentException($"Esiste già un'aliquota {taxRateDto.Aliquota}% con descrizione '{taxRateDto.Descrizione}'");

            // ✅ AGGIORNAMENTO
            taxRate.Aliquota = taxRateDto.Aliquota;
            taxRate.Descrizione = taxRateDto.Descrizione;
            taxRate.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON DATI DB
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
            // ✅ SILENT FAIL - Nessuna eccezione se non trovato
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
        public async Task<PaginatedResponseDTO<TaxRatesFrontendDTO>> GetAllPerFrontendAsync(int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.TaxRates
                .AsNoTracking()
                .OrderBy(t => t.Aliquota);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => new TaxRatesFrontendDTO
                {
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    AliquotaFormattata = $"{t.Aliquota.ToString("0.00", CultureInfo.InvariantCulture)}%"
                })
                .ToListAsync();

            return new PaginatedResponseDTO<TaxRatesFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedResponseDTO<TaxRatesFrontendDTO>> GetByAliquotaPerFrontendAsync(decimal? aliquota = null, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.TaxRates.AsQueryable();

            // ✅ FILTRA SOLO SE ALIQUOTA SPECIFICATA
            if (aliquota.HasValue)
            {
                query = query.Where(t => t.Aliquota == aliquota.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Aliquota)
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => new TaxRatesFrontendDTO
                {
                    Aliquota = t.Aliquota,
                    Descrizione = t.Descrizione,
                    AliquotaFormattata = $"{t.Aliquota.ToString("0.00", CultureInfo.InvariantCulture)}%"
                })
                .ToListAsync();

            return new PaginatedResponseDTO<TaxRatesFrontendDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> ExistsByAliquotaDescrizioneAsync(decimal aliquota, string descrizione, int? excludeId = null)
        {
            // ✅ USA STRINGHELPER PER NORMALIZZAZIONE
            var normalizedDescrizione = StringHelper.NormalizeSearchTerm(descrizione);

            var query = _context.TaxRates
                .Where(t => t.Aliquota == aliquota &&
                           StringHelper.EqualsCaseInsensitive(t.Descrizione, normalizedDescrizione));

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.TaxRateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasDependenciesAsync(int taxRateId)
        {
            // ✅ CONTROLLA SE CI SONO ORDER ITEM CHE USANO QUESTA ALIQUOTA
            return await _context.OrderItem
                .AnyAsync(oi => oi.TaxRateId == taxRateId);
        }
    }
}