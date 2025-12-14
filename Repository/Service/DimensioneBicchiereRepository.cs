using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;
using System.ComponentModel;

namespace Repository.Service
{
    public class DimensioneBicchiereRepository(BubbleTeaContext context, ILogger<DimensioneBicchiereRepository> logger) : IDimensioneBicchiereRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger _logger = logger;

        private static DimensioneBicchiereDTO MapToDTO(DimensioneBicchiere bicchiere)
        {
            var dto = new DimensioneBicchiereDTO
            {
                DimensioneBicchiereId = bicchiere.DimensioneBicchiereId,
                Sigla = bicchiere.Sigla,
                Descrizione = bicchiere.Descrizione,
                Capienza = bicchiere.Capienza,
                PrezzoBase = bicchiere.PrezzoBase,
                Moltiplicatore = bicchiere.Moltiplicatore,
                UnitaMisuraId = bicchiere.UnitaMisuraId
            };

            // ✅ Gestione sicura della navigation property
            if (bicchiere.UnitaMisura != null)
            {
                dto.UnitaMisura = new UnitaDiMisuraDTO
                {
                    UnitaMisuraId = bicchiere.UnitaMisura.UnitaMisuraId,
                    Sigla = bicchiere.UnitaMisura.Sigla,
                    Descrizione = bicchiere.UnitaMisura.Descrizione
                };
            }
            else
            {
                // ✅ Assegna null esplicitamente se necessario
                dto.UnitaMisura = null;
            }

            return dto;
        }

        private async Task<bool> ExistsSiglaInternalAsync(string sigla)
        {
            if (string.IsNullOrWhiteSpace(sigla))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(sigla);
            return await _context.DimensioneBicchiere
                .AsNoTracking()
                .AnyAsync(b => StringHelper.EqualsCaseInsensitive(b.Sigla, searchTerm));
        }

        private async Task<bool> ExistsDescrizioneInternalAsync(string descrizione)
        {
            if (string.IsNullOrWhiteSpace(descrizione))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);
            return await _context.DimensioneBicchiere
                .AsNoTracking()
                .AnyAsync(b => StringHelper.EqualsCaseInsensitive(b.Descrizione, searchTerm));
        }

        private async Task<bool> ExistsSiglaForOtherInternalAsync(int bicchiereId, string sigla)
        {
            if (string.IsNullOrWhiteSpace(sigla))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(sigla);
            return await _context.DimensioneBicchiere
                .AsNoTracking()
                .AnyAsync(b => b.DimensioneBicchiereId != bicchiereId && StringHelper.EqualsCaseInsensitive(b.Sigla, searchTerm));
        }

        private async Task<bool> ExistsDescrizioneForOtherInternalAsync(int bicchiereId, string descrizione)
        {
            if (string.IsNullOrWhiteSpace(descrizione))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);
            return await _context.DimensioneBicchiere
                .AsNoTracking()
                .AnyAsync(b => b.DimensioneBicchiereId != bicchiereId && StringHelper.EqualsCaseInsensitive(b.Descrizione, searchTerm));
        }

        // ✅ Metodo per verificare esistenza Unità di Misura
        private async Task<bool> UnitaMisuraExistsInternalAsync(int unitaMisuraId)
        {
            if (unitaMisuraId <= 0)
                return false;

            return await _context.UnitaDiMisura
                .AsNoTracking()
                .AnyAsync(u => u.UnitaMisuraId == unitaMisuraId);
        }

        private async Task<bool> HasDependenciesAsync(int dimensioneBicchiereId)
        {
            bool hasBevandaStandard = await _context.BevandaStandard
                .AnyAsync(b => b.DimensioneBicchiereId == dimensioneBicchiereId);

            bool hasDimensioneQuantitaIngredienti = await _context.DimensioneQuantitaIngredienti
                .AnyAsync(d => d.DimensioneBicchiereId == dimensioneBicchiereId);

            bool hasPersonalizzazioneCustom = await _context.PersonalizzazioneCustom
                .AnyAsync(p => p.DimensioneBicchiereId == dimensioneBicchiereId);

            bool hasPreferitiCliente = await _context.PreferitiCliente
                .AnyAsync(c => c.DimensioneBicchiereId == dimensioneBicchiereId);

            return hasBevandaStandard || hasDimensioneQuantitaIngredienti || hasPersonalizzazioneCustom || hasPreferitiCliente;
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.DimensioneBicchiere
                    .AsNoTracking()
                    .OrderBy(b => b.Sigla)
                    .ThenBy(b => b.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessun bicchiere trovato"
                        : $"Trovato {totalCount} bicchieri"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei bicchieri"
                };
            }
        }

        public async Task<SingleResponseDTO<DimensioneBicchiereDTO>> GetByIdAsync(int bicchiereId)
        {
            try
            {
                if (bicchiereId <= 0)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("ID bicchiere non valido");

                var bicchiere = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.DimensioneBicchiereId == bicchiereId);

                if (bicchiere == null)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.NotFoundResponse(
                        $"Bicchiere con ID {bicchiereId} non trovato");

                return SingleResponseDTO<DimensioneBicchiereDTO>.SuccessResponse(
                    MapToDTO(bicchiere),
                    $"Bicchiere con ID {bicchiereId} trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per bicchiereId: {BicchiereId}", bicchiereId);
                return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse(
                    "Errore interno nel recupero del bicchiere");
            }
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetBySiglaAsync(string sigla, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 3))
                {
                    return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'sigla' contiene caratteri non validi"
                    };
                }

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(sigla);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'sigla' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.DimensioneBicchiere
                    .AsNoTracking()
                    .Where(b => b.Sigla != null &&
                               StringHelper.StartsWithCaseInsensitive(b.Sigla, searchTerm))
                    .OrderBy(b => b.Sigla)
                    .ThenBy(b => b.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessun bicchiere trovato con sigla che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovato 1 bicchiere con sigla che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovati {totalCount} bicchieri con sigla che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
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
                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei bicchieri per sigla"
                };
            }
        }

        public async Task<PaginatedResponseDTO<DimensioneBicchiereDTO>> GetByDescrizioneAsync(string descrizione, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                {
                    return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' contiene caratteri non validi"
                    };
                }

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizione' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.DimensioneBicchiere
                    .AsNoTracking()
                    .Where(b => b.Descrizione != null &&
                               StringHelper.StartsWithCaseInsensitive(b.Descrizione, searchTerm))
                    .OrderBy(b => b.Sigla)
                    .ThenBy(b => b.Descrizione);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                string message;
                if (totalCount == 0)
                {
                    message = $"Nessun bicchiere trovato con descrizione che inizia con '{searchTerm}'";
                }
                else if (totalCount == 1)
                {
                    message = $"Trovato 1 bicchiere con descrizione che inizia con '{searchTerm}'";
                }
                else
                {
                    message = $"Trovati {totalCount} bicchieri con descrizione che inizia con '{searchTerm}'";
                }

                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
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
                return new PaginatedResponseDTO<DimensioneBicchiereDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei bicchieri per descrizione"
                };
            }
        }

        public async Task<SingleResponseDTO<DimensioneBicchiereDTO>> AddAsync(DimensioneBicchiereDTO bicchiereDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bicchiereDto);

                // ✅ 1. Validazioni input obbligatori
                if (string.IsNullOrWhiteSpace(bicchiereDto.Sigla))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Sigla obbligatoria");

                if (string.IsNullOrWhiteSpace(bicchiereDto.Descrizione))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Descrizione obbligatoria");

                if (bicchiereDto.UnitaMisuraId <= 0)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Unità di misura obbligatoria");

                // ✅ 2. Validazioni range/numeriche
                if (bicchiereDto.Capienza < 250m || bicchiereDto.Capienza > 1000m)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("La capienza deve essere tra 250 e 1000");

                if (bicchiereDto.PrezzoBase < 0.01m || bicchiereDto.PrezzoBase > 100m)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Il prezzo base deve essere tra 0.01 e 100");

                if (bicchiereDto.Moltiplicatore < 0.1m || bicchiereDto.Moltiplicatore > 3.0m)
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Il moltiplicatore deve essere tra 0.1 e 3.0");

                // ✅ 3. Validazione sicurezza SULL'INPUT ORIGINALE con SecurityHelper
                if (!SecurityHelper.IsValidInput(bicchiereDto.Sigla, 3))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Sigla non valida o contiene caratteri pericolosi");

                if (!SecurityHelper.IsValidInput(bicchiereDto.Descrizione, 50))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse("Descrizione non valida o contiene caratteri pericolosi");

                // ✅ 4. Validazione Unità di Misura esista
                if (!await UnitaMisuraExistsInternalAsync(bicchiereDto.UnitaMisuraId))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse(
                        $"Unità di misura con ID {bicchiereDto.UnitaMisuraId} non trovata");

                // ✅ 5. SOLO DOPO la validazione, normalizza con StringHelper
                var sigla = StringHelper.NormalizeSearchTerm(bicchiereDto.Sigla);
                var descrizione = StringHelper.NormalizeSearchTerm(bicchiereDto.Descrizione);

                // ✅ 6. Controllo duplicati (usa metodi interni che usano StringHelper)
                if (await ExistsSiglaInternalAsync(sigla))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse(
                        $"Esiste già un bicchiere con sigla '{sigla}'");

                if (await ExistsDescrizioneInternalAsync(descrizione))
                    return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse(
                        $"Esiste già un bicchiere con descrizione '{descrizione}'");

                // ✅ 7. CREAZIONE ENTITÀ
                var bicchiere = new DimensioneBicchiere
                {
                    Sigla = SecurityHelper.NormalizeSafe(sigla),
                    Descrizione = descrizione,
                    Capienza = bicchiereDto.Capienza,
                    UnitaMisuraId = bicchiereDto.UnitaMisuraId,
                    PrezzoBase = bicchiereDto.PrezzoBase,
                    Moltiplicatore = bicchiereDto.Moltiplicatore
                };

                // ✅ 8. SALVATAGGIO
                await _context.DimensioneBicchiere.AddAsync(bicchiere);
                await _context.SaveChangesAsync();

                // ✅ 9. AGGIORNA DTO
                bicchiereDto.DimensioneBicchiereId = bicchiere.DimensioneBicchiereId;

                // Carica l'unità di misura per il DTO
                await _context.Entry(bicchiere)
                    .Reference(b => b.UnitaMisura)
                    .LoadAsync();

                // ✅ 10. LOG
                _logger.LogInformation("Bicchiere aggiunto: ID={DimensioneBicchiereId}, Sigla={Sigla}",
                    bicchiere.DimensioneBicchiereId, sigla);

                // ✅ 11. RISPOSTA
                return SingleResponseDTO<DimensioneBicchiereDTO>.SuccessResponse(
                    bicchiereDto,
                    $"Bicchiere '{sigla}' creato con successo (ID: {bicchiere.DimensioneBicchiereId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per bicchiereDto: {@BicchiereDto}", bicchiereDto);
                return SingleResponseDTO<DimensioneBicchiereDTO>.ErrorResponse(
                    "Errore interno durante la creazione del bicchiere");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(DimensioneBicchiereDTO bicchiereDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(bicchiereDto);

                // ✅ 1. Validazioni input obbligatori
                if (string.IsNullOrWhiteSpace(bicchiereDto.Sigla))
                    return SingleResponseDTO<bool>.ErrorResponse("Sigla obbligatoria");

                if (string.IsNullOrWhiteSpace(bicchiereDto.Descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione obbligatoria");

                if (bicchiereDto.UnitaMisuraId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Unità di misura obbligatoria");

                // ✅ 2. Validazioni range/numeriche
                if (bicchiereDto.Capienza < 250m || bicchiereDto.Capienza > 1000m)
                    return SingleResponseDTO<bool>.ErrorResponse("La capienza deve essere tra 250 e 1000");

                if (bicchiereDto.PrezzoBase < 0.01m || bicchiereDto.PrezzoBase > 100m)
                    return SingleResponseDTO<bool>.ErrorResponse("Il prezzo base deve essere tra 0.01 e 100");

                if (bicchiereDto.Moltiplicatore < 0.1m || bicchiereDto.Moltiplicatore > 3.0m)
                    return SingleResponseDTO<bool>.ErrorResponse("Il moltiplicatore deve essere tra 0.1 e 3.0");

                // ✅ 3. Validazione sicurezza SULL'INPUT ORIGINALE
                if (!SecurityHelper.IsValidInput(bicchiereDto.Sigla, 3))
                    return SingleResponseDTO<bool>.ErrorResponse("Sigla non valida o contiene caratteri pericolosi");

                if (!SecurityHelper.IsValidInput(bicchiereDto.Descrizione, 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione non valida o contiene caratteri pericolosi");

                // ✅ 4. SOLO DOPO la validazione, normalizza
                var sigla = StringHelper.NormalizeSearchTerm(bicchiereDto.Sigla);
                var descrizione = StringHelper.NormalizeSearchTerm(bicchiereDto.Descrizione);

                // ✅ 5. Verifica esistenza del bicchiere
                var bicchiere = await _context.DimensioneBicchiere
                    .FirstOrDefaultAsync(b => b.DimensioneBicchiereId == bicchiereDto.DimensioneBicchiereId);

                if (bicchiere == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bicchiere con ID {bicchiereDto.DimensioneBicchiereId} non trovato");

                // ✅ 6. Validazione Unità di Misura esista
                if (!await UnitaMisuraExistsInternalAsync(bicchiereDto.UnitaMisuraId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Unità di misura con ID {bicchiereDto.UnitaMisuraId} non trovata");

                // ✅ 7. Controllo duplicati ESCLUDENDO questo bicchiere
                if (await ExistsSiglaForOtherInternalAsync(bicchiereDto.DimensioneBicchiereId, sigla))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un altro bicchiere con sigla '{sigla}'");

                if (await ExistsDescrizioneForOtherInternalAsync(bicchiereDto.DimensioneBicchiereId, descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un altro bicchiere con descrizione '{descrizione}'");

                // ✅ 8. Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                // Usa EqualsTrimmedCaseInsensitive per maggiore robustezza
                if (!StringHelper.EqualsTrimmedCaseInsensitive(bicchiere.Sigla, sigla))
                {
                    bicchiere.Sigla = SecurityHelper.NormalizeSafe(sigla);
                    hasChanges = true;
                }

                if (!StringHelper.EqualsTrimmedCaseInsensitive(bicchiere.Descrizione, descrizione))
                {
                    bicchiere.Descrizione = descrizione;
                    hasChanges = true;
                }

                if (bicchiere.Capienza != bicchiereDto.Capienza)
                {
                    bicchiere.Capienza = bicchiereDto.Capienza;
                    hasChanges = true;
                }

                if (bicchiere.UnitaMisuraId != bicchiereDto.UnitaMisuraId)
                {
                    bicchiere.UnitaMisuraId = bicchiereDto.UnitaMisuraId;
                    hasChanges = true;
                }

                if (bicchiere.PrezzoBase != bicchiereDto.PrezzoBase)
                {
                    bicchiere.PrezzoBase = bicchiereDto.PrezzoBase;
                    hasChanges = true;
                }

                if (bicchiere.Moltiplicatore != bicchiereDto.Moltiplicatore)
                {
                    bicchiere.Moltiplicatore = bicchiereDto.Moltiplicatore;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Bicchiere aggiornato: ID={Id}, Sigla={Sigla}",
                        bicchiere.DimensioneBicchiereId,
                        sigla);

                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Bicchiere con ID {bicchiereDto.DimensioneBicchiereId} aggiornato con successo");
                }
                else
                {
                    _logger.LogInformation(
                        "Nessuna modifica necessaria per bicchiere: ID={Id}",
                        bicchiere.DimensioneBicchiereId);

                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per il bicchiere con ID {bicchiereDto.DimensioneBicchiereId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per bicchiereDto: {@BicchiereDto}", bicchiereDto);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento del bicchiere");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int bicchiereId)
        {
            try
            {
                if (bicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID bicchiere non valido");

                var bicchiere = await _context.DimensioneBicchiere
                    .FirstOrDefaultAsync(b => b.DimensioneBicchiereId == bicchiereId);

                if (bicchiere == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Bicchiere con ID {bicchiereId} non trovato");

                if (await HasDependenciesAsync(bicchiereId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare il bicchiere perché ci sono dipendenze collegate");

                _context.DimensioneBicchiere.Remove(bicchiere);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                     $"Bicchiere '{bicchiere.Sigla}' eliminato con successo (ID: {bicchiere.DimensioneBicchiereId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per bicchiereId: {BicchiereId}", bicchiereId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione del bicchiere");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int bicchiereId)
        {
            try
            {
                if (bicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID bicchiere non valido");

                var exists = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .AnyAsync(b => b.DimensioneBicchiereId == bicchiereId);

                string message = exists
                    ? $"Bicchiere con ID {bicchiereId} esiste"
                    : $"Bicchiere con ID {bicchiereId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per bicchiereId: {BicchiereId}", bicchiereId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza del bicchiere");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsSiglaAsync(string sigla)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sigla))
                    return SingleResponseDTO<bool>.ErrorResponse("La sigla è obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(sigla, maxLength: 3))
                    return SingleResponseDTO<bool>.ErrorResponse("La sigla contiene caratteri non validi");

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(sigla);

                var exists = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .AnyAsync(b => StringHelper.EqualsCaseInsensitive(b.Sigla, searchTerm));

                string message = exists
                    ? $"Bicchiere con sigla '{searchTerm}' esiste"
                    : $"Bicchiere con sigla '{searchTerm}' non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsSiglaAsync per sigla: {Sigla}", sigla);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza del bicchiere per sigla");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsDescrizioneAsync(string descrizione)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione è obbligatoria");

                // ✅ Validazione sicurezza SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(descrizione, maxLength: 50))
                    return SingleResponseDTO<bool>.ErrorResponse("La descrizione contiene caratteri non validi");

                // ✅ SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizione);

                var exists = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .AnyAsync(b => StringHelper.EqualsCaseInsensitive(b.Descrizione, searchTerm));

                string message = exists
                    ? $"Bicchiere con descrizione '{searchTerm}' esiste"
                    : $"Bicchiere con descrizione '{searchTerm}' non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsDescrizioneAsync per descrizione: {Descrizione}", descrizione);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza del bicchiere per descrizione");
            }
        }
    }
}