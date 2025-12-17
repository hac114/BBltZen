using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Helper;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class ConfigSoglieTempiRepository(BubbleTeaContext context, ILogger<ConfigSoglieTempiRepository> logger) : IConfigSoglieTempiRepository
    {
        private readonly BubbleTeaContext _context = context;
        private readonly ILogger<ConfigSoglieTempiRepository> _logger = logger;

        // ========== METODI PRIVATI ==========

        private static ConfigSoglieTempiDTO MapToDTO(ConfigSoglieTempi configSoglieTempi)
        {
            return new ConfigSoglieTempiDTO
            {
                SogliaId = configSoglieTempi.SogliaId,
                StatoOrdineId = configSoglieTempi.StatoOrdineId,
                StatoOrdine = configSoglieTempi.StatoOrdine != null ?
                    new StatoOrdineDTO
                    {
                        StatoOrdineId = configSoglieTempi.StatoOrdine.StatoOrdineId,
                        StatoOrdine1 = configSoglieTempi.StatoOrdine.StatoOrdine1,
                        Terminale = configSoglieTempi.StatoOrdine.Terminale
                    } : null,
                SogliaAttenzione = configSoglieTempi.SogliaAttenzione,
                SogliaCritico = configSoglieTempi.SogliaCritico,
                DataAggiornamento = configSoglieTempi.DataAggiornamento,
                UtenteAggiornamento = configSoglieTempi.UtenteAggiornamento
            };
        }

        //private async Task<bool> ExistsByStatoOrdineInternalAsync(int statoOrdineId)
        //{
        //    return await _context.ConfigSoglieTempi
        //        .AsNoTracking()
        //        .AnyAsync(c => c.StatoOrdineId == statoOrdineId);
        //}

        private async Task<bool> ExistsByStatoOrdineInternalAsync(int statoOrdineId, int excludeId)
        {
            return await _context.ConfigSoglieTempi
                .AsNoTracking()
                .AnyAsync(c => c.StatoOrdineId == statoOrdineId && c.SogliaId != excludeId);
        }

        private async Task<StatoOrdine?> FindStatoOrdineByNomeAsync(string nomeStato)
        {
            if (string.IsNullOrWhiteSpace(nomeStato))
                return null;

            var termineRicerca = StringHelper.NormalizeSearchTerm(
                SecurityHelper.NormalizeSafe(nomeStato)
            );

            if (!SecurityHelper.IsValidInput(termineRicerca, 100))
                return null;

            return await _context.StatoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(so =>
                    EF.Functions.Like(so.StatoOrdine1.Trim().ToUpper(), termineRicerca.ToUpper()));
        }

        // ✅ NUOVO: Verifica se stato ordine è terminale
        private async Task<bool> IsStatoOrdineTerminaleAsync(int statoOrdineId)
        {
            var stato = await _context.StatoOrdine
                .AsNoTracking()
                .FirstOrDefaultAsync(so => so.StatoOrdineId == statoOrdineId);

            return stato?.Terminale ?? true; // Se non trovato, consideralo terminale (sicurezza)
        }

        // ========== METODI PUBBLICI ==========

        public async Task<PaginatedResponseDTO<ConfigSoglieTempiDTO>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (safePage, safePageSize) = SecurityHelper.ValidatePagination(page, pageSize);
                var skip = (safePage - 1) * safePageSize;

                var query = _context.ConfigSoglieTempi.AsNoTracking();
                var totalCount = await query.CountAsync();

                var entities = await query
                    .Include(c => c.StatoOrdine)
                    .OrderBy(c => c.SogliaId)
                    .ThenBy(c => c.StatoOrdineId)
                    .Skip(skip)
                    .Take(safePageSize)
                    .ToListAsync();

                var data = entities.Select(c => MapToDTO(c)).ToList();

                return new PaginatedResponseDTO<ConfigSoglieTempiDTO>
                {
                    Data = data,
                    Page = safePage,
                    PageSize = safePageSize,
                    TotalCount = totalCount,
                    Message = totalCount == 0
                        ? "Nessuna configurazione soglie tempo trovata"
                        : $"Trovate {totalCount} configurazioni soglie tempo"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetAllAsync");
                return new PaginatedResponseDTO<ConfigSoglieTempiDTO>
                {
                    Data = [],
                    Page = 1,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Message = "Errore nel recupero delle configurazioni soglie tempo"
                };
            }
        }

        public async Task<SingleResponseDTO<ConfigSoglieTempiDTO>> GetByIdAsync(int sogliaId)
        {
            try
            {
                if (sogliaId <= 0)
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse("ID configurazione soglia tempo non valido");

                var configSoglieTempi = await _context.ConfigSoglieTempi
                    .AsNoTracking()
                    .Include(c => c.StatoOrdine)
                    .FirstOrDefaultAsync(c => c.SogliaId == sogliaId);

                if (configSoglieTempi == null)
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.NotFoundResponse(
                        $"Configurazione soglia tempo con ID {sogliaId} non trovata");

                return SingleResponseDTO<ConfigSoglieTempiDTO>.SuccessResponse(
                    MapToDTO(configSoglieTempi),
                    $"Configurazione soglia tempo con ID {sogliaId} trovata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByIdAsync per sogliaId: {sogliaId}", sogliaId);
                return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                    "Errore interno nel recupero della configurazione soglia tempo");
            }
        }

        public async Task<SingleResponseDTO<ConfigSoglieTempiDTO>> GetByStatoOrdineAsync(string nomeStatoOrdine)
        {
            try
            {
                if (!SecurityHelper.IsValidInput(nomeStatoOrdine, maxLength: 100))
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoOrdine' contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(nomeStatoOrdine);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                        "Il parametro 'nomeStatoOrdine' è obbligatorio");

                var config = await _context.ConfigSoglieTempi
                    .AsNoTracking()
                    .Include(c => c.StatoOrdine)
                    .Where(c => EF.Functions.Like(c.StatoOrdine.StatoOrdine1.Trim().ToUpper(), searchTerm.ToUpper()))
                    .Select(c => MapToDTO(c))
                    .FirstOrDefaultAsync();

                if (config == null)
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.NotFoundResponse(
                        $"Nessuna configurazione trovata per stato ordine '{searchTerm}'");

                return SingleResponseDTO<ConfigSoglieTempiDTO>.SuccessResponse(
                    config,
                    $"Configurazione per stato ordine '{searchTerm}' trovata con successo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in GetByStatoOrdineAsync per nomeStatoOrdine: {nomeStatoOrdine}", nomeStatoOrdine);
                return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                    "Errore interno nel recupero della configurazione per stato ordine");
            }
        }

        //public async Task<SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>> GetSoglieByStatiOrdineAsync(IEnumerable<int> statiOrdineIds)
        //{
        //    try
        //    {
        //        if (statiOrdineIds == null || !statiOrdineIds.Any())
        //            return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
        //                "Lista stati ordine vuota o nulla");

        //        if (statiOrdineIds.Any(id => id <= 0))
        //            return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
        //                "Uno o più ID stati ordine non validi (<= 0)");

        //        var soglie = await _context.ConfigSoglieTempi
        //            .AsNoTracking()
        //            .Include(c => c.StatoOrdine)
        //            .Where(c => statiOrdineIds.Contains(c.StatoOrdineId))
        //            .ToListAsync();

        //        var dictionary = soglie
        //            .Select(c => MapToDTO(c))
        //            .ToDictionary(s => s.StatoOrdineId, s => s);

        //        return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.SuccessResponse(
        //            dictionary,
        //            $"Trovate {dictionary.Count} configurazioni per {statiOrdineIds.Count()} stati ordine");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex,
        //            "Errore in GetSoglieByStatiOrdineAsync per stati: {StatiOrdineIds}",
        //            string.Join(", ", statiOrdineIds));
        //        return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
        //            "Errore interno durante il recupero delle soglie");
        //    }
        //}

        public async Task<SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>> GetSoglieByStatiOrdineAsync(IEnumerable<int> statiOrdineIds)
        {
            try
            {
                if (statiOrdineIds == null || !statiOrdineIds.Any())
                    return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
                        "La lista degli stati ordine non può essere vuota");

                var idsList = statiOrdineIds.ToList();

                if (idsList.Any(id => id <= 0))
                    return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
                        "Uno o più ID stati ordine non sono validi");

                var soglie = await _context.ConfigSoglieTempi
                    .AsNoTracking()
                    .Include(c => c.StatoOrdine)
                    .Where(c => idsList.Contains(c.StatoOrdineId))
                    .ToListAsync();

                // Verifica se ci sono duplicati (dovrebbe essere impossibile con il vincolo UNIQUE)
                var duplicates = soglie
                    .GroupBy(c => c.StatoOrdineId)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicates.Any())
                {
                    _logger.LogWarning(
                        "Trovati duplicati per StatoOrdineId: {DuplicateIds}. Vincolo UNIQUE violato.",
                        string.Join(", ", duplicates.Select(d => d.Key)));

                    // Per sicurezza, prendi il primo record di ogni duplicato
                    soglie = soglie
                        .GroupBy(c => c.StatoOrdineId)
                        .Select(g => g.First())
                        .ToList();
                }

                var dictionary = soglie
                    .Select(c => MapToDTO(c))
                    .ToDictionary(s => s.StatoOrdineId, s => s);

                var statiTrovati = dictionary.Keys.ToList();
                var statiMancanti = idsList.Except(statiTrovati).ToList();

                var message = $"Trovate {dictionary.Count} configurazioni";
                if (statiMancanti.Any())
                {
                    message += $". Attenzione: {statiMancanti.Count} stati senza configurazione: {string.Join(", ", statiMancanti)}";
                }

                return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.SuccessResponse(
                    dictionary, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore in GetSoglieByStatiOrdineAsync per stati: {StatiOrdineIds}",
                    string.Join(", ", statiOrdineIds));
                return SingleResponseDTO<Dictionary<int, ConfigSoglieTempiDTO>>.ErrorResponse(
                    "Errore interno durante il recupero delle soglie");
            }
        }

        public async Task<SingleResponseDTO<ConfigSoglieTempiDTO>> AddAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(configSoglieTempiDto);

                if (string.IsNullOrWhiteSpace(configSoglieTempiDto.UtenteAggiornamento))
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse("Utente aggiornamento obbligatorio");

                var utenteNormalizzato = SecurityHelper.NormalizeSafe(configSoglieTempiDto.UtenteAggiornamento);
                if (!SecurityHelper.IsValidInput(utenteNormalizzato, 100))
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse("Utente aggiornamento non valido");

                var validationResults = configSoglieTempiDto.Validate(new ValidationContext(configSoglieTempiDto));
                if (validationResults.Any())
                {
                    var errorMessage = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(errorMessage);
                }

                if (configSoglieTempiDto.StatoOrdine == null ||
                    string.IsNullOrWhiteSpace(configSoglieTempiDto.StatoOrdine.StatoOrdine1))
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse("Nome stato ordine obbligatorio");

                var statoOrdine = await FindStatoOrdineByNomeAsync(configSoglieTempiDto.StatoOrdine.StatoOrdine1);

                if (statoOrdine == null)
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                        $"Stato ordine '{configSoglieTempiDto.StatoOrdine.StatoOrdine1}' non trovato");

                // ✅ NUOVO CONTROLLO usando IsStatoOrdineTerminaleAsync
                if (await IsStatoOrdineTerminaleAsync(statoOrdine.StatoOrdineId))
                {
                    return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                        $"Impossibile configurare soglie per lo stato '{statoOrdine.StatoOrdine1}' perché è terminale");
                }

                var configSoglieTempi = new ConfigSoglieTempi
                {
                    StatoOrdineId = statoOrdine.StatoOrdineId,
                    SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione,
                    SogliaCritico = configSoglieTempiDto.SogliaCritico,
                    UtenteAggiornamento = utenteNormalizzato,
                    DataAggiornamento = DateTime.UtcNow
                };

                await _context.ConfigSoglieTempi.AddAsync(configSoglieTempi);
                await _context.SaveChangesAsync();

                var configCompleta = await _context.ConfigSoglieTempi
                    .Include(c => c.StatoOrdine)
                    .FirstOrDefaultAsync(c => c.SogliaId == configSoglieTempi.SogliaId);

                var risultatoDto = MapToDTO(configCompleta!);

                return SingleResponseDTO<ConfigSoglieTempiDTO>.SuccessResponse(
                    risultatoDto,
                    $"Configurazione soglie creata con successo per stato '{statoOrdine.StatoOrdine1}' (ID: {configSoglieTempi.SogliaId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in AddAsync per configurazione soglie: {statoOrdine}",
                    configSoglieTempiDto?.StatoOrdine?.StatoOrdine1);
                return SingleResponseDTO<ConfigSoglieTempiDTO>.ErrorResponse(
                    "Errore interno durante la creazione della configurazione soglie");
            }
        }

        public async Task<SingleResponseDTO<bool>> UpdateAsync(ConfigSoglieTempiDTO configSoglieTempiDto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(configSoglieTempiDto);

                if (configSoglieTempiDto.SogliaId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID configurazione non valido");

                if (string.IsNullOrWhiteSpace(configSoglieTempiDto.UtenteAggiornamento))
                    return SingleResponseDTO<bool>.ErrorResponse("Utente aggiornamento obbligatorio");

                var utenteNormalizzato = SecurityHelper.NormalizeSafe(configSoglieTempiDto.UtenteAggiornamento);
                if (!SecurityHelper.IsValidInput(utenteNormalizzato, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Utente aggiornamento non valido");

                var validationErrors = configSoglieTempiDto.Validate(new ValidationContext(configSoglieTempiDto));
                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("; ", validationErrors.Select(vr => vr.ErrorMessage));
                    return SingleResponseDTO<bool>.ErrorResponse(errorMessage);
                }

                var configEsistente = await _context.ConfigSoglieTempi
                    .Include(c => c.StatoOrdine)
                    .FirstOrDefaultAsync(c => c.SogliaId == configSoglieTempiDto.SogliaId);

                if (configEsistente == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Configurazione ID {configSoglieTempiDto.SogliaId} non trovata");

                // ✅ NUOVO CONTROLLO usando IsStatoOrdineTerminaleAsync
                if (await IsStatoOrdineTerminaleAsync(configEsistente.StatoOrdineId))
                {
                    return SingleResponseDTO<bool>.ErrorResponse(
                        $"Impossibile aggiornare la configurazione perché lo stato '{configEsistente.StatoOrdine.StatoOrdine1}' è terminale");
                }

                int nuovoStatoOrdineId = configEsistente.StatoOrdineId;

                if (configSoglieTempiDto.StatoOrdine != null &&
                    !string.IsNullOrWhiteSpace(configSoglieTempiDto.StatoOrdine.StatoOrdine1))
                {
                    var nuovoStato = await FindStatoOrdineByNomeAsync(configSoglieTempiDto.StatoOrdine.StatoOrdine1);

                    if (nuovoStato == null)
                        return SingleResponseDTO<bool>.ErrorResponse(
                            $"Stato ordine '{configSoglieTempiDto.StatoOrdine.StatoOrdine1}' non trovato");

                    // ✅ NUOVO CONTROLLO: Verifica che il nuovo stato NON sia terminale
                    if (nuovoStato.Terminale)
                    {
                        return SingleResponseDTO<bool>.ErrorResponse(
                            $"Impossibile assegnare la configurazione allo stato '{nuovoStato.StatoOrdine1}' perché è terminale");
                    }

                    if (nuovoStato.StatoOrdineId != configEsistente.StatoOrdineId)
                    {
                        bool conflitto = await ExistsByStatoOrdineInternalAsync(
                            nuovoStato.StatoOrdineId,
                            configSoglieTempiDto.SogliaId
                        );

                        if (conflitto)
                            return SingleResponseDTO<bool>.ErrorResponse(
                                $"Esiste già una configurazione per lo stato '{nuovoStato.StatoOrdine1}'");

                        nuovoStatoOrdineId = nuovoStato.StatoOrdineId;
                    }
                }

                configEsistente.StatoOrdineId = nuovoStatoOrdineId;
                configEsistente.SogliaAttenzione = configSoglieTempiDto.SogliaAttenzione;
                configEsistente.SogliaCritico = configSoglieTempiDto.SogliaCritico;
                configEsistente.UtenteAggiornamento = utenteNormalizzato;
                configEsistente.DataAggiornamento = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Configurazione aggiornata - ID: {SogliaId}, Stato: {StatoOrdineId}, Utente: {Utente}",
                    configEsistente.SogliaId,
                    configEsistente.StatoOrdineId,
                    utenteNormalizzato
                );

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Configurazione ID {configEsistente.SogliaId} aggiornata con successo"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                return SingleResponseDTO<bool>.ErrorResponse(
                    "La configurazione è stata modificata da un altro utente. Ricaricare e riprovare.");
            }
            catch (DbUpdateException)
            {
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore database. Verificare i vincoli di integrità.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore in UpdateAsync per ID: {SogliaId}",
                    configSoglieTempiDto?.SogliaId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'aggiornamento");
            }
        }

        public async Task<SingleResponseDTO<bool>> DeleteAsync(int sogliaId, string utenteRichiedente)
        {
            try
            {
                if (sogliaId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID configurazione non valido");

                if (string.IsNullOrWhiteSpace(utenteRichiedente))
                    return SingleResponseDTO<bool>.ErrorResponse("Utente richiedente obbligatorio");

                var utenteNormalizzato = SecurityHelper.NormalizeSafe(utenteRichiedente);
                if (!SecurityHelper.IsValidInput(utenteNormalizzato, 100))
                    return SingleResponseDTO<bool>.ErrorResponse("Utente richiedente non valido");

                var config = await _context.ConfigSoglieTempi
                    .FirstOrDefaultAsync(c => c.SogliaId == sogliaId);

                if (config == null)
                    return SingleResponseDTO<bool>.NotFoundResponse(
                        $"Configurazione con ID {sogliaId} non trovata");

                _context.ConfigSoglieTempi.Remove(config);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Configurazione eliminata - ID: {SogliaId}, StatoOrdineId: {StatoOrdineId}, Utente: {Utente}",
                    sogliaId, config.StatoOrdineId, utenteNormalizzato);

                return SingleResponseDTO<bool>.SuccessResponse(
                    true,
                    $"Configurazione ID {sogliaId} eliminata con successo"
                );
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx,
                    "Database error in DeleteAsync per ID: {SogliaId}",
                    sogliaId);

                if (dbEx.InnerException?.Message.Contains("FOREIGN KEY") == true ||
                    dbEx.InnerException?.Message.Contains("REFERENCE") == true)
                {
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Impossibile eliminare: la configurazione è referenziata da altri dati.");
                }

                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore database durante l'eliminazione");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Errore in DeleteAsync per ID: {SogliaId}, Utente: {Utente}",
                    sogliaId, utenteRichiedente);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore interno durante l'eliminazione");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsAsync(int sogliaId)
        {
            try
            {
                if (sogliaId <= 0)
                    return SingleResponseDTO<bool>.ErrorResponse("ID configurazione soglia tempo non valida");

                var exists = await _context.ConfigSoglieTempi
                    .AsNoTracking()
                    .AnyAsync(c => c.SogliaId == sogliaId);

                string message = exists
                    ? $"Configurazione soglia tempo con ID {sogliaId} esiste"
                    : $"Configurazione soglia tempo con ID {sogliaId} non trovata";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsAsync per sogliaId: {sogliaId}", sogliaId);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza della configurazione della soglia tempo");
            }
        }

        public async Task<SingleResponseDTO<bool>> ExistsByStatoOrdine(string statoOrdine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statoOrdine))
                    return SingleResponseDTO<bool>.ErrorResponse("Il nome dello stato ordine è obbligatorio");

                if (!SecurityHelper.IsValidInput(statoOrdine, maxLength: 100))
                    return SingleResponseDTO<bool>.ErrorResponse(
                        "Il nome dello stato ordine contiene caratteri non validi");

                var searchTerm = StringHelper.NormalizeSearchTerm(statoOrdine);

                var exists = await _context.ConfigSoglieTempi
                    .AsNoTracking()
                    .Include(c => c.StatoOrdine)
                    .AnyAsync(c => EF.Functions.Like(c.StatoOrdine.StatoOrdine1.Trim().ToUpper(), searchTerm.ToUpper()));

                string message = exists
                    ? $"Esiste già una configurazione per lo stato ordine '{searchTerm}'"
                    : $"Nessuna configurazione trovata per lo stato ordine '{searchTerm}'";

                return SingleResponseDTO<bool>.SuccessResponse(exists, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ExistsByStatoOrdine per statoOrdine: {statoOrdine}", statoOrdine);
                return SingleResponseDTO<bool>.ErrorResponse(
                    "Errore nella verifica dell'esistenza della configurazione per stato ordine");
            }
        }
    }
}