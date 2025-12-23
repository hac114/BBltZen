using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class PersonalizzazioneIngredienteRepository(BubbleTeaContext context, ILogger<PersonalizzazioneIngredienteRepository> logger) : IPersonalizzazioneIngredienteRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger _logger = logger;

        private static PersonalizzazioneIngredienteDTO MapToDTO(PersonalizzazioneIngrediente personalizzazioneIngrediente)
        {
            return new PersonalizzazioneIngredienteDTO
            {
                PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                PersonalizzazioneId = personalizzazioneIngrediente.PersonalizzazioneId,
                NomePersonalizzazione = personalizzazioneIngrediente.Personalizzazione?.Nome,    // ✅ Corretto: nullable
                IngredienteId = personalizzazioneIngrediente.IngredienteId,
                NomeIngrediente = personalizzazioneIngrediente.Ingrediente?.Ingrediente1,       // ✅ Corretto: nullable
                Quantita = personalizzazioneIngrediente.Quantita,
                UnitaMisuraId = personalizzazioneIngrediente.UnitaMisuraId,
                UnitaMisura = personalizzazioneIngrediente.UnitaMisura.Descrizione             // ✅ Corretto: nullable
            };
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.PersonalizzazioneIngrediente
                    .Include(p => p.Personalizzazione)
                    .Include(p => p.Ingrediente) // ✅ Aggiunto per avere i nomi
                    .Include(p => p.UnitaMisura)
                    .AsNoTracking()
                    .OrderBy(p => p.Personalizzazione.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => MapToDTO(p))
                    .ToListAsync();

                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna personalizzazione di ingrediente trovata"
                        : $"Trovate {totalCount} personalizzazioni di ingrediente"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni di ingrediente"
                };
            }
        }

        public async Task<SingleResponseDTO<PersonalizzazioneIngredienteDTO>> GetByIdAsync(int personalizzazioneIngredienteId)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse("ID personalizzazione ingrediente non valido");

                // ⚠️ CORREZIONE CRITICA: Stavi cercando per PersonalizzazioneId invece di PersonalizzazioneIngredienteId!
                var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                    .Include(p => p.Personalizzazione)
                    .Include(p => p.Ingrediente) // ✅ Aggiunto per avere il nome
                    .Include(p => p.UnitaMisura)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId); // ✅ Corretto!

                if (personalizzazioneIngrediente == null)
                    return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.NotFoundResponse(
                        $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteId} non trovata");

                return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.SuccessResponse(
                    MapToDTO(personalizzazioneIngrediente),
                    $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}",
                    personalizzazioneIngredienteId); // ✅ Corretto nome parametro log
                return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse(
                    "Errore interno nel recupero della personalizzazione ingrediente");
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetByPersonalizzazioneAsync(string nomePersonalizzazione, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE
                if (!SecurityHelper.IsValidInput(nomePersonalizzazione, maxLength: 50))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome personalizzazione' contiene caratteri non validi"
                    };
                }

                // ✅ 2. Normalizzazione
                var searchTerm = StringHelper.NormalizeSearchTerm(nomePersonalizzazione);

                // ✅ 3. Verifica se è vuoto dopo la normalizzazione
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome personalizzazione' è obbligatorio"
                    };
                }

                // ✅ 4. Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 5. Query con ricerca case-insensitive usando StringHelper
                var query = _context.PersonalizzazioneIngrediente
                    .Include(p => p.Personalizzazione)
                    .Include(p => p.Ingrediente) // ✅ Aggiunto per avere i nomi
                    .Include(p => p.UnitaMisura)
                    .AsNoTracking()
                    .Where(p => p.Personalizzazione != null &&
                           StringHelper.ContainsCaseInsensitive(p.Personalizzazione.Nome, searchTerm))
                    .OrderBy(p => p.Personalizzazione.Nome);

                // ✅ 6. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => MapToDTO(p))
                    .ToListAsync();

                // ✅ 7. Messaggio appropriato
                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione di ingrediente trovata con nome che contiene '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione di ingrediente con nome che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni di ingrediente con nome che contiene '{searchTerm}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
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
                _logger.LogError(ex, "Errore in GetByPersonalizzazioneAsync per nome: {NomePersonalizzazione}", nomePersonalizzazione);
                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni di ingrediente filtrate in base al nome della personalizzazione"
                };
            }
        }


        public async Task<PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>> GetByIngredienteAsync(string ingrediente, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE
                if (!SecurityHelper.IsValidInput(ingrediente, maxLength: 50))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'ingrediente' contiene caratteri non validi"
                    };
                }

                // ✅ 2. Normalizzazione
                var searchTerm = StringHelper.NormalizeSearchTerm(ingrediente);

                // ✅ 3. Verifica se è vuoto dopo la normalizzazione
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
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

                // ✅ 5. ⚠️ CORREZIONE CRITICA: Stavi cercando per nome personalizzazione invece di ingrediente!
                // Query con ricerca case-insensitive sull'ingrediente
                var query = _context.PersonalizzazioneIngrediente
                    .Include(p => p.Personalizzazione)
                    .Include(p => p.Ingrediente) // ✅ Aggiunto per cercare nell'ingrediente
                    .Include(p => p.UnitaMisura)
                    .AsNoTracking()
                    .Where(p => p.Ingrediente != null &&
                           StringHelper.ContainsCaseInsensitive(p.Ingrediente.Ingrediente1, searchTerm))
                    .OrderBy(p => p.Ingrediente.Ingrediente1);

                // ✅ 6. Conteggio e paginazione
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => MapToDTO(p))
                    .ToListAsync();

                // ✅ 7. Messaggio appropriato
                string message = totalCount switch
                {
                    0 => $"Nessuna personalizzazione trovata con ingrediente che contiene '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione con ingrediente che contiene '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni con ingrediente che contiene '{searchTerm}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
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
                _logger.LogError(ex, "Errore in GetByIngredienteAsync per ingrediente: {Ingrediente}", ingrediente);
                return new PaginatedResponseDTO<PersonalizzazioneIngredienteDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni filtrate in base ad un ingrediente"
                };
            }
        }

        private async Task<bool> IngredienteEsisteInPersonalizzazioneAsync(int personalizzazioneId, int ingredienteId)
        {
            return await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .AnyAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                                pi.IngredienteId == ingredienteId);
        }

        public async Task<SingleResponseDTO<PersonalizzazioneIngredienteDTO>> AddAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneIngredienteDto);

                // ✅ Validazioni input
                if (personalizzazioneIngredienteDto.PersonalizzazioneId <= 0)
                    return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse("parametro personalizzazioneId obbligatorio");

                if (personalizzazioneIngredienteDto.IngredienteId <= 0)
                    return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse("parametro ingredienteId obbligatorio");

                // ✅ Controllo duplicati
                if (await IngredienteEsisteInPersonalizzazioneAsync(personalizzazioneIngredienteDto.PersonalizzazioneId, personalizzazioneIngredienteDto.IngredienteId))
                    return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse(
                        $"Esiste già una personalizzazioneId {personalizzazioneIngredienteDto.PersonalizzazioneId} con ingredienteId '{personalizzazioneIngredienteDto.IngredienteId}'");

                var personalizzazioneIngrediente = new PersonalizzazioneIngrediente
                {
                    PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId,
                    IngredienteId = personalizzazioneIngredienteDto.IngredienteId,
                    Quantita = personalizzazioneIngredienteDto.Quantita,
                    UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId
                };

                await _context.PersonalizzazioneIngrediente.AddAsync(personalizzazioneIngrediente);
                await _context.SaveChangesAsync();

                // ✅ Restituisci il DTO con l'ID generato dal database
                var resultDto = new PersonalizzazioneIngredienteDTO
                {
                    PersonalizzazioneIngredienteId = personalizzazioneIngrediente.PersonalizzazioneIngredienteId,
                    PersonalizzazioneId = personalizzazioneIngrediente.PersonalizzazioneId,
                    IngredienteId = personalizzazioneIngrediente.IngredienteId,
                    Quantita = personalizzazioneIngrediente.Quantita,
                    UnitaMisuraId = personalizzazioneIngrediente.UnitaMisuraId
                };

                return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.SuccessResponse(
                    resultDto,
                    $"Personalizzazione ingrediente creata con successo (ID: {personalizzazioneIngrediente.PersonalizzazioneIngredienteId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per PersonalizzazioneIngrediente: {PersonalizzazioneIngrediente}",
                    personalizzazioneIngredienteDto?.IngredienteId);
                return SingleResponseDTO<PersonalizzazioneIngredienteDTO>.ErrorResponse("Errore interno durante la creazione della personalizzazione ingrediente");
            }
        }

        private async Task<bool> IngredienteEsisteInPersonalizzazioneAsync(int personalizzazioneId, int ingredienteId, int escludiPersonalizzazioneIngredienteId)
        {
            return await _context.PersonalizzazioneIngrediente
                .AsNoTracking()
                .AnyAsync(pi => pi.PersonalizzazioneId == personalizzazioneId &&
                                pi.IngredienteId == ingredienteId &&
                                pi.PersonalizzazioneIngredienteId != escludiPersonalizzazioneIngredienteId);
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneIngredienteDTO personalizzazioneIngredienteDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneIngredienteDto);

                // ✅ Validazioni input - AGGIUNTO controllo ID primario
                if (personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione ingrediente obbligatorio");

                if (personalizzazioneIngredienteDto.PersonalizzazioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("parametro personalizzazioneId obbligatorio");

                if (personalizzazioneIngredienteDto.IngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("parametro ingredienteId obbligatorio");

                // ✅ CERCA per ID PRIMARIO (PersonalizzazioneIngredienteId), non per PersonalizzazioneId
                var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneIngredienteId == personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId);

                if (personalizzazioneIngrediente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId} non trovata");

                // ✅ Controllo duplicati - CORRETTO parametro
                if (await IngredienteEsisteInPersonalizzazioneAsync(
                    personalizzazioneIngredienteDto.PersonalizzazioneId,
                    personalizzazioneIngredienteDto.IngredienteId,
                    personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId))
                {
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra personalizzazioneId {personalizzazioneIngredienteDto.PersonalizzazioneId} con ingredienteId '{personalizzazioneIngredienteDto.IngredienteId}'");
                }

                // ✅ Aggiorna solo se ci sono cambiamenti (COMPLETO)
                bool hasChanges = false;

                if (personalizzazioneIngrediente.PersonalizzazioneId != personalizzazioneIngredienteDto.PersonalizzazioneId)
                {
                    personalizzazioneIngrediente.PersonalizzazioneId = personalizzazioneIngredienteDto.PersonalizzazioneId;
                    hasChanges = true;
                }

                if (personalizzazioneIngrediente.IngredienteId != personalizzazioneIngredienteDto.IngredienteId)
                {
                    personalizzazioneIngrediente.IngredienteId = personalizzazioneIngredienteDto.IngredienteId;
                    hasChanges = true;
                }

                if (personalizzazioneIngrediente.Quantita != personalizzazioneIngredienteDto.Quantita)
                {
                    personalizzazioneIngrediente.Quantita = personalizzazioneIngredienteDto.Quantita;
                    hasChanges = true;
                }

                if (personalizzazioneIngrediente.UnitaMisuraId != personalizzazioneIngredienteDto.UnitaMisuraId)
                {
                    personalizzazioneIngrediente.UnitaMisuraId = personalizzazioneIngredienteDto.UnitaMisuraId;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Personalizzazione ingrediente con ID {personalizzazioneIngrediente.PersonalizzazioneIngredienteId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per la personalizzazione ingrediente con ID {personalizzazioneIngredienteDto.PersonalizzazioneIngredienteId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}",
                    personalizzazioneIngredienteDto?.PersonalizzazioneIngredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della personalizzazione ingrediente");
            }
        }

        private async Task<bool> HasDependenciesAsync(int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .AnyAsync(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
        }

        // ✅ Metodo per ottenere il conteggio delle dipendenze (utile per il messaggio)
        private async Task<int> GetDependencyCountAsync(int personalizzazioneIngredienteId)
        {
            return await _context.DimensioneQuantitaIngredienti
                .CountAsync(d => d.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int personalizzazioneIngredienteId, bool forceDelete = false)
        {
            try
            {
                // ✅ Validazione input
                if (personalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("personalizzazioneIngredienteId non valido");

                // ✅ Ricerca della personalizzazione ingrediente
                var personalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                    .Include(p => p.Ingrediente)
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId);

                if (personalizzazioneIngrediente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteId} non trovata");

                // ✅ Controlla se ci sono dipendenze
                bool hasDependencies = await HasDependenciesAsync(personalizzazioneIngredienteId);

                if (hasDependencies && !forceDelete)
                {
                    // ✅ C'è dipendenze MA l'utente non ha forzato la cancellazione
                    string ingredientName = personalizzazioneIngrediente.Ingrediente?.Ingrediente1 ?? "l'ingrediente";
                    int dependencyCount = await GetDependencyCountAsync(personalizzazioneIngredienteId);

                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Impossibile eliminare {ingredientName} perché ha {dependencyCount} configurazioni di dimensione collegate. " +
                        $"Se si procede, verranno eliminate anche le quantità per i diversi bicchieri. " +
                        $"Usare forceDelete=true per confermare l'eliminazione completa.");
                }

                // ✅ Eliminazione (con o senza dipendenze)
                _context.PersonalizzazioneIngrediente.Remove(personalizzazioneIngrediente);
                await _context.SaveChangesAsync();

                // ✅ Messaggio differenziato in base alle dipendenze
                string successMessage = hasDependencies
                    ? $"Personalizzazione ingrediente (ID: {personalizzazioneIngredienteId}) eliminata con successo. " +
                      $"Sono state eliminate anche tutte le quantità associate alle diverse dimensioni di bicchiere."
                    : $"Personalizzazione ingrediente (ID: {personalizzazioneIngredienteId}) eliminata con successo.";

                return SingleResponseDTO<bool>.SuccessResponse(true, successMessage);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.GetType().Name.Contains("SqlException") ?? false)
            {
                // ⚠️ Backup: se per qualche motivo il cascade non funzionasse
                _logger.LogWarning(ex,
                    "Tentativo di eliminazione con dipendenze attive per ID: {PersonalizzazioneIngredienteId}",
                    personalizzazioneIngredienteId);

                return SingleResponseDTO<bool>.ErrorResponse(
                    "Impossibile eliminare: esistono dipendenze attive. " +
                    "Contattare l'amministratore per verificare la configurazione del database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore in DeleteAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}",
                    personalizzazioneIngredienteId);

                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione della personalizzazione ingrediente");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int personalizzazioneIngredienteId)
        {
            try
            {
                if (personalizzazioneIngredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID non valido");

                // ⚠️ CORREZIONE CRITICA: Stavi cercando per IngredienteId invece di PersonalizzazioneIngredienteId!
                var exists = await _context.PersonalizzazioneIngrediente
                    .AsNoTracking()
                    .AnyAsync(p => p.PersonalizzazioneIngredienteId == personalizzazioneIngredienteId); // ✅ Corretto!

                string message = exists
                    ? $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteId} esiste"
                    : $"Personalizzazione ingrediente con ID {personalizzazioneIngredienteId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per personalizzazioneIngredienteId: {PersonalizzazioneIngredienteId}",
                    personalizzazioneIngredienteId); // ✅ Corretto nome parametro log
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della personalizzazione ingrediente");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByPersonalizzazioneAndIngredienteAsync(int personalizzazioneId, int ingredienteId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("personalizzazioneId non valido");

                if (ingredienteId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ingredienteId non valido");

                var exists = await _context.PersonalizzazioneIngrediente
                    .AsNoTracking()
                    .AnyAsync(p => p.PersonalizzazioneId == personalizzazioneId && p.IngredienteId == ingredienteId);

                string message = exists
                    ? $"Personalizzazione ingrediente con personalizzazioneId {personalizzazioneId} e con ingredienteId {ingredienteId} esiste"
                    : $"Personalizzazione ingrediente con personalizzazioneId {personalizzazioneId} e con ingredienteId {ingredienteId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByPersonalizzazioneAndIngredienteAsync per personalizzazioneId: {PersonalizzazioneId} e per ingredienteId: {IngredienteId}",
                    personalizzazioneId, ingredienteId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della personalizzazione ingrediente");
            }
        }

        public async Task<SingleResponseDTO<int>> CountAsync()
        {
            try
            {
                var count = await _context.PersonalizzazioneIngrediente.AsNoTracking().CountAsync();
                string message = count == 0
                    ? "Nessuna personalizzazione ingrediente presente"
                    : count == 1
                        ? "C'è 1 personalizzazione ingrediente in totale"
                        : $"Ci sono {count} personalizzazioni ingrediente in totale";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in CountAsync");
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio delle personalizzazioni ingrediente");
            }
        }

        // Dato il nome di una personalizzazione ne restituisce il numero degli ingredienti
        public async Task<SingleResponseDTO<int>> GetCountByPersonalizzazioneAsync(string nomePersonalizzazione)
        {
            try
            {
                // ✅ Validazione input con SecurityHelper
                if (!SecurityHelper.IsValidInput(nomePersonalizzazione, maxLength: 50))
                    return SingleResponseDTO<int>.ErrorResponse("Nome personalizzazione non valido");

                if (string.IsNullOrWhiteSpace(nomePersonalizzazione))
                    return SingleResponseDTO<int>.ErrorResponse("Nome personalizzazione obbligatorio");

                // ✅ Normalizzazione con StringHelper
                var nomeNormalizzato = StringHelper.NormalizeSearchTerm(nomePersonalizzazione);

                if (string.IsNullOrWhiteSpace(nomeNormalizzato))
                    return SingleResponseDTO<int>.ErrorResponse("Nome personalizzazione non valido dopo la normalizzazione");

                // ✅ Query per ottenere l'ID della personalizzazione dal nome
                var personalizzazione = await _context.Personalizzazione
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        StringHelper.EqualsCaseInsensitive(p.Nome, nomeNormalizzato) ||
                        StringHelper.EqualsTrimmedCaseInsensitive(p.Nome, nomeNormalizzato));

                if (personalizzazione == null)
                    return SingleResponseDTO<int>.NotFoundResponse(
                        $"Nessuna personalizzazione trovata con il nome '{nomeNormalizzato}'");

                // ✅ Conta gli ingredienti associati alla personalizzazione
                var count = await _context.PersonalizzazioneIngrediente
                    .AsNoTracking()
                    .Where(pi => pi.PersonalizzazioneId == personalizzazione.PersonalizzazioneId)
                    .CountAsync();

                string message = count == 0
                    ? $"Nessun ingrediente presente per la personalizzazione '{nomeNormalizzato}'"
                    : count == 1
                        ? $"C'è 1 ingrediente per la personalizzazione '{nomeNormalizzato}'"
                        : $"Ci sono {count} ingredienti per la personalizzazione '{nomeNormalizzato}'";

                return SingleResponseDTO<int>.SuccessResponse(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetCountByPersonalizzazioneAsync per nome: {NomePersonalizzazione}", nomePersonalizzazione);
                return SingleResponseDTO<int>.ErrorResponse("Errore nel conteggio degli ingredienti della personalizzazione");
            }
        }
    }
}