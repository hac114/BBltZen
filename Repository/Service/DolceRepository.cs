using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class DolceRepository(BubbleTeaContext context, ILogger<DolceRepository> logger) : IDolceRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<DolceRepository> _logger = logger;

        private static DolceDTO MapToDTO(Dolce dolce)
        {
            return new DolceDTO
            {
                ArticoloId = dolce.ArticoloId,
                Nome = dolce.Nome,
                Prezzo = dolce.Prezzo,
                Descrizione = dolce.Descrizione,
                ImmagineUrl = dolce.ImmagineUrl,
                Disponibile = dolce.Disponibile,
                Priorita = dolce.Priorita,
                DataCreazione = dolce.DataCreazione,
                DataAggiornamento = dolce.DataAggiornamento
            };
        }

        public async Task<PaginatedResponseDTO<DolceDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ ORDINAMENTO CORRETTO: prima per Priorità (decrescente) poi per Nome
                var query = _context.Dolce
                    .AsNoTracking()
                    .OrderByDescending(d => d.Priorita)
                    .ThenBy(d => d.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(d => MapToDTO(d))
                    .ToListAsync();

                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessun dolce trovato",
                        1 => "Trovato 1 dolce",
                        _ => $"Trovati {totalCount} dolci"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync con page={Page}, pageSize={PageSize}", page, pageSize);
                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei dolci"
                };
            }
        }

        public async Task<SingleResponseDTO<DolceDTO>> GetByIdAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("ID dolce non valido");

                var dolce = await _context.Dolce
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

                if (dolce == null)
                    return SingleResponseDTO<DolceDTO>.NotFoundResponse(
                        $"Dolce con ID {articoloId} non trovato");

                return SingleResponseDTO<DolceDTO>.SuccessResponse(
                    MapToDTO(dolce),
                    $"Dolce con ID {articoloId} trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<DolceDTO>.ErrorResponse("Errore interno nel recupero del dolce");
            }
        }

        public async Task<PaginatedResponseDTO<DolceDTO>> GetDisponibiliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ ORDINAMENTO PER PRIORITÀ (come richiesto dal business)
                var query = _context.Dolce
                    .AsNoTracking()
                    .Where(d => d.Disponibile)
                    .OrderByDescending(d => d.Priorita)
                    .ThenBy(d => d.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(d => MapToDTO(d))
                    .ToListAsync();

                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessun dolce disponibile trovato",
                        1 => "Trovato 1 dolce disponibile",
                        _ => $"Trovati {totalCount} dolci disponibili"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetDisponibiliAsync con page={Page}, pageSize={PageSize}", page, pageSize);
                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei dolci disponibili"
                };
            }
        }

        public async Task<PaginatedResponseDTO<DolceDTO>> GetNonDisponibiliAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Dolce
                    .AsNoTracking()
                    .Where(d => !d.Disponibile)
                    .OrderByDescending(d => d.Priorita)
                    .ThenBy(d => d.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(d => MapToDTO(d))
                    .ToListAsync();

                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => "Nessun dolce non disponibile trovato",
                        1 => "Trovato 1 dolce non disponibile",
                        _ => $"Trovati {totalCount} dolci non disponibili"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetNonDisponibiliAsync con page={Page}, pageSize={PageSize}", page, pageSize);
                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei dolci non disponibili"
                };
            }
        }

        public async Task<PaginatedResponseDTO<DolceDTO>> GetByPrioritaAsync(int priorita = 1, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ VALIDAZIONE PRIORITÀ (1-10 come nel vincolo CHECK)
                if (priorita < 1 || priorita > 10)
                {
                    return new PaginatedResponseDTO<DolceDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "La priorità deve essere tra 1 e 10"
                    };
                }

                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Dolce
                    .AsNoTracking()
                    .Where(d => d.Priorita == priorita)
                    .OrderBy(d => d.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(d => MapToDTO(d))
                    .ToListAsync();

                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount switch
                    {
                        0 => $"Nessun dolce con priorità {priorita}",
                        1 => $"Trovato 1 dolce con priorità {priorita}",
                        _ => $"Trovati {totalCount} dolci con priorità {priorita}"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByPrioritaAsync con priorita={Priorita}, page={Page}, pageSize={PageSize}",
                    priorita, page, pageSize);
                return new PaginatedResponseDTO<DolceDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero dei dolci filtrati per priorità"
                };
            }
        }

        private async Task<bool> NomeExistsInternalAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(nome);
            return await _context.Dolce
                .AsNoTracking()
                .AnyAsync(d => StringHelper.EqualsCaseInsensitive(d.Nome, searchTerm));
        }

        public async Task<SingleResponseDTO<DolceDTO>> AddAsync(DolceDTO dolceDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(dolceDto);

                // ✅ VALIDAZIONE NOME
                if (string.IsNullOrWhiteSpace(dolceDto.Nome))
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("Nome obbligatorio");

                if (!SecurityHelper.IsValidInput(dolceDto.Nome, 100))
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("Nome non valido");

                // ✅ VALIDAZIONE PREZZO (corretto: 0.01-99.99 come nel DTO)
                if (dolceDto.Prezzo < 0.01m || dolceDto.Prezzo > 99.99m)
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("Il prezzo deve essere compreso tra 0.01 e 99.99");

                // ✅ VALIDAZIONE DESCRIZIONE
                if (dolceDto.Descrizione != null && !SecurityHelper.IsValidInput(dolceDto.Descrizione, 255))
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("Descrizione non valida");

                // ✅ VALIDAZIONE URL IMMAGINE (corretto: usa ImmagineUrl, non Descrizione)
                if (dolceDto.ImmagineUrl != null && !StringHelper.IsValidUrlInput(dolceDto.ImmagineUrl, 500))
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("URL immagine non valido");

                // ✅ VALIDAZIONE PRIORITÀ
                if (dolceDto.Priorita < 1 || dolceDto.Priorita > 10)
                    return SingleResponseDTO<DolceDTO>.ErrorResponse("La priorità deve essere tra 1 e 10");

                // ✅ NORMALIZZAZIONE NOME (solo per controllo duplicati)
                var nomeNormalizzato = StringHelper.NormalizeSearchTerm(dolceDto.Nome);

                // ✅ CONTROLLO DUPLICATI
                if (await NomeExistsInternalAsync(nomeNormalizzato))
                    return SingleResponseDTO<DolceDTO>.ErrorResponse($"Esiste già un dolce con il nome '{dolceDto.Nome}'");

                // ✅ CREAZIONE ARTICOLO (obbligatorio per FK)
                var articolo = new Articolo
                {
                    Tipo = "D", // Tipo fisso per Dolce
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };

                _context.Articolo.Add(articolo);
                await _context.SaveChangesAsync();

                // ✅ CREAZIONE DOLCE
                var dolce = new Dolce
                {
                    ArticoloId = articolo.ArticoloId,
                    Nome = dolceDto.Nome.Trim(), // Trimma spazi ma mantieni caso
                    Prezzo = dolceDto.Prezzo,
                    Descrizione = dolceDto.Descrizione?.Trim(),
                    ImmagineUrl = dolceDto.ImmagineUrl?.Trim(),
                    Disponibile = dolceDto.Disponibile,
                    Priorita = dolceDto.Priorita,
                    DataCreazione = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };

                await _context.Dolce.AddAsync(dolce);
                await _context.SaveChangesAsync();

                // ✅ AGGIORNA DTO CON ID GENERATO
                dolceDto.ArticoloId = dolce.ArticoloId;

                return SingleResponseDTO<DolceDTO>.SuccessResponse(dolceDto,
                    $"Dolce '{dolceDto.Nome}' creato con successo (ID: {dolce.ArticoloId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per dolceDto: {@DolceDto}", dolceDto);
                return SingleResponseDTO<DolceDTO>.ErrorResponse("Errore interno durante la creazione del dolce");
            }
        }

        private async Task<bool> NomeExistsInternalAsync(string nome, int excludeArticoloId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(nome);
            return await _context.Dolce
                .AsNoTracking()
                .AnyAsync(d => StringHelper.EqualsCaseInsensitive(d.Nome, searchTerm) &&
                    d.ArticoloId != excludeArticoloId);
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(DolceDTO dolceDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(dolceDto);

                // ✅ VALIDAZIONE ID
                if (dolceDto.ArticoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID dolce non valido");

                // ✅ VALIDAZIONE NOME
                if (string.IsNullOrWhiteSpace(dolceDto.Nome))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome obbligatorio");

                if (!SecurityHelper.IsValidInput(dolceDto.Nome, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome non valido");

                // ✅ VALIDAZIONE PREZZO
                if (dolceDto.Prezzo < 0.01m || dolceDto.Prezzo > 99.99m)
                    return SingleResponseDTO<bool>.ErrorResponse("Il prezzo deve essere compreso tra 0.01 e 99.99");

                // ✅ VALIDAZIONE DESCRIZIONE
                if (dolceDto.Descrizione != null && !SecurityHelper.IsValidInput(dolceDto.Descrizione, 255))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione non valida");

                // ✅ VALIDAZIONE URL IMMAGINE (corretto: usa ImmagineUrl)
                if (dolceDto.ImmagineUrl != null && !StringHelper.IsValidUrlInput(dolceDto.ImmagineUrl, 500))
                    return SingleResponseDTO<bool>.ErrorResponse("URL immagine non valido");

                // ✅ VALIDAZIONE PRIORITÀ
                if (dolceDto.Priorita < 1 || dolceDto.Priorita > 10)
                    return SingleResponseDTO<bool>.ErrorResponse("La priorità deve essere tra 1 e 10");

                // ✅ RECUPERO DOLCE ESISTENTE (CERCA SOLO PER ID!) - CORRETTO
                var dolce = await _context.Dolce
                    .FirstOrDefaultAsync(d => d.ArticoloId == dolceDto.ArticoloId);

                if (dolce == null)
                    return SingleResponseDTO<bool>.NotFoundResponse($"Dolce con ID {dolceDto.ArticoloId} non trovato");

                // ✅ NORMALIZZAZIONE NOME PER CONTROLLO DUPLICATI
                var nomeNormalizzato = StringHelper.NormalizeSearchTerm(dolceDto.Nome);

                // ✅ CONTROLLO DUPLICATI (escludendo il record corrente)
                if (await NomeExistsInternalAsync(nomeNormalizzato, dolceDto.ArticoloId))
                    return SingleResponseDTO<bool>.ErrorResponse($"Esiste già un altro dolce con il nome '{dolceDto.Nome}'");

                bool hasChanges = false;

                // ✅ CONFRONTO NOME (case-insensitive e trimmed)
                if (!StringHelper.EqualsTrimmedCaseInsensitive(dolce.Nome, dolceDto.Nome))
                {
                    dolce.Nome = dolceDto.Nome.Trim();
                    hasChanges = true;
                }

                // ✅ CONFRONTO PREZZO
                if (dolce.Prezzo != dolceDto.Prezzo)
                {
                    dolce.Prezzo = dolceDto.Prezzo;
                    hasChanges = true;
                }

                // ✅ CONFRONTO DESCRIZIONE
                var descrizioneNormalizzata = dolceDto.Descrizione?.Trim();
                if (dolce.Descrizione != descrizioneNormalizzata)
                {
                    dolce.Descrizione = descrizioneNormalizzata;
                    hasChanges = true;
                }

                // ✅ CONFRONTO URL IMMAGINE
                var immagineUrlNormalizzata = dolceDto.ImmagineUrl?.Trim();
                if (dolce.ImmagineUrl != immagineUrlNormalizzata)
                {
                    dolce.ImmagineUrl = immagineUrlNormalizzata;
                    hasChanges = true;
                }

                // ✅ CONFRONTO DISPONIBILITÀ
                if (dolce.Disponibile != dolceDto.Disponibile)
                {
                    dolce.Disponibile = dolceDto.Disponibile;
                    hasChanges = true;
                }

                // ✅ CONFRONTO PRIORITÀ
                if (dolce.Priorita != dolceDto.Priorita)
                {
                    dolce.Priorita = dolceDto.Priorita;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    dolce.DataAggiornamento = DateTime.UtcNow;

                    // ✅ AGGIORNA ANCHE L'ARTICOLO ASSOCIATO
                    var articolo = await _context.Articolo
                        .FirstOrDefaultAsync(a => a.ArticoloId == dolceDto.ArticoloId);

                    if (articolo != null)
                    {
                        articolo.DataAggiornamento = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(true,
                        $"Dolce '{dolceDto.Nome}' aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(false,
                        $"Nessuna modifica necessaria per dolce '{dolceDto.Nome}' (ID: {dolceDto.ArticoloId})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per DolceId: {dolceId}", dolceDto?.ArticoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento del dolce");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var dolce = await _context.Dolce
                    .FirstOrDefaultAsync(d => d.ArticoloId == articoloId);

                if (dolce == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Dolce con ArticoloId: {articoloId} non trovato");

                // ✅ ELIMINA DOLCE
                _context.Dolce.Remove(dolce);

                // ✅ ELIMINA ANCHE ARTICOLO ASSOCIATO (orphan removal)
                var articolo = await _context.Articolo
                    .FirstOrDefaultAsync(a => a.ArticoloId == articoloId);

                if (articolo != null)
                    _context.Articolo.Remove(articolo);

                await _context.SaveChangesAsync();

                return SingleResponseDTO<bool>.SuccessResponse(true,
                    $"Dolce con ArticoloId: {articoloId} eliminato con successo");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore DB in DeleteAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Impossibile eliminare a causa di vincoli referenziali. Assicurati che non ci siano dipendenze");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'eliminazione del dolce");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                var exists = await _context.Dolce
                    .AsNoTracking()
                    .AnyAsync(d => d.ArticoloId == articoloId);

                string message = exists
                    ? $"Dolce con ArticoloId {articoloId} esiste"
                    : $"Dolce con ArticoloId {articoloId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per ArticoloId: {ArticoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza del dolce");
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var totalCount = await _context.Dolce
                    .AsNoTracking()
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessun dolce presente",
                    1 => "C'è 1 dolce in totale",
                    _ => $"Ci sono {totalCount} dolci in totale"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei dolci");
            }
        }

        public async Task<SingleResponseDTO<int>> CountDisponibiliAsync()
        {
            try
            {
                var totalCount = await _context.Dolce
                    .AsNoTracking()
                    .Where(d => d.Disponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessun dolce disponibile",
                    1 => "C'è 1 dolce disponibile",
                    _ => $"Ci sono {totalCount} dolci disponibili"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei dolci disponibili");
            }
        }

        public async Task<SingleResponseDTO<int>> CountNonDisponibiliAsync()
        {
            try
            {
                var totalCount = await _context.Dolce
                    .AsNoTracking()
                    .Where(d => !d.Disponibile)
                    .CountAsync();

                string message = totalCount switch
                {
                    0 => "Nessun dolce non disponibile",
                    1 => "C'è 1 dolce non disponibile",
                    _ => $"Ci sono {totalCount} dolci non disponibili"
                };

                return SingleResponseDTO<int>.SuccessResponse(totalCount, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountNonDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio dei dolci non disponibili");
            }
        }

        // ✅ TOGGLE DISPONIBILITÀ
        public async Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID articolo non valido");

                var dolce = await _context.Dolce.FindAsync(articoloId); // ✅ Cambiato nome variabile per chiarezza

                if (dolce == null)
                    return SingleResponseDTO<bool>.NotFoundResponse($"Dolce con ID {articoloId} non trovato");

                var nuovoStato = !dolce.Disponibile;
                dolce.Disponibile = nuovoStato;

                // ✅ AGGIORNA LA DATA DI AGGIORNAMENTO
                dolce.DataAggiornamento = DateTime.UtcNow;

                // ✅ Aggiorna anche l'articolo associato
                var articoloAssociato = await _context.Articolo.FindAsync(articoloId);
                if (articoloAssociato != null)
                {
                    articoloAssociato.DataAggiornamento = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                string stato = nuovoStato ? "disponibile" : "non disponibile";
                return SingleResponseDTO<bool>.SuccessResponse(nuovoStato,
                    $"Dolce {dolce.Nome} (ID: {articoloId}) impostato come {stato}"); // ✅ Corretto riferimento a dolce.Nome
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ToggleDisponibilitaAsync per articoloId: {articoloId}", articoloId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante il cambio di disponibilità");
            }
        }
    }
}