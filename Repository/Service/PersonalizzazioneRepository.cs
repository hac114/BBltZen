using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;

namespace Repository.Service
{
    public class PersonalizzazioneRepository(BubbleTeaContext context, ILogger<PersonalizzazioneRepository> logger) : IPersonalizzazioneRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<PersonalizzazioneRepository> _logger = logger;

        private static PersonalizzazioneDTO MapToDTO(Personalizzazione personalizzazione)
        {
            return new PersonalizzazioneDTO
            {
                PersonalizzazioneId = personalizzazione.PersonalizzazioneId,
                Nome = personalizzazione.Nome,
                Descrizione = personalizzazione.Descrizione,
                DtCreazione = personalizzazione.DtCreazione
            };
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.Personalizzazione
                    .AsNoTracking()
                    .OrderBy(p => p.Nome);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip(skip)
                    .Take(safePageSize)
                    .Select(p => MapToDTO(p))
                    .ToListAsync();

                return new PaginatedResponseDTO<PersonalizzazioneDTO>
                {
                    Data = items,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna personalizzazione trovata"
                        : $"Trovate {totalCount} personalizzazioni"
                };
            }
            catch (Exception)
            {
                return new PaginatedResponseDTO<PersonalizzazioneDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero della personalizzazione"
                };
            }
        }

        public async Task<SingleResponseDTO<PersonalizzazioneDTO>> GetByIdAsync(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("ID personalizzazione non valido");

                var personalizzazione = await _context.Personalizzazione
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneId == personalizzazioneId);

                if (personalizzazione == null)
                    return SingleResponseDTO<PersonalizzazioneDTO>.NotFoundResponse(
                        $"Personalizzazione con ID {personalizzazioneId} non trovata");

                return SingleResponseDTO<PersonalizzazioneDTO>.SuccessResponse(
                    MapToDTO(personalizzazione),
                    $"Personalizzazione con ID {personalizzazioneId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per personalizzazioneId: {personalizzazioneId}", personalizzazioneId);
                return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse(
                    "Errore interno nel recupero della personalizzazione");
            }
        }

        public async Task<PaginatedResponseDTO<PersonalizzazioneDTO>> GetByNomeAsync(string nome, int page = 1, int pageSize = 10)
        {
            try
            {
                // ✅ 1. VALIDAZIONE SICUREZZA SULL'INPUT ORIGINALE (PRIMA)
                if (!SecurityHelper.IsValidInput(nome, maxLength: 50))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome' contiene caratteri non validi"
                    };
                }

                // ✅ 2. SOLO DOPO la validazione, normalizza e converte in maiuscolo
                var searchTerm = StringHelper.NormalizeSearchTerm(nome).ToUpper();

                // ✅ 3. Verifica se è vuoto dopo la normalizzazione
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new PaginatedResponseDTO<PersonalizzazioneDTO>
                    {
                        Data = [],
                        Page = 1,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Message = "Il parametro 'nome' è obbligatorio"
                    };
                }

                // ✅ 4. Validazione paginazione
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                // ✅ 5. Query con "INIZIA CON" usando EF.Functions.Like (case-insensitive)
                var query = _context.Personalizzazione
                    .AsNoTracking()
                    .Where(p => EF.Functions.Like(p.Nome, $"{searchTerm}%"))
                    .OrderBy(p => p.Nome);

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
                    0 => $"Nessuna personalizzazione trovata con nome che inizia con '{searchTerm}'",
                    1 => $"Trovata 1 personalizzazione con nome che inizia con '{searchTerm}'",
                    _ => $"Trovate {totalCount} personalizzazioni con nome che inizia con '{searchTerm}'"
                };

                return new PaginatedResponseDTO<PersonalizzazioneDTO>
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
                return new PaginatedResponseDTO<PersonalizzazioneDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle personalizzazioni"
                };
            }
        }

        private async Task<bool> ExistsByNomeInternalAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(nome).ToUpper();
            return await _context.Personalizzazione
                .AsNoTracking()
                .AnyAsync(p => EF.Functions.Like(p.Nome, searchTerm));
        }

        public async Task<SingleResponseDTO<PersonalizzazioneDTO>> AddAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(personalizzazioneDto.Nome))
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("Nome personalizzazione obbligatorio");

                if (string.IsNullOrWhiteSpace(personalizzazioneDto.Descrizione))
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("Descrizione personalizzazione obbligatorio");

                // ✅ Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(personalizzazioneDto.Nome, 50))
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("Nome personalizzazione non valido");

                if (!SecurityHelper.IsValidInput(personalizzazioneDto.Descrizione, 500))
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("Descrizione personalizzazione non valido");

                // ✅ Poi normalizza e per il nome converte in maiuscolo
                var nomeNormalizzato = StringHelper.NormalizeSearchTerm(personalizzazioneDto.Nome);                

                // ✅ Controllo duplicati (usa metodo interno)
                if (await ExistsByNomeInternalAsync(nomeNormalizzato))
                    return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse(
                        $"Esiste già una personalizzazione con nome '{nomeNormalizzato}'");

                var personalizzazione = new Personalizzazione
                {
                    Nome = nomeNormalizzato,
                    Descrizione = personalizzazioneDto.Descrizione,
                    DtCreazione = DateTime.UtcNow
                };

                await _context.Personalizzazione.AddAsync(personalizzazione);
                await _context.SaveChangesAsync();

                personalizzazioneDto.PersonalizzazioneId = personalizzazione.PersonalizzazioneId;
                personalizzazioneDto.Nome = nomeNormalizzato;                
                personalizzazioneDto.DtCreazione = personalizzazione.DtCreazione;

                return SingleResponseDTO<PersonalizzazioneDTO>.SuccessResponse(
                    personalizzazioneDto,
                    $"Personalizzazione '{nomeNormalizzato}' creata con successo (ID: {personalizzazione.PersonalizzazioneId})");
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("UNIQUE") == true ||
                                                dbEx.InnerException?.Message.Contains("unique") == true)
            {
                _logger.LogError(dbEx, "Violazione vincolo UNIQUE in AddAsync per personalizzazione: {personalizzazione}", personalizzazioneDto?.Nome);
                return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse(
                    $"Esiste già una personalizzazione con nome '{personalizzazioneDto?.Nome}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per personalizzazione: {personalizzazione}", personalizzazioneDto?.Nome);
                return SingleResponseDTO<PersonalizzazioneDTO>.ErrorResponse("Errore interno durante la creazione della personalizzazione");
            }
        }

        private async Task<bool> ExistsByNomeInternalAsync(int excludeId, string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var searchTerm = StringHelper.NormalizeSearchTerm(nome).ToUpper();
            return await _context.Personalizzazione
                .AsNoTracking()
                .AnyAsync(p => p.PersonalizzazioneId != excludeId &&
                              EF.Functions.Like(p.Nome, searchTerm));
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(PersonalizzazioneDTO personalizzazioneDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(personalizzazioneDto);

                // ✅ Validazioni input
                if (string.IsNullOrWhiteSpace(personalizzazioneDto.Nome))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome personalizzazione obbligatorio");

                if (string.IsNullOrWhiteSpace(personalizzazioneDto.Descrizione))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione personalizzazione obbligatorio");

                // ✅ Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(personalizzazioneDto.Nome, 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Nome personalizzazione non valido");

                if (!SecurityHelper.IsValidInput(personalizzazioneDto.Descrizione, 500))
                    return SingleResponseDTO<bool>.ErrorResponse("Descrizione personalizzazione non valido");

                // ✅ Poi normalizza e per il nome converte in maiuscolo
                var nomeNormalizzato = StringHelper.NormalizeSearchTerm(personalizzazioneDto.Nome);                

                var personalizzazione = await _context.Personalizzazione
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneId == personalizzazioneDto.PersonalizzazioneId);

                if (personalizzazione == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione con ID {personalizzazioneDto.PersonalizzazioneId} non trovata");

                // ✅ Controllo duplicati ESCLUDENDO questo ID (usa metodo interno)
                if (await ExistsByNomeInternalAsync(personalizzazioneDto.PersonalizzazioneId, nomeNormalizzato))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Esiste già un'altra personalizzazione con nome '{nomeNormalizzato}'");

                // ✅ Aggiorna solo se ci sono cambiamenti
                bool hasChanges = false;

                if (!StringHelper.EqualsCaseInsensitive(personalizzazione.Nome, nomeNormalizzato))
                {
                    personalizzazione.Nome = nomeNormalizzato;
                    hasChanges = true;
                }

                if (!StringHelper.EqualsCaseInsensitive(personalizzazione.Descrizione, personalizzazioneDto.Descrizione))
                {
                    personalizzazione.Descrizione = personalizzazioneDto.Descrizione;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    return SingleResponseDTO<bool>.SuccessResponse(
                        true,
                        $"Personalizzazione con ID {personalizzazioneDto.PersonalizzazioneId} aggiornata con successo");
                }
                else
                {
                    return SingleResponseDTO<bool>.SuccessResponse(
                        false,
                        $"Nessuna modifica necessaria per la personalizzazione con ID {personalizzazioneDto.PersonalizzazioneId}");
                }
            }           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in UpdateAsync per personalizzazioneId: {personalizzazioneId}", personalizzazioneDto?.PersonalizzazioneId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore interno durante l'aggiornamento della personalizzazione");
            }
        }

        private async Task<bool> HasDependenciesAsync(int personalizzazioneId)
        {
            bool hasBevandaStandard = await _context.BevandaStandard
                .AnyAsync(b => b.PersonalizzazioneId == personalizzazioneId);

            bool hasPersonalizzazioneIngrediente = await _context.PersonalizzazioneIngrediente
                .AnyAsync(p => p.PersonalizzazioneId == personalizzazioneId);

            return hasBevandaStandard || hasPersonalizzazioneIngrediente;
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int personalizzazioneId)
        {
            try
            {
                // ✅ Validazione input
                if (personalizzazioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione non valido");

                // ✅ Ricerca della personalizzazione
                var personalizzazione = await _context.Personalizzazione
                    .FirstOrDefaultAsync(p => p.PersonalizzazioneId == personalizzazioneId);

                if (personalizzazione == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Personalizzazione con ID {personalizzazioneId} non trovata");

                // ✅ Controllo dipendenze
                if (await HasDependenciesAsync(personalizzazioneId))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare la personalizzazione perché ci sono dipendenze (BevandaStandard o PersonalizzazioneIngrediente)");

                // ✅ Eliminazione
                _context.Personalizzazione.Remove(personalizzazione);
                await _context.SaveChangesAsync();

                // ✅ Successo con messaggio
                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Personalizzazione '{personalizzazione.Nome}' (ID: {personalizzazioneId}) eliminata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in DeleteAsync per personalizzazioneId: {personalizzazioneId}", personalizzazioneId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione della personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int personalizzazioneId)
        {
            try
            {
                if (personalizzazioneId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID personalizzazione non valido");

                var exists = await _context.Personalizzazione
                    .AsNoTracking()
                    .AnyAsync(p => p.PersonalizzazioneId == personalizzazioneId);

                string message = exists
                    ? $"Personalizzazione con ID {personalizzazioneId} esiste"
                    : $"Personalizzazione con ID {personalizzazioneId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per personalizzazioneId: {personalizzazioneId}", personalizzazioneId);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della personalizzazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByNomeAsync(string nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome della personalizzazione è obbligatorio");

                // ✅ Valida PRIMA l'input originale
                if (!SecurityHelper.IsValidInput(nome, maxLength: 50))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome della personalizzazione contiene caratteri non validi");

                // ✅ Poi normalizza e converte in maiuscolo
                var searchTerm = StringHelper.NormalizeSearchTerm(nome).ToUpper();

                var exists = await _context.Personalizzazione
                    .AsNoTracking()
                    .AnyAsync(p => EF.Functions.Like(p.Nome, searchTerm));

                string message = exists
                    ? $"Personalizzazione con nome '{searchTerm}' esiste"
                    : $"Personalizzazione con nome '{searchTerm}' non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByNomeAsync per personalizzazione: {nome}", nome);
                return SingleResponseDTO<bool>.ErrorResponse("Errore nella verifica dell'esistenza della personalizzazione");
            }
        }
    }
}