using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class DimensioneQuantitaIngredientiRepository(BubbleTeaContext context, ILogger<DimensioneQuantitaIngredientiRepository> logger) : IDimensioneQuantitaIngredientiRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger _logger = logger;               

        public async Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ MODIFICA: Query separata per evitare Include() in InMemory
                var query = _context.DimensioneQuantitaIngredienti
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                // ✅ MODIFICA: Carica i dati con join separati per evitare problemi di Include()
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(d => d.DimensioneId) // Ordine di default sicuro
                    .Select(d => new DimensioneQuantitaIngredientiDTO
                    {
                        DimensioneId = d.DimensioneId,
                        PersonalizzazioneIngredienteId = d.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = d.DimensioneBicchiereId,
                        Moltiplicatore = d.Moltiplicatore,
                        // ✅ Carica le proprietà di navigazione separatamente
                        NomePersonalizzazione = _context.PersonalizzazioneIngrediente
                            .Where(pi => pi.PersonalizzazioneIngredienteId == d.PersonalizzazioneIngredienteId)
                            .Select(pi => pi.Personalizzazione != null ? pi.Personalizzazione.Nome : "")
                            .FirstOrDefault() ?? "",
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == d.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna dimensione quantità ingredienti trovata"
                        : $"Trovate {totalCount} dimensioni quantità ingredienti"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle dimensioni quantità ingredienti"
                };
            }
        }

        public async Task<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByIdAsync(int dimensioneId)
        {
            try
            {
                if (dimensioneId <= 0)
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse("parametro dimensioneId non valido");

                // ✅ MODIFICA: Query senza Include() per compatibilità con InMemory
                var dimensioneQuantita = await _context.DimensioneQuantitaIngredienti
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DimensioneId == dimensioneId);

                if (dimensioneQuantita == null)
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.NotFoundResponse(
                        $"Dimensione quantità ingredienti con ID {dimensioneId} non trovata");

                // ✅ MODIFICA: Carica le relazioni separatamente
                var nomePersonalizzazione = await _context.PersonalizzazioneIngrediente
                    .Where(pi => pi.PersonalizzazioneIngredienteId == dimensioneQuantita.PersonalizzazioneIngredienteId)
                    .Select(pi => pi.Personalizzazione != null ? pi.Personalizzazione.Nome : "")
                    .FirstOrDefaultAsync() ?? "";

                var descrizioneBicchiere = await _context.DimensioneBicchiere
                    .Where(db => db.DimensioneBicchiereId == dimensioneQuantita.DimensioneBicchiereId)
                    .Select(db => db.Descrizione)
                    .FirstOrDefaultAsync() ?? "";

                var dto = new DimensioneQuantitaIngredientiDTO
                {
                    DimensioneId = dimensioneQuantita.DimensioneId,
                    PersonalizzazioneIngredienteId = dimensioneQuantita.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dimensioneQuantita.DimensioneBicchiereId,
                    Moltiplicatore = dimensioneQuantita.Moltiplicatore,
                    NomePersonalizzazione = nomePersonalizzazione,
                    DescrizioneBicchiere = descrizioneBicchiere
                };

                return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.SuccessResponse(
                    dto,
                    $"Dimensione quantità ingredienti con ID {dimensioneId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per dimensioneId: {dimensioneId}", dimensioneId);
                return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse(
                    "Errore interno nel recupero della dimensione quantità ingredienti");
            }
        }        

        public async Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByBicchiereIdAsync(int bicchiereId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (bicchiereId <= 0)
                {
                    return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'bicchiereId' non è valido"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ MODIFICA: Query base con filtro per bicchiereId
                var query = _context.DimensioneQuantitaIngredienti
                    .Where(d => d.DimensioneBicchiereId == bicchiereId)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                // ✅ MODIFICA: Carica i dati con join separati per evitare problemi di Include()
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(d => d.DimensioneId) // Ordine di default sicuro
                    .Select(d => new DimensioneQuantitaIngredientiDTO
                    {
                        DimensioneId = d.DimensioneId,
                        PersonalizzazioneIngredienteId = d.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = d.DimensioneBicchiereId,
                        Moltiplicatore = d.Moltiplicatore,
                        // ✅ Carica le proprietà di navigazione separatamente
                        NomePersonalizzazione = _context.PersonalizzazioneIngrediente
                            .Where(pi => pi.PersonalizzazioneIngredienteId == d.PersonalizzazioneIngredienteId)
                            .Select(pi => pi.Personalizzazione != null ? pi.Personalizzazione.Nome : "")
                            .FirstOrDefault() ?? "",
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == d.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna dimensione quantità ingredienti trovata con ID '{bicchiereId}'",
                    1 => $"Trovata 1 dimensione quantità ingredienti con ID '{bicchiereId}'",
                    _ => $"Trovate {totalCount} dimensioni quantità ingredienti con ID '{bicchiereId}'"
                };

                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByBicchiereIdAsync per ID: {bicchiereId}", bicchiereId);
                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle dimensioni quantità ingredienti filtrate in base al parametro bicchiereId"
                };
            }
        }        

        public async Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByPersonalizzazioneIngredienteIdAsync(int personalizzazioneIngredienteId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                {
                    return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'personalizzazioneIngredienteId' non è valido"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ MODIFICA: Query base con filtro per personalizzazioneIngredienteId
                var query = _context.DimensioneQuantitaIngredienti
                    .Where(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                // ✅ MODIFICA: Carica i dati con join separati per evitare problemi di Include()
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(d => d.DimensioneId) // Ordine di default sicuro
                    .Select(d => new DimensioneQuantitaIngredientiDTO
                    {
                        DimensioneId = d.DimensioneId,
                        PersonalizzazioneIngredienteId = d.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = d.DimensioneBicchiereId,
                        Moltiplicatore = d.Moltiplicatore,
                        // ✅ Carica le proprietà di navigazione separatamente
                        NomePersonalizzazione = _context.PersonalizzazioneIngrediente
                            .Where(pi => pi.PersonalizzazioneIngredienteId == d.PersonalizzazioneIngredienteId)
                            .Select(pi => pi.Personalizzazione != null ? pi.Personalizzazione.Nome : "")
                            .FirstOrDefault() ?? "",
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == d.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna dimensione quantità ingredienti trovata con parametro personalizzazioneIngredienteId '{personalizzazioneIngredienteId}'",
                    1 => $"Trovata 1 dimensione quantità ingredienti con personalizzazioneIngredienteId '{personalizzazioneIngredienteId}'",
                    _ => $"Trovate {totalCount} dimensioni quantità ingredienti con personalizzazioneIngredienteId '{personalizzazioneIngredienteId}'"
                };

                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPersonalizzazioneIngredienteAsync per personalizzazioneIngredienteId: {personalizzazioneIngredienteId}", personalizzazioneIngredienteId);
                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle dimensioni quantità ingredienti filtrate in base al parametro personalizzazioneIngredienteId"
                };
            }
        }        

        public async Task<PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>> GetByBicchiereDescrizioneAsync(string descrizioneBicchiere, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizioneBicchiere, maxLength: 50))
                {
                    return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizioneBicchiere' contiene caratteri non validi"
                    };
                }

                var searchTerm = StringHelper.NormalizeSearchTerm(descrizioneBicchiere);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'descrizioneBicchiere' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ PRIMA: Trova tutti i bicchieri che corrispondono alla descrizione
                var bicchieriIds = await _context.DimensioneBicchiere
                    .Where(db => db.Descrizione != null &&
                           StringHelper.ContainsCaseInsensitive(db.Descrizione, searchTerm))
                    .Select(db => db.DimensioneBicchiereId)
                    .ToListAsync();

                if (!bicchieriIds.Any())
                {
                    return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                    {
                        Data = [],
                        Page = safePage,
                        PageSize = safePageSize,
                        TotalCount = 0,
                        Message = $"Nessuna dimensione quantità ingredienti con descrizioneBicchiere che contiene '{searchTerm}'"
                    };
                }

                // ✅ MODIFICA: Query base con filtro per lista di bicchieriIds
                var query = _context.DimensioneQuantitaIngredienti
                    .Where(d => bicchieriIds.Contains(d.DimensioneBicchiereId))
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                // ✅ MODIFICA: Carica i dati con join separati per evitare problemi di Include()
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(d => d.DimensioneId) // Ordine di default sicuro
                    .Select(d => new DimensioneQuantitaIngredientiDTO
                    {
                        DimensioneId = d.DimensioneId,
                        PersonalizzazioneIngredienteId = d.PersonalizzazioneIngredienteId,
                        DimensioneBicchiereId = d.DimensioneBicchiereId,
                        Moltiplicatore = d.Moltiplicatore,
                        // ✅ Carica le proprietà di navigazione separatamente
                        NomePersonalizzazione = _context.PersonalizzazioneIngrediente
                            .Where(pi => pi.PersonalizzazioneIngredienteId == d.PersonalizzazioneIngredienteId)
                            .Select(pi => pi.Personalizzazione != null ? pi.Personalizzazione.Nome : "")
                            .FirstOrDefault() ?? "",
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == d.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna dimensione quantità ingredienti con descrizioneBicchiere che contiene '{searchTerm}'",
                    1 => $"Trovata 1 dimensione quantità ingredienti con descrizioneBicchiere che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} dimensioni quantità ingredienti con descrizioneBicchiere che contiene '{searchTerm}'"
                };

                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByBicchiereDescrizioneAsync per descrizioneBicchiere: {descrizioneBicchiere}", descrizioneBicchiere);
                return new PaginatedResponseDTO<DimensioneQuantitaIngredientiDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle dimensioni quantità ingredienti filtrate in base al parametro descrizioneBicchiere"
                };
            }
        }

        private async Task<bool> BicchiereEsisteInPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId, int bicchiereId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .AnyAsync(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId &&
                                d.DimensioneBicchiereId == bicchiereId);
        }

        public async Task<SingleResponseDTO<DimensioneQuantitaIngredientiDTO>> AddAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(dimensioneQuantitaIngredientiDto);

                if (dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse("parametro PersonalizzazioneIngredienteId obbligatorio");

                if (dimensioneQuantitaIngredientiDto.DimensioneBicchiereId <= 0)
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse("parametro DimensioneBicchiereId obbligatorio");

                // ✅ AGGIUNTA: Validazione del moltiplicatore
                if (dimensioneQuantitaIngredientiDto.Moltiplicatore <= 0 || dimensioneQuantitaIngredientiDto.Moltiplicatore > 10)
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse("Il moltiplicatore deve essere maggiore di 0 e minore o uguale a 10");

                if (await BicchiereEsisteInPersonalizzazioneIngredienteAsync(dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId, dimensioneQuantitaIngredientiDto.DimensioneBicchiereId))
                    return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse(
                        $"Esiste già una dimensione quantità ingredienti {dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId} con DimensioneBicchiereId '{dimensioneQuantitaIngredientiDto.DimensioneBicchiereId}'");

                var dimensioneQuantitaIngredienti = new DimensioneQuantitaIngredienti
                {
                    PersonalizzazioneIngredienteId = dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dimensioneQuantitaIngredientiDto.DimensioneBicchiereId,
                    Moltiplicatore = dimensioneQuantitaIngredientiDto.Moltiplicatore
                };

                await _context.DimensioneQuantitaIngredienti.AddAsync(dimensioneQuantitaIngredienti);
                await _context.SaveChangesAsync();

                var resultDto = new DimensioneQuantitaIngredientiDTO
                {
                    DimensioneId = dimensioneQuantitaIngredienti.DimensioneId,
                    PersonalizzazioneIngredienteId = dimensioneQuantitaIngredienti.PersonalizzazioneIngredienteId,
                    DimensioneBicchiereId = dimensioneQuantitaIngredienti.DimensioneBicchiereId,
                    Moltiplicatore = dimensioneQuantitaIngredienti.Moltiplicatore
                };

                return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.SuccessResponse(
                    resultDto,
                    $"Dimensione quantità ingredienti creata con successo (ID: {dimensioneQuantitaIngredienti.DimensioneId})");
            }
            catch (Exception ex)
            {
                // ✅ CORREZIONE: Log migliorato
                _logger.LogError(ex, "Errore in AddAsync per PersonalizzazioneIngredienteId: {PersonalizzazioneIngredienteId} e DimensioneBicchiereId: {DimensioneBicchiereId}",
                    dimensioneQuantitaIngredientiDto?.PersonalizzazioneIngredienteId, dimensioneQuantitaIngredientiDto?.DimensioneBicchiereId);
                return SingleResponseDTO<DimensioneQuantitaIngredientiDTO>.ErrorResponse("Errore interno durante la creazione della dimensione quantità ingredienti");
            }
        }

        private async Task<bool> BicchiereEsisteInPersonalizzazioneIngredienteAsync(int personalizzazioneIngredienteId, int bicchiereId, int escludiDimensioneId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AsNoTracking()
                .AnyAsync(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId &&
                                d.DimensioneBicchiereId == bicchiereId &&
                                d.DimensioneId != escludiDimensioneId);
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(DimensioneQuantitaIngredientiDTO dimensioneQuantitaIngredientiDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(dimensioneQuantitaIngredientiDto);

                if (dimensioneQuantitaIngredientiDto.DimensioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID dimensione quantità ingredienti obbligatorio");

                if (dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("parametro personalizzazioneId obbligatorio");

                if (dimensioneQuantitaIngredientiDto.DimensioneBicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("parametro dimensioneBicchiereId obbligatorio");

                // ✅ AGGIUNTA: Validazione del moltiplicatore
                if (dimensioneQuantitaIngredientiDto.Moltiplicatore <= 0 || dimensioneQuantitaIngredientiDto.Moltiplicatore > 10)
                    return SingleResponseDTO<bool>.ErrorResponse("Il moltiplicatore deve essere maggiore di 0 e minore o uguale a 10");

                var dimensioniQuantitaIngredienti = await _context.DimensioneQuantitaIngredienti
                    .FirstOrDefaultAsync(d => d.DimensioneId == dimensioneQuantitaIngredientiDto.DimensioneId);

                if (dimensioniQuantitaIngredienti == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Dimensione quantità ingredienti con ID {dimensioneQuantitaIngredientiDto.DimensioneId} non trovata");

                if (await BicchiereEsisteInPersonalizzazioneIngredienteAsync(
                    dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId,
                    dimensioneQuantitaIngredientiDto.DimensioneBicchiereId,
                    dimensioneQuantitaIngredientiDto.DimensioneId))
                {
                    // ✅ CORREZIONE: Messaggio corretto
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra dimensione quantità ingredienti con personalizzazioneIngredienteId: {dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId} e dimensioneBicchiereId '{dimensioneQuantitaIngredientiDto.DimensioneBicchiereId}'");
                }

                bool hasChanges = false;

                if (dimensioniQuantitaIngredienti.DimensioneId != dimensioneQuantitaIngredientiDto.DimensioneId)
                {
                    dimensioniQuantitaIngredienti.DimensioneId = dimensioneQuantitaIngredientiDto.DimensioneId;
                    hasChanges = true;
                }

                if (dimensioniQuantitaIngredienti.PersonalizzazioneIngredienteId != dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId)
                {
                    // ✅ CORREZIONE: Assegnazione corretta dal DTO
                    dimensioniQuantitaIngredienti.PersonalizzazioneIngredienteId = dimensioneQuantitaIngredientiDto.PersonalizzazioneIngredienteId;
                    hasChanges = true;
                }

                if (dimensioniQuantitaIngredienti.DimensioneBicchiereId != dimensioneQuantitaIngredientiDto.DimensioneBicchiereId)
                {
                    dimensioniQuantitaIngredienti.DimensioneBicchiereId = dimensioneQuantitaIngredientiDto.DimensioneBicchiereId;
                    hasChanges = true;
                }

                if (dimensioniQuantitaIngredienti.Moltiplicatore != dimensioneQuantitaIngredientiDto.Moltiplicatore)
                {
                    dimensioniQuantitaIngredienti.Moltiplicatore = dimensioneQuantitaIngredientiDto.Moltiplicatore;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Dimensione quantità ingredienti ID: {dimensioniQuantitaIngredienti.DimensioneId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per dimensione quantità ingredienti con ID {dimensioniQuantitaIngredienti.DimensioneId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ID: {dimensioneId}",
                    dimensioneQuantitaIngredientiDto?.DimensioneId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della dimensione quantità ingredienti");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int dimensioneId)
        {
            try
            {
                if (dimensioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("dimensioneId non valido");

                var dimensioneQuantitaIngredienti = await _context.DimensioneQuantitaIngredienti
                    .Include(d => d.DimensioneBicchiere)
                    .FirstOrDefaultAsync(d => d.DimensioneId == dimensioneId);

                if (dimensioneQuantitaIngredienti == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Dimensione quantità ingredienti con ID {dimensioneId} non trovata");

                _context.DimensioneQuantitaIngredienti.Remove(dimensioneQuantitaIngredienti);
                await _context.SaveChangesAsync();

                string successMessage = $"Dimensione quantità ingredienti con ID {dimensioneId} eliminata con successo.";

                return SingleResponseDTO<bool>.SuccessResponse(true, successMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore in DeleteAsync per dimensioneId: {dimensioneId}", dimensioneId);

                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione della dimensione quantità ingredienti");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int dimensioneId)
        {
            try
            {
                if (dimensioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");


                var exists = await _context.DimensioneQuantitaIngredienti
                    .AsNoTracking()
                    .AnyAsync(d => d.DimensioneId == dimensioneId);

                string message = exists
                    ? $"Dimensione quantità ingredienti con ID {dimensioneId} esiste"
                    : $"Dimensione quantità ingredienti con ID {dimensioneId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per dimensioneId: {DimensioneId}",
                    dimensioneId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della dimensione quantità ingredienti");
            }

        }

        public async Task<SingleResponseDTO<bool>> ExistsByCombinazioneAsync(int personalizzazioneIngredienteId, int bicchiereId)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("personalizzazioneIngredienteId non valido");

                if (bicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("dimensioneBicchiereId non valido");

                var exists = await _context.DimensioneQuantitaIngredienti
                    .AsNoTracking()
                    .AnyAsync(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId && d.DimensioneBicchiereId == bicchiereId);

                string message = exists
                    ? $"Dimensione quantità ingredienti con personalizzazioneIngredienteId {personalizzazioneIngredienteId} e con bicchiereId {bicchiereId} esiste"
                    : $"Dimensione quantità ingredienti con personalizzazioneId {personalizzazioneIngredienteId} e con bicchiereId {bicchiereId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByCombinazioneAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId} e per bicchiereId: {bicchiereId}",
                    personalizzazioneIngredienteId, bicchiereId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della dimensione quantità ingredienti");
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var count = await _context.DimensioneQuantitaIngredienti.AsNoTracking().CountAsync();
                string message = count == 0
                    ? "Nessuna dimensione quantità ingredienti presente"
                    : count == 1
                        ? "C'è 1 dimensione quantità ingredienti in totale"
                        : $"Ci sono {count} dimensioni quantità ingredienti in totale";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle dimensioni quantità ingredienti");
            }
        }

        public async Task<SingleResponseDTO<int>> GetCountByPersonalizzazioneIngredientiAsync(int personalizzazioneIngredienteId)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<int>.ErrorResponse("personalizzazioneIngredienteId non valido");

                // ✅ CORREZIONE COMPLETA: Query corretta per contare le DimensioneQuantitaIngredienti
                var count = await _context.DimensioneQuantitaIngredienti
                    .AsNoTracking()
                    .Where(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId)
                    .CountAsync();

                string message = count == 0
                    ? $"Nessun bicchiere presente per la personalizzazione ingrediente ID: '{personalizzazioneIngredienteId}'"
                    : count == 1
                        ? $"C'è 1 bicchiere per la personalizzazione ingrediente '{personalizzazioneIngredienteId}'"
                        : $"Ci sono {count} bicchieri per la personalizzazione ingrediente '{personalizzazioneIngredienteId}'";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCountByPersonalizzazioneIngredientiAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}", personalizzazioneIngredienteId);
                // ✅ CORREZIONE: Ortografia corretta
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei bicchieri contenuti nella personalizzazione ingredienti");
            }
        }
    }
}