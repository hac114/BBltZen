using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class PersonalizzazioneCustomRepository(BubbleTeaContext context, ILogger<PersonalizzazioneCustomRepository> logger) : IPersonalizzazioneCustomRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<PersonalizzazioneCustomRepository> _logger = logger;        

        private PersonalizzazioneCustomDTO MapToDTOWithJoin(PersonalizzazioneCustom p)
        {
            return new PersonalizzazioneCustomDTO
            {
                PersCustomId = p.PersCustomId,
                Nome = p.Nome,
                GradoDolcezza = p.GradoDolcezza,
                DimensioneBicchiereId = p.DimensioneBicchiereId,
                DataCreazione = p.DataCreazione,
                DataAggiornamento = p.DataAggiornamento,
                DescrizioneBicchiere = _context.DimensioneBicchiere
                    .Where(db => db.DimensioneBicchiereId == p.DimensioneBicchiereId)
                    .Select(db => db.Descrizione)
                    .FirstOrDefault() ?? ""
            };
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .OrderBy(p => p.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => new PersonalizzazioneCustomDTO
                    {
                        PersCustomId = p.PersCustomId,
                        Nome = p.Nome,
                        GradoDolcezza = p.GradoDolcezza,
                        DimensioneBicchiereId = p.DimensioneBicchiereId,
                        DataCreazione = p.DataCreazione,
                        DataAggiornamento = p.DataAggiornamento,
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == p.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna personalizzazione custom trovata"
                        : $"Trovate {totalCount} personalizzazioni custom"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni custom"
                };
            }
        }

        public async Task<SingleResponseDTO<PersonalizzazioneCustomDTO>> GetByIdAsync(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("ID personalizzazione custom non valido");

                var dto = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(p => p.PersCustomId == persCustomId)
                    .Select(p => new PersonalizzazioneCustomDTO
                    {
                        PersCustomId = p.PersCustomId,
                        Nome = p.Nome,
                        GradoDolcezza = p.GradoDolcezza,
                        DimensioneBicchiereId = p.DimensioneBicchiereId,
                        DataCreazione = p.DataCreazione,
                        DataAggiornamento = p.DataAggiornamento,
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == p.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.NotFoundResponse(
                        $"Personalizzazione custom con ID {persCustomId} non trovata");

                return SingleResponseDTO<PersonalizzazioneCustomDTO>.SuccessResponse(
                    dto,
                    $"Personalizzazione custom con ID {persCustomId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per ID: {persCustomId}", persCustomId);
                return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse(
                    "Errore interno nel recupero della personalizzazione custom");
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetBicchiereByIdAsync(int bicchiereId, int page = 1, int pageSize = 10)
        {
            try
            {
                if (bicchiereId <= 0)
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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

                var query = _context.PersonalizzazioneCustom
                    .Where(p => p.DimensioneBicchiereId == bicchiereId)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(p => p.PersCustomId)
                    .Select(p => MapToDTOWithJoin(p))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom trovata per bicchiere ID '{bicchiereId}'",
                    1 => $"Trovata 1 personalizzazione custom per bicchiere ID '{bicchiereId}'",
                    _ => $"Trovate {totalCount} personalizzazioni custom per bicchiere ID '{bicchiereId}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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
                _logger.LogError(ex, "Errore in GetBicchiereByIdAsync per ID: {bicchiereId}", bicchiereId);
                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni custom per bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetByGradoDolcezzaAsync(byte gradoDolcezza, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ CORRETTO: Range deve essere 1-3 come nel DB
                if (gradoDolcezza < 1 || gradoDolcezza > 3)
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'gradoDolcezza' deve essere compreso tra 1 e 3"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.PersonalizzazioneCustom
                    .Where(p => p.GradoDolcezza == gradoDolcezza)
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(p => p.PersCustomId)
                    .Select(p => MapToDTOWithJoin(p))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom trovata con grado dolcezza '{gradoDolcezza}'",
                    1 => $"Trovata 1 personalizzazione custom con grado dolcezza '{gradoDolcezza}'",
                    _ => $"Trovate {totalCount} personalizzazioni custom con grado dolcezza '{gradoDolcezza}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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
                _logger.LogError(ex, "Errore in GetByGradoDolcezzaAsync per grado dolcezza: {gradoDolcezza}", gradoDolcezza);
                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni custom per grado dolcezza"
                };
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetBicchiereByDescrizioneAsync(string descrizioneBicchiere, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(descrizioneBicchiere, maxLength: 50))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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

                // Ottimizzato: join diretto invece di due query separate
                var bicchieriIds = await _context.DimensioneBicchiere
                    .Where(db => db.Descrizione != null &&
                           StringHelper.ContainsCaseInsensitive(db.Descrizione, searchTerm))
                    .Select(db => db.DimensioneBicchiereId)
                    .ToListAsync();

                if (bicchieriIds.Count == 0)
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                    {
                        Data = [],
                        Page = safePage,
                        PageSize = safePageSize,
                        TotalCount = 0,
                        Message = $"Nessuna personalizzazione custom trovata per descrizione bicchiere che contiene '{searchTerm}'"
                    };
                }

                var query = _context.PersonalizzazioneCustom
                    .Where(p => bicchieriIds.Contains(p.DimensioneBicchiereId))
                    .AsNoTracking();

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .OrderBy(p => p.PersCustomId)
                    .Select(p => MapToDTOWithJoin(p))
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom con descrizione bicchiere che contiene '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione custom con descrizione bicchiere che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni custom con descrizione bicchiere che contiene '{searchTerm}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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
                _logger.LogError(ex, "Errore in GetBicchiereByDescrizioneAsync per descrizioneBicchiere: {descrizioneBicchiere}", descrizioneBicchiere);
                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni custom per descrizione bicchiere"
                };
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneCustomDTO>> GetByNomeAsync(string nome, int page = 1, int pageSize = 10)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nome, maxLength: 100))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome' contiene caratteri non validi"
                    };
                }

                var searchTerm = StringHelper.NormalizeSearchTerm(nome);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome' è obbligatorio"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(p => StringHelper.ContainsCaseInsensitive(p.Nome, searchTerm))
                    .OrderBy(p => p.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => new PersonalizzazioneCustomDTO
                    {
                        PersCustomId = p.PersCustomId,
                        Nome = p.Nome,
                        GradoDolcezza = p.GradoDolcezza,
                        DimensioneBicchiereId = p.DimensioneBicchiereId,
                        DataCreazione = p.DataCreazione,
                        DataAggiornamento = p.DataAggiornamento,
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == p.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom trovata con nome che contiene '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione custom con nome che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni custom con nome che contiene '{searchTerm}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
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
                _logger.LogError(ex, "Errore in GetByNomeAsync per nome: {nome}", nome);
                return new PaginatedResponseDTO<PersonalizzazioneCustomDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni custom per nome"
                };
            }
        }

        public async Task<SingleResponseDTO<PersonalizzazioneCustomDTO>> AddAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneCustomDto);

                if (string.IsNullOrWhiteSpace(personalizzazioneCustomDto.Nome))
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("Il parametro Nome è obbligatorio");

                if (personalizzazioneCustomDto.GradoDolcezza < 1 || personalizzazioneCustomDto.GradoDolcezza > 3)
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("Il parametro GradoDolcezza deve essere compreso tra 1 e 3");

                if (personalizzazioneCustomDto.DimensioneBicchiereId <= 0)
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("Il parametro DimensioneBicchiereId è obbligatorio");

                var personalizzazioneCustom = new PersonalizzazioneCustom
                {
                    Nome = personalizzazioneCustomDto.Nome,
                    GradoDolcezza = personalizzazioneCustomDto.GradoDolcezza,
                    DimensioneBicchiereId = personalizzazioneCustomDto.DimensioneBicchiereId
                };

                await _context.PersonalizzazioneCustom.AddAsync(personalizzazioneCustom);
                await _context.SaveChangesAsync();

                // ✅ Recupera il DTO con la descrizione del bicchiere
                var resultDto = await _context.PersonalizzazioneCustom
                    .Where(p => p.PersCustomId == personalizzazioneCustom.PersCustomId)
                    .Select(p => new PersonalizzazioneCustomDTO
                    {
                        PersCustomId = p.PersCustomId,
                        Nome = p.Nome,
                        GradoDolcezza = p.GradoDolcezza,
                        DimensioneBicchiereId = p.DimensioneBicchiereId,
                        DataCreazione = p.DataCreazione,
                        DataAggiornamento = p.DataAggiornamento,
                        DescrizioneBicchiere = _context.DimensioneBicchiere
                            .Where(db => db.DimensioneBicchiereId == p.DimensioneBicchiereId)
                            .Select(db => db.Descrizione)
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (resultDto == null)
                    return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("Errore nel recupero della personalizzazione creata");

                return SingleResponseDTO<PersonalizzazioneCustomDTO>.SuccessResponse(
                    resultDto,
                    $"Personalizzazione custom creata con successo (ID: {personalizzazioneCustom.PersCustomId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per Nome: {Nome}, GradoDolcezza: {GradoDolcezza} e DimensioneBicchiereId: {DimensioneBicchiereId}",
                    personalizzazioneCustomDto?.Nome, personalizzazioneCustomDto?.GradoDolcezza, personalizzazioneCustomDto?.DimensioneBicchiereId);
                return SingleResponseDTO<PersonalizzazioneCustomDTO>.ErrorResponse("Errore interno durante la creazione della personalizzazione custom");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneCustomDTO personalizzazioneCustomDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneCustomDto);

                if (personalizzazioneCustomDto.PersCustomId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione custom obbligatorio");

                if (string.IsNullOrWhiteSpace(personalizzazioneCustomDto.Nome))
                    return SingleResponseDTO<bool>.ErrorResponse("Il parametro Nome è obbligatorio");

                // ✅ CORRETTO: Range 1-3 come nel DB
                if (personalizzazioneCustomDto.GradoDolcezza < 1 || personalizzazioneCustomDto.GradoDolcezza > 3)
                    return SingleResponseDTO<bool>.ErrorResponse("Il parametro GradoDolcezza deve essere compreso tra 1 e 3");

                if (personalizzazioneCustomDto.DimensioneBicchiereId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("Il parametro DimensioneBicchiereId è obbligatorio");

                var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                    .FirstOrDefaultAsync(p => p.PersCustomId == personalizzazioneCustomDto.PersCustomId);

                if (personalizzazioneCustom == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione custom con ID {personalizzazioneCustomDto.PersCustomId} non trovata");

                bool hasChanges = false;

                // ✅ CORRETTO: Rimuovi il controllo ridondante su PersCustomId
                if (!string.Equals(personalizzazioneCustom.Nome, personalizzazioneCustomDto.Nome, StringComparison.OrdinalIgnoreCase))
                {
                    personalizzazioneCustom.Nome = personalizzazioneCustomDto.Nome;
                    hasChanges = true;
                }

                if (personalizzazioneCustom.GradoDolcezza != personalizzazioneCustomDto.GradoDolcezza)
                {
                    personalizzazioneCustom.GradoDolcezza = personalizzazioneCustomDto.GradoDolcezza;
                    hasChanges = true;
                }

                if (personalizzazioneCustom.DimensioneBicchiereId != personalizzazioneCustomDto.DimensioneBicchiereId)
                {
                    personalizzazioneCustom.DimensioneBicchiereId = personalizzazioneCustomDto.DimensioneBicchiereId;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    // ✅ Aggiungi aggiornamento data (opzionale, il DB ha default GETDATE())
                    personalizzazioneCustom.DataAggiornamento = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Personalizzazione custom ID: {personalizzazioneCustom.PersCustomId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per personalizzazione custom con ID: {personalizzazioneCustom.PersCustomId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ID: {PersCustomId}",
                    personalizzazioneCustomDto?.PersCustomId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della personalizzazione custom");
            }
        }

        private async Task<bool> HasDependenciesAsync(int persCustomId)
        {
            bool hasIngredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                .AnyAsync(i => i.PersCustomId == persCustomId);

            bool hasBevandaCustom = await _context.BevandaCustom
                .AnyAsync(b => b.PersCustomId == persCustomId);

            return hasIngredientiPersonalizzazione || hasBevandaCustom;
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione custom non valido");

                var personalizzazioneCustom = await _context.PersonalizzazioneCustom
                    .FirstOrDefaultAsync(p => p.PersCustomId == persCustomId);

                if (personalizzazioneCustom == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione custom con ID {persCustomId} non trovato");

                if (await HasDependenciesAsync(persCustomId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare la personalizzazione custom perché ci sono dipendenze collegate");

                _context.PersonalizzazioneCustom.Remove(personalizzazioneCustom);
                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Personalizzazione custom '{personalizzazioneCustom.Nome}' (ID: {persCustomId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per persCustomId: {persCustomId}", persCustomId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione della personalizzazione custom");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int persCustomId)
        {
            try
            {
                if (persCustomId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .AnyAsync(p => p.PersCustomId == persCustomId);

                string message = exists
                    ? $"Personalizzazione custom con ID {persCustomId} esiste"
                    : $"Personalizzazione custom con ID {persCustomId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per persCustomId: {persCustomId}",
                    persCustomId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della personalizzazione custom");
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var totalCount = await _context.PersonalizzazioneCustom.AsNoTracking().CountAsync();
                string message = totalCount switch
                {
                    0 => $"Personalizzazione custom non trovata",
                    1 => $"Trovata 1 personalizzazione custom",
                    _ => $"Trovate {totalCount} personalizzazioni custom"

                };                    

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle personalizzazioni custom");
            }
        }

        public async Task<SingleResponseDTO<int>> CountBicchiereByDescrizioneAsync(string descrizioneBicchiere)
        {
            try
            {
                // ✅ Validazione input con SecurityHelper
                if (!SecurityHelper.IsValidInput(descrizioneBicchiere, maxLength: 50))
                    return SingleResponseDTO<int>.ErrorResponse("Il parametro 'descrizioneBicchiere' contiene caratteri non validi");

                // ✅ Normalizzazione con StringHelper
                var searchTerm = StringHelper.NormalizeSearchTerm(descrizioneBicchiere);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SingleResponseDTO<int>.ErrorResponse("Il parametro 'descrizioneBicchiere' è obbligatorio");

                // ✅ Query per ottenere gli ID dei bicchieri che contengono il termine (case-insensitive)
                var bicchieriIds = await _context.DimensioneBicchiere
                    .AsNoTracking()
                    .Where(db => db.Descrizione != null &&
                           StringHelper.ContainsCaseInsensitive(db.Descrizione, searchTerm))
                    .Select(db => db.DimensioneBicchiereId)
                    .ToListAsync();

                // ✅ Conta le personalizzazioni che hanno un DimensioneBicchiereId nella lista
                var totalCount = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(p => bicchieriIds.Contains(p.DimensioneBicchiereId))
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom trovata per descrizione bicchiere che contiene '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione custom per descrizione bicchiere che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni custom per descrizione bicchiere che contiene '{searchTerm}'"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountBicchiereByDescrizioneAsync per descrizioneBicchiere: {descrizioneBicchiere}", descrizioneBicchiere);
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle personalizzazioni custom per descrizione bicchiere");
            }
        }

        public async Task<SingleResponseDTO<int>> CountByGradoDolcezzaAsync(byte gradoDolcezza)
        {
            try
            {                
                if (gradoDolcezza < 1 || gradoDolcezza > 3)
                    return SingleResponseDTO<int>.ErrorResponse("Il parametro 'gradoDolcezza' deve essere compreso tra 1 e 3");

                var totalCount = await _context.PersonalizzazioneCustom
                    .AsNoTracking()
                    .Where(p => p.GradoDolcezza == gradoDolcezza)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione custom trovata con grado dolcezza {gradoDolcezza}",
                    1 => $"Trovata 1 personalizzazione custom con grado dolcezza {gradoDolcezza}",
                    _ => $"Trovate {totalCount} personalizzazioni custom con grado dolcezza {gradoDolcezza}"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountByGradoDolcezzaAsync per grado dolcezza: {gradoDolcezza}", gradoDolcezza);
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle personalizzazioni custom per grado dolcezza");
            }
        }
    }
}