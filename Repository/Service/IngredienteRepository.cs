using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class IngredienteRepository(BubbleTeaContext context, ILogger<IngredienteRepository> logger) : IIngredienteRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<IngredienteRepository> _logger = logger;

        // ✅ METODO PRIVATO PER MAPPING (PATTERN STANDARD)
        private static IngredienteDTO MapToDTO(Ingrediente ingrediente)
        {
            return new IngredienteDTO
            {
                IngredienteId = ingrediente.IngredienteId,
                Nome = ingrediente.Ingrediente1,
                CategoriaId = ingrediente.CategoriaId,
                CategoriaNome = ingrediente.Categoria?.Categoria ?? string.Empty, // ✅ Safe navigation
                PrezzoAggiunto = ingrediente.PrezzoAggiunto,
                Disponibile = ingrediente.Disponibile,
                DataInserimento = ingrediente.DataInserimento,
                DataAggiornamento = ingrediente.DataAggiornamento
            };
        }

        // ✅ GET ALL: Solo per admin - mostra TUTTI gli ingredienti
        public async Task<PaginatedResponseDTO<IngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ CORRETTO
                    .AsNoTracking()
                    .OrderBy(i => i.Categoria.Categoria)
                    .ThenBy(i => i.Ingrediente1);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(i => MapToDTO(i))
                    .ToListAsync();

                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessun ingrediente trovato"
                        : $"Trovato {totalCount} ingredienti"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli ingredienti"
                };
            }
        }

        // ✅ GET BY ID: Cerca per ID indipendentemente dalla disponibilità
        public async Task<SingleResponseDTO<IngredienteDTO>> GetByIdAsync(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SingleResponseDTO<IngredienteDTO>.ErrorResponse("ID ingrediente non valido");

                var ingrediente = await _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ AGGIUNTO Include
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.IngredienteId == ingredienteId);

                if (ingrediente == null)
                    return SingleResponseDTO<IngredienteDTO>.NotFoundResponse(
                        $"Ingrediente con ID {ingredienteId} non trovato");

                return SingleResponseDTO<IngredienteDTO>.SuccessResponse(
                    MapToDTO(ingrediente),
                    $"Ingrediente con ID {ingredienteId} trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per ingredienteId: {ingredienteId}", ingredienteId);
                return SingleResponseDTO<IngredienteDTO>.ErrorResponse(
                    "Errore interno nel recupero dell'ingrediente");
            }
        }

        public async Task<PaginatedResponseDTO<IngredienteDTO>> GetByNomeAsync(string ingrediente, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(ingrediente, maxLength: 50))
                {
                    return new PaginatedResponseDTO<IngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'ingrediente' contiene caratteri non validi"
                    };
                }

                // ✅ 2. SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(ingrediente);

                // ✅ 3. Verifica se è vuoto dopo la normalizzazione
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<IngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'ingrediente' è obbligatorio"
                    };
                }

                // ✅ 4. Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 5. Query con "INIZIA CON" case-insensitive
                var query = _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ CORRETTO
                    .AsNoTracking()
                    .Where(i => i.Ingrediente1 != null &&
                               StringHelper.StartsWithCaseInsensitive(i.Ingrediente1, searchTerm))
                    .OrderBy(i => i.Categoria.Categoria)
                    .ThenBy(i => i.Ingrediente1);

                // ✅ 6. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(i => MapToDTO(i))
                    .ToListAsync();

                // ✅ 7. Messaggio appropriato
                string message = totalCount switch
                {
                    0 => $"Nessun ingrediente trovato con nome che inizia con '{ingrediente}'",
                    1 => $"Trovato 1 ingrediente con nome che inizia con '{ingrediente}'",
                    _ => $"Trovati {totalCount} ingredienti con nome che inizia con '{ingrediente}'",
                };

                return new PaginatedResponseDTO<IngredienteDTO>
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
                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli ingredienti"
                };
            }
        }

        public async Task<PaginatedResponseDTO<IngredienteDTO>> GetByCategoriaAsync(string categoria, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(categoria, maxLength: 50))
                {
                    return new PaginatedResponseDTO<IngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'categoria' contiene caratteri non validi"
                    };
                }

                // ✅ 2. SOLO DOPO la validazione, normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(categoria);

                // ✅ 3. Verifica se è vuoto dopo la normalizzazione
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<IngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'categoria' è obbligatorio"
                    };
                }

                // ✅ 4. Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 5. Query con "INIZIA CON" case-insensitive
                var query = _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ CORRETTO
                    .AsNoTracking()
                    .Where(i => i.Categoria.Categoria != null &&
                               StringHelper.StartsWithCaseInsensitive(i.Categoria.Categoria, searchTerm))
                    .OrderBy(i => i.Ingrediente1);

                // ✅ 6. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(i => MapToDTO(i))
                    .ToListAsync();

                // ✅ 7. Messaggio appropriato (rimosso "message =" duplicato)
                string message = totalCount switch
                {
                    0 => $"Nessun ingrediente trovato appartenente alla categoria che inizia con '{categoria}'",
                    1 => $"Trovato 1 ingrediente appartenente alla categoria che inizia con '{categoria}'",
                    _ => $"Trovati {totalCount} ingredienti appartenenti alla categoria che inizia con '{categoria}'"
                };

                return new PaginatedResponseDTO<IngredienteDTO>
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
                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli ingredienti"
                };
            }
        }

        // ✅ GET DISPONIBILI: Solo ingredienti DISPONIBILI
        public async Task<PaginatedResponseDTO<IngredienteDTO>> GetByDisponibilisync(int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. Validazione paginazione (coerente con GetAllAsync)
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 2. Query semplice (solo aggiunto Where per disponibilità)
                var query = _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ CORRETTO
                    .AsNoTracking()
                    .Where(i => i.Disponibile)
                    .OrderBy(i => i.Categoria.Categoria)
                    .ThenBy(i => i.Ingrediente1);

                // ✅ 3. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(i => MapToDTO(i))
                    .ToListAsync();

                // ✅ 4. Messaggio specifico per disponibili
                string message = totalCount switch
                {
                    0 => "Nessun ingrediente disponibile trovato",
                    1 => "Trovato 1 ingrediente disponibile",
                    _ => $"Trovati {totalCount} ingredienti disponibili",
                };
                return new PaginatedResponseDTO<IngredienteDTO>
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
                // ✅ 5. Gestione errori minimale (coerente)
                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli ingredienti disponibili"
                };
            }
        }

        public async Task<PaginatedResponseDTO<IngredienteDTO>> GetByNonDisponibilisync(int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. Validazione paginazione (coerente con GetAllAsync)
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 2. Query semplice (solo aggiunto Where per disponibilità)
                var query = _context.Ingrediente
                    .Include(i => i.Categoria) // ✅ CORRETTO
                    .AsNoTracking()
                    .Where(i => !i.Disponibile)
                    .OrderBy(i => i.Categoria.Categoria)
                    .ThenBy(i => i.Ingrediente1);

                // ✅ 3. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(i => MapToDTO(i))
                    .ToListAsync();

                // ✅ 4. Messaggio specifico per disponibili
                string message = totalCount switch
                {
                    0 => "Nessun ingrediente non disponibile trovato",
                    1 => "Trovato 1 ingrediente non disponibile",
                    _ => $"Trovati {totalCount} ingredienti non disponibili",
                };

                return new PaginatedResponseDTO<IngredienteDTO>
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
                // ✅ 5. Gestione errori minimale (coerente)
                return new PaginatedResponseDTO<IngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero degli ingredienti non disponibili"
                };
            }
        }

        private async Task<bool> ExistsByNomeInternalAsync(string ingrediente)
        {
            if (string.IsNullOrWhiteSpace(ingrediente))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(ingrediente);
            return await _context.Ingrediente
                .AsNoTracking()
                .AnyAsync(i => StringHelper.EqualsCaseInsensitive(i.Ingrediente1, searchTerm));
        }

        // ✅ CORRETTO: AddAsync ritorna DTO con ID aggiornato
        public async Task<SingleResponseDTO<IngredienteDTO>> AddAsync(IngredienteDTO ingredienteDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(ingredienteDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(ingredienteDto.Nome))
                    return SingleResponseDTO<IngredienteDTO>.ErrorResponse("Nome ingrediente obbligatorio");

                // ✅ NUOVO: Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(ingredienteDto.Nome, 50))
                    return SingleResponseDTO<IngredienteDTO>.ErrorResponse("Nome ingrediente non valido");

                // ✅ Poi normalizza per controllo duplicati
                var searchTerm = StringHelper.NormalizeSearchTerm(ingredienteDto.Nome);

                // ✅ Controllo duplicati (usa metodo interno)
                if (await ExistsByNomeInternalAsync(searchTerm))
                    return SingleResponseDTO<IngredienteDTO>.ErrorResponse($"Esiste già un ingrediente con nome '{ingredienteDto.Nome}'");

                // ✅ Crea entità con nome ORIGINALE
                var ingrediente = new Ingrediente
                {
                    Ingrediente1 = ingredienteDto.Nome, // ✅ Salva originale
                    CategoriaId = ingredienteDto.CategoriaId,
                    PrezzoAggiunto = ingredienteDto.PrezzoAggiunto,
                    Disponibile = ingredienteDto.Disponibile,
                    DataInserimento = DateTime.UtcNow,
                    DataAggiornamento = DateTime.UtcNow
                };

                await _context.Ingrediente.AddAsync(ingrediente);
                await _context.SaveChangesAsync();

                // ✅ Ricarica con categoria per avere tutti i dati
                await _context.Entry(ingrediente)
                    .Reference(i => i.Categoria)
                    .LoadAsync();

                // ✅ Crea DTO di ritorno completo dai dati salvati
                var resultDto = new IngredienteDTO
                {
                    IngredienteId = ingrediente.IngredienteId,
                    Nome = ingrediente.Ingrediente1,
                    CategoriaId = ingrediente.CategoriaId,
                    CategoriaNome = ingrediente.Categoria?.Categoria ?? string.Empty,
                    PrezzoAggiunto = ingrediente.PrezzoAggiunto,
                    Disponibile = ingrediente.Disponibile,
                    DataInserimento = ingrediente.DataInserimento,
                    DataAggiornamento = ingrediente.DataAggiornamento
                };

                return SingleResponseDTO<IngredienteDTO>.SuccessResponse(resultDto,
                    $"Ingrediente '{ingredienteDto.Nome}' creato con successo (ID: {ingrediente.IngredienteId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per ingrediente: {Nome}", ingredienteDto?.Nome);
                return SingleResponseDTO<IngredienteDTO>.ErrorResponse("Errore interno durante la creazione dell'ingrediente");
            }
        }

        private async Task<bool> ExistsByNomeInternalAsync(int excludeId, string ingrediente)
        {
            if (string.IsNullOrWhiteSpace(ingrediente))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(ingrediente);
            return await _context.Ingrediente
                .AsNoTracking()
                .AnyAsync(i => i.IngredienteId != excludeId &&
                              StringHelper.EqualsCaseInsensitive(i.Ingrediente1, searchTerm));
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(IngredienteDTO ingredienteDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(ingredienteDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(ingredienteDto.Nome))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome ingrediente obbligatorio");

                // ✅ NUOVO: Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(ingredienteDto.Nome, 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome ingrediente non valido");

                // ✅ Poi normalizza per controllo duplicati
                var searchTerm = StringHelper.NormalizeSearchTerm(ingredienteDto.Nome);

                var ingrediente = await _context.Ingrediente
                    .FirstOrDefaultAsync(i => i.IngredienteId == ingredienteDto.IngredienteId);

                if (ingrediente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Ingrediente con ID {ingredienteDto.IngredienteId} non trovato");

                // ✅ Controllo duplicati ESCLUDENDO questo ingrediente (usa metodo interno)
                if (await ExistsByNomeInternalAsync(ingredienteDto.IngredienteId, searchTerm))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un altro ingrediente con nome '{ingredienteDto.Nome}'");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                // Confronto case-insensitive con nome ORIGINALE
                if (!StringHelper.EqualsCaseInsensitive(ingrediente.Ingrediente1, ingredienteDto.Nome))
                {
                    ingrediente.Ingrediente1 = ingredienteDto.Nome; // ✅ Salva originale
                    hasChanges = true;
                }

                if (ingrediente.CategoriaId != ingredienteDto.CategoriaId)
                {
                    ingrediente.CategoriaId = ingredienteDto.CategoriaId;
                    hasChanges = true;
                }

                if (ingrediente.PrezzoAggiunto != ingredienteDto.PrezzoAggiunto)
                {
                    ingrediente.PrezzoAggiunto = ingredienteDto.PrezzoAggiunto;
                    hasChanges = true;
                }

                if (ingrediente.Disponibile != ingredienteDto.Disponibile)
                {
                    ingrediente.Disponibile = ingredienteDto.Disponibile;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    ingrediente.DataAggiornamento = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Ingrediente '{ingredienteDto.Nome}' aggiornato con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per l'ingrediente '{ingredienteDto.Nome}' con ID {ingredienteDto.IngredienteId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per ingredienteId: {ingredienteId}", ingredienteDto?.IngredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento dell'ingrediente");
            }
        }

        private async Task<bool> HasDependenciesAsync(int ingredienteId)
        {
            bool hasIngredientiPersonalizzazione = await _context.IngredientiPersonalizzazione
                .AnyAsync(i => i.IngredienteId == ingredienteId);

            bool hasPersonalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AnyAsync(p => p.IngredienteId == ingredienteId);

            return hasIngredientiPersonalizzazione || hasPersonalizzazioneIngrediente;
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int ingredienteId)
        {
            try
            {
                // ✅ Validazione input
                if (ingredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID ingrediente non valido");

                // ✅ Ricerca dell'ingrediente
                var ingrediente = await _context.Ingrediente
                    .FirstOrDefaultAsync(i => i.IngredienteId == ingredienteId);

                if (ingrediente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Ingrediente con ID {ingredienteId} non trovato");

                // ✅ Controllo dipendenze
                if (await HasDependenciesAsync(ingredienteId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare l'ingrediente perché ci sono dipendenze collegate");

                // ✅ Eliminazione
                _context.Ingrediente.Remove(ingrediente);
                await _context.SaveChangesAsync();

                // ✅ Successo con messaggio
                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Ingrediente '{ingrediente.Ingrediente1}' (ID: {ingredienteId}) eliminato con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per ingredienteId: {ingredienteId}", ingredienteId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione dell'ingrediente");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID ingrediente non valido");

                var exists = await _context.Ingrediente
                    .AsNoTracking()
                    .AnyAsync(i => i.IngredienteId == ingredienteId);

                string message = exists
                    ? $"Ingrediente con ID {ingredienteId} esiste"
                    : $"Ingrediente con ID {ingredienteId} non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per ingredienteId: {ingredienteId}", ingredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica esistenza ingrediente");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string ingrediente)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ingrediente))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome dell'ingrediente è obbligatorio");

                // ✅ NUOVO: Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(ingrediente, maxLength: 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome dell'ingrediente contiene caratteri non validi");

                // ✅ Poi normalizza
                var searchTerm = StringHelper.NormalizeSearchTerm(ingrediente);

                var exists = await _context.Ingrediente
                    .AsNoTracking()
                    .AnyAsync(i => StringHelper.EqualsCaseInsensitive(i.Ingrediente1, searchTerm));

                string message = exists
                    ? $"Ingrediente con nome '{ingrediente}' esiste"
                    : $"Ingrediente con nome '{ingrediente}' non trovato";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByNomeAsync per nome ingrediente: {ingrediente}", ingrediente);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza dell'ingrediente");
            }
        }

        // ✅ TOGGLE DISPONIBILITÀ
        public async Task<SingleResponseDTO<bool>> ToggleDisponibilitaAsync(int ingredienteId)
        {
            try
            {
                if (ingredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID ingrediente non valido");

                var ingrediente = await _context.Ingrediente.FindAsync(ingredienteId);
                if (ingrediente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Ingrediente con ID {ingredienteId} non trovato");

                var nuovoStato = !ingrediente.Disponibile;
                ingrediente.Disponibile = nuovoStato;
                await _context.SaveChangesAsync();

                string stato = nuovoStato ? "disponibile" : "non disponibile";
                return SingleResponseDTO<bool>.SuccessResponse(
                    nuovoStato,
                    $"Ingrediente {ingrediente.Ingrediente1} (ID: {ingredienteId}) impostato come {stato}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ToggleDisponibilitaAsync per ingredienteId: {ingredienteId}", ingredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante il cambio di disponibilità");
            }
        }

        // Per statistiche/conteggi rapidi
        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var count = await _context.Ingrediente.AsNoTracking().CountAsync();
                string message = count == 0
                    ? "Nessun ingrediente presente"
                    : count == 1
                        ? "C'è 1 ingrediente in totale"
                        : $"Ci sono {count} ingredienti in totale";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio degli ingredienti");
            }
        }

        public async Task<SingleResponseDTO<int>> CountDisponibiliAsync()
        {
            try
            {
                var count = await _context.Ingrediente
                    .AsNoTracking()
                    .CountAsync(i => i.Disponibile);

                string message = count == 0
                    ? "Nessun ingrediente disponibile"
                    : count == 1
                        ? "C'è 1 ingrediente disponibile"
                        : $"Ci sono {count} ingredienti disponibili";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio degli ingredienti disponibili");
            }
        }

        public async Task<SingleResponseDTO<int>> CountNonDisponibiliAsync()
        {
            try
            {
                var count = await _context.Ingrediente
                    .AsNoTracking()
                    .CountAsync(i => !i.Disponibile);

                string message = count == 0
                    ? "Nessun ingrediente non disponibile"
                    : count == 1
                        ? "C'è 1 ingrediente non disponibile"
                        : $"Ci sono {count} ingredienti non disponibili";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountNonDisponibiliAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio degli ingredienti non disponibili");
            }
        }
    }
}