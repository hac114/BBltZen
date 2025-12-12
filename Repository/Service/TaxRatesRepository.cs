using Database;
using Database.Models;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class TaxRatesRepository(BubbleTeaContext context, ILogger<TaxRatesRepository> logger) : ITaxRatesRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<TaxRatesRepository> _logger = logger;

        private static TaxRatesDTO MapToDTO(TaxRates taxRates) 
        {
            return new TaxRatesDTO
            {
                TaxRateId = taxRates.TaxRateId,
                Aliquota = taxRates.Aliquota,
                Descrizione = taxRates.Descrizione,
                DataCreazione = taxRates.DataCreazione,
                DataAggiornamento = taxRates.DataAggiornamento                
            };
        }

        public async Task<PaginatedResponseDTO<TaxRatesDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.TaxRates
                    .AsNoTracking()
                    .OrderByDescending(t => t.Aliquota);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                // ✅ CORRETTO: messaggio appropriato per aliquote
                return new PaginatedResponseDTO<TaxRatesDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna aliquota trovata"
                        : $"Trovate {totalCount} aliquote"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<TaxRatesDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle aliquote"
                };
            }
        }

        public async Task<SingleResponseDTO<TaxRatesDTO>> GetByIdAsync(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SingleResponseDTO<TaxRatesDTO>.ErrorResponse("ID aliquota non valido");

                var taxRate = await _context.TaxRates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TaxRateId == taxRateId);

                if (taxRate == null)
                    return SingleResponseDTO<TaxRatesDTO>.NotFoundResponse(
                        $"Aliquota con ID {taxRateId} non trovata");

                return SingleResponseDTO<TaxRatesDTO>.SuccessResponse(
                    MapToDTO(taxRate),
                    $"Aliquota con ID {taxRateId} trovata ({taxRate.Aliquota:F2}%)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per taxRateId: {TaxRateId}", taxRateId);
                return SingleResponseDTO<TaxRatesDTO>.ErrorResponse(
                    "Errore interno nel recupero dell'aliquota");
            }
        }

        public async Task<PaginatedResponseDTO<TaxRatesDTO>> GetByAliquotaAsync(decimal aliquota, int page = 1, int pageSize = 10)
        {
            var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
            var skip = (safePage - 1) * safePageSize;

            var query = _context.TaxRates
                .Where(t => t.Aliquota == aliquota);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Aliquota)
                .Skip(skip)
                .Take(safePageSize)
                .Select(t => MapToDTO(t))
                .ToListAsync();

            return new PaginatedResponseDTO<TaxRatesDTO>
            {
                Data = items,
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                Message = totalCount == 0
                    ? $"Nessuna aliquota trovata con valore {aliquota}"
                    : $"Trovate {totalCount} aliquote con valore {aliquota}"
            };
        }

        public async Task<PaginatedResponseDTO<TaxRatesDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. NORMALIZZAZIONE input con StringHelper
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                // ✅ 2. Validazione input (dopo normalizzazione)
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<TaxRatesDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' è obbligatorio"
                    };
                }

                // ✅ 3. Validazione sicurezza con lunghezza specifica per descrizione
                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 100))
                {
                    return new PaginatedResponseDTO<TaxRatesDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' contiene caratteri non validi"
                    };
                }

                // ✅ 4. Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 5. Query con "INIZIA CON" case-insensitive
                // Usiamo direttamente StringHelper.StartsWithCaseInsensitive
                var query = _context.TaxRates
                    .AsNoTracking()
                    .Where(t => t.Descrizione != null &&
                               StringHelper.StartsWithCaseInsensitive(t.Descrizione, searchTerm))
                    .OrderByDescending(t => t.Aliquota)
                    .ThenBy(t => t.Descrizione); // ✅ Ordinamento secondario per descrizione

                // ✅ 6. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(t => MapToDTO(t))
                    .ToListAsync();

                // ✅ 7. Messaggio appropriato (usando il termine normalizzato)
                string message;
                if (totalCount == 0)
                {
                    message = $"Nessuna aliquota trovata con descrizione che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovata 1 aliquota con descrizione che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovate {totalCount} aliquote con descrizione che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<TaxRatesDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception)
            {
                // ✅ 8. Logging opzionale (se vuoi mantenere consistenza con altri metodi)
                // In alternativa, puoi rimuovere il try-catch e lasciare che l'eccezione salga
                return new PaginatedResponseDTO<TaxRatesDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle aliquote per descrizione"
                };
            }
        }

        private async Task<bool> ExistsByAliquotaDescrizioneInternalAsync(decimal aliquota, string descrizione, int? excludeId = null)
        {
            var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

            var query = _context.TaxRates
                .AsNoTracking()
                .Where(t => t.Aliquota == aliquota &&
                           StringHelper.EqualsCaseInsensitive(t.Descrizione, searchTerm));

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.TaxRateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<SingleResponseDTO<TaxRatesDTO>> AddAsync(TaxRatesDTO taxRateDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(taxRateDto);

                // ✅ Validazione input
                if (taxRateDto.Aliquota < 0 || taxRateDto.Aliquota > 100)
                    return SingleResponseDTO<TaxRatesDTO>.ErrorResponse("Aliquota deve essere compresa tra 0 e 100");

                if (string.IsNullOrWhiteSpace(taxRateDto.Descrizione))
                    return SingleResponseDTO<TaxRatesDTO>.ErrorResponse("Descrizione obbligatoria");

                var descrizione = StringHelper.NormalizeSearchTerm(taxRateDto.Descrizione);

                if (!SecurityHelper.IsValidInput(descrizione, 100))
                    return SingleResponseDTO<TaxRatesDTO>.ErrorResponse("Descrizione non valida");

                // ✅ Controllo duplicati (usa metodo interno)
                if (await ExistsByAliquotaDescrizioneInternalAsync(taxRateDto.Aliquota, descrizione, null))
                    return SingleResponseDTO<TaxRatesDTO>.ErrorResponse(
                        $"Esiste già un'aliquota {taxRateDto.Aliquota:F2}% con descrizione '{descrizione}'");

                var taxRate = new TaxRates
                {
                    Aliquota = taxRateDto.Aliquota,
                    Descrizione = descrizione,
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };

                await _context.TaxRates.AddAsync(taxRate);
                await _context.SaveChangesAsync();

                taxRateDto.TaxRateId = taxRate.TaxRateId;
                taxRateDto.DataCreazione = taxRate.DataCreazione;
                taxRateDto.DataAggiornamento = taxRate.DataAggiornamento;

                return SingleResponseDTO<TaxRatesDTO>.SuccessResponse(
                    taxRateDto,
                    $"Aliquota {taxRateDto.Aliquota:F2}% '{descrizione}' creata con successo (ID: {taxRate.TaxRateId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per taxRateDto: {@TaxRateDto}", taxRateDto);
                return SingleResponseDTO<TaxRatesDTO>.ErrorResponse("Errore interno durante la creazione dell'aliquota");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(TaxRatesDTO taxRateDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(taxRateDto);

                // ✅ Validazione input
                if (taxRateDto.Aliquota < 0 || taxRateDto.Aliquota > 100)
                    return SingleResponseDTO<bool>.ErrorResponse("Aliquota deve essere compresa tra 0 e 100");

                if (string.IsNullOrWhiteSpace(taxRateDto.Descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione obbligatoria");

                var descrizione = StringHelper.NormalizeSearchTerm(taxRateDto.Descrizione);

                if (!SecurityHelper.IsValidInput(descrizione, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione non valida");

                var taxRate = await _context.TaxRates
                    .FirstOrDefaultAsync(t => t.TaxRateId == taxRateDto.TaxRateId);

                if (taxRate == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Aliquota con ID {taxRateDto.TaxRateId} non trovata");

                // ✅ Controllo duplicati ESCLUDENDO questo ID (usa metodo interno)
                if (await ExistsByAliquotaDescrizioneInternalAsync(taxRateDto.Aliquota, descrizione, taxRateDto.TaxRateId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra aliquota {taxRateDto.Aliquota:F2}% con descrizione '{descrizione}'");

                // ✅ Aggiornamento solo se cambiato
                bool hasChanges = false;

                if (taxRate.Aliquota != taxRateDto.Aliquota)
                {
                    taxRate.Aliquota = taxRateDto.Aliquota;
                    hasChanges = true;
                }

                if (!StringHelper.EqualsCaseInsensitive(taxRate.Descrizione, descrizione))
                {
                    taxRate.Descrizione = descrizione;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    taxRate.DataAggiornamento = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Aliquota con ID {taxRateDto.TaxRateId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per l'aliquota con ID {taxRateDto.TaxRateId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per taxRateDto: {@TaxRateDto}", taxRateDto);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento dell'aliquota");
            }
        }

        private async Task<bool> HasDependenciesAsync(int taxRateId)
        {
            return await _context.OrderItem.AnyAsync(oi => oi.TaxRateId == taxRateId);
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID aliquota non valido");

                var taxRate = await _context.TaxRates.FindAsync(taxRateId);
                if (taxRate == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Aliquota con ID {taxRateId} non trovata");

                if (await HasDependenciesAsync(taxRateId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Impossibile eliminare aliquota IVA '{taxRate.Aliquota:F2}% ({taxRate.Descrizione})' perché ci sono dipendenze attive");

                _context.TaxRates.Remove(taxRate);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Aliquota '{taxRate.Aliquota:F2}% ({taxRate.Descrizione})' (ID: {taxRateId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per taxRateId: {TaxRateId}", taxRateId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione dell'aliquota");
            }
        }       

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int taxRateId)
        {
            try
            {
                if (taxRateId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID aliquota non valido");

                var exists = await _context.TaxRates
                    .AsNoTracking()
                    .AnyAsync(t => t.TaxRateId == taxRateId);

                string message = exists
                    ? $"Aliquota con ID {taxRateId} esiste"
                    : $"Aliquota con ID {taxRateId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per taxRateId: {TaxRateId}", taxRateId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'aliquota");
            }
        }       

        public async Task<SingleResponseDTO<bool>> ExistsByAliquotaAsync(decimal aliquota)
        {
            try
            {
                if (aliquota < 0 || aliquota > 100)
                    return SingleResponseDTO<bool>.ErrorResponse("Aliquota deve essere compresa tra 0 e 100");

                var exists = await _context.TaxRates
                    .AsNoTracking()
                    .AnyAsync(t => t.Aliquota == aliquota);

                string message = exists
                    ? $"Aliquota {aliquota:F2}% esiste"
                    : $"Aliquota {aliquota:F2}% non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByAliquotaAsync per aliquota: {Aliquota}", aliquota);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'aliquota");
            }
        }

        //private async Task<bool> ExistsByAliquotaDescrizioneAsync(decimal aliquota, string descrizione, int? excludeId = null)
        //{
        //    var query = _context.TaxRates
        //        .AsNoTracking()
        //        .Where(t => t.Aliquota == aliquota && t.Descrizione == descrizione);

        //    if (excludeId.HasValue)
        //    {
        //        query = query.Where(t => t.TaxRateId != excludeId.Value);
        //    }

        //    return await query.AnyAsync();
        //}

        public async Task<SingleResponseDTO<bool>> ExistsByAliquotaDescrizioneAsync(decimal aliquota, string descrizione)
        {
            try
            {
                if (aliquota < 0 || aliquota > 100)
                    return SingleResponseDTO<bool>.ErrorResponse("Aliquota deve essere compresa tra 0 e 100");

                if (string.IsNullOrWhiteSpace(descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione è obbligatoria");

                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                if (!SecurityHelper.IsValidInput(searchTerm, maxLength: 100))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione contiene caratteri non validi");

                var exists = await ExistsByAliquotaDescrizioneInternalAsync(aliquota, searchTerm, null);

                string message = exists
                    ? $"Aliquota {aliquota:F2}% con descrizione '{searchTerm}' esiste"
                    : $"Aliquota {aliquota:F2}% con descrizione '{searchTerm}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByAliquotaDescrizioneAsync per aliquota: {Aliquota}, descrizione: {Descrizione}", aliquota, descrizione);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'aliquota");
            }
        }
    }
}
