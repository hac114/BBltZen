using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Database;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StatoOrdineController : SecureBaseController
    {
        private readonly IStatoOrdineRepository _repository;
        private readonly BubbleTeaContext _context;

        public StatoOrdineController(
            IStatoOrdineRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<StatoOrdineController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_STATI_ORDINE", "StatoOrdine", "All");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli stati ordine");
                return SafeInternalError<IEnumerable<StatoOrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati ordine: {ex.Message}"
                        : "Errore interno nel recupero stati ordine"
                );
            }
        }

        [HttpGet("{statoOrdineId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<StatoOrdineDTO>> GetById(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest<StatoOrdineDTO>("ID stato ordine non valido");

                var result = await _repository.GetByIdAsync(statoOrdineId);

                if (result == null)
                    return SafeNotFound<StatoOrdineDTO>("Stato ordine");

                // ✅ Log per audit
                LogAuditTrail("GET_STATO_ORDINE_BY_ID", "StatoOrdine", statoOrdineId.ToString());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError<StatoOrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato ordine {statoOrdineId}: {ex.Message}"
                        : "Errore interno nel recupero stato ordine"
                );
            }
        }

        [HttpGet("nome/{nomeStatoOrdine}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<StatoOrdineDTO>> GetByNome(string nomeStatoOrdine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeStatoOrdine))
                    return SafeBadRequest<StatoOrdineDTO>("Nome stato ordine non valido");

                var result = await _repository.GetByNomeAsync(nomeStatoOrdine);

                if (result == null)
                    return SafeNotFound<StatoOrdineDTO>("Stato ordine con il nome specificato");

                // ✅ Log per audit
                LogAuditTrail("GET_STATO_ORDINE_BY_NOME", "StatoOrdine", nomeStatoOrdine);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato ordine per nome {NomeStatoOrdine}", nomeStatoOrdine);
                return SafeInternalError<StatoOrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato ordine per nome {nomeStatoOrdine}: {ex.Message}"
                        : "Errore interno nel recupero stato ordine per nome"
                );
            }
        }

        [HttpGet("terminali")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiTerminali()
        {
            try
            {
                var result = await _repository.GetStatiTerminaliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_STATI_ORDINE_TERMINALI", "StatoOrdine", $"Count: {result?.Count()}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati ordine terminali");
                return SafeInternalError<IEnumerable<StatoOrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati ordine terminali: {ex.Message}"
                        : "Errore interno nel recupero stati ordine terminali"
                );
            }
        }

        [HttpGet("non-terminali")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<StatoOrdineDTO>>> GetStatiNonTerminali()
        {
            try
            {
                var result = await _repository.GetStatiNonTerminaliAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_STATI_ORDINE_NON_TERMINALI", "StatoOrdine", $"Count: {result?.Count()}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati ordine non terminali");
                return SafeInternalError<IEnumerable<StatoOrdineDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati ordine non terminali: {ex.Message}"
                        : "Errore interno nel recupero stati ordine non terminali"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatoOrdineDTO>> Create([FromBody] StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (!IsModelValid(statoOrdineDto))
                    return SafeBadRequest<StatoOrdineDTO>("Dati stato ordine non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var nomeEsistente = await _context.StatoOrdine
                    .AnyAsync(s => s.StatoOrdine1 == statoOrdineDto.StatoOrdine1);
                if (nomeEsistente)
                    return SafeBadRequest<StatoOrdineDTO>("Esiste già uno stato ordine con questo nome");

                // Verifica se esiste già uno stato ordine con lo stesso ID
                if (statoOrdineDto.StatoOrdineId > 0 && await _repository.ExistsAsync(statoOrdineDto.StatoOrdineId))
                    return SafeBadRequest<StatoOrdineDTO>("Esiste già uno stato ordine con questo ID");

                await _repository.AddAsync(statoOrdineDto);

                // ✅ Audit trail e security event
                LogAuditTrail("CREATE_STATO_ORDINE", "StatoOrdine", statoOrdineDto.StatoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineCreated", new
                {
                    StatoOrdineId = statoOrdineDto.StatoOrdineId,
                    Nome = statoOrdineDto.StatoOrdine1,
                    Terminale = statoOrdineDto.Terminale,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return CreatedAtAction(nameof(GetById),
                    new { statoOrdineId = statoOrdineDto.StatoOrdineId },
                    statoOrdineDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione stato ordine");
                return SafeInternalError<StatoOrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione stato ordine: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione stato ordine"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione stato ordine");
                return SafeBadRequest<StatoOrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato ordine");
                return SafeInternalError<StatoOrdineDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante la creazione dello stato ordine: {ex.Message}"
                        : "Errore interno nella creazione stato ordine"
                );
            }
        }

        [HttpPut("{statoOrdineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int statoOrdineId, [FromBody] StatoOrdineDTO statoOrdineDto)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest("ID stato ordine non valido");

                if (statoOrdineId != statoOrdineDto.StatoOrdineId)
                    return SafeBadRequest("ID stato ordine non corrispondente");

                if (!IsModelValid(statoOrdineDto))
                    return SafeBadRequest("Dati stato ordine non validi");

                var existing = await _repository.GetByIdAsync(statoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato ordine");

                // ✅ Controlli avanzati con BubbleTeaContext
                var nomeEsistenteAltro = await _context.StatoOrdine
                    .AnyAsync(s => s.StatoOrdine1 == statoOrdineDto.StatoOrdine1 && s.StatoOrdineId != statoOrdineId);
                if (nomeEsistenteAltro)
                    return SafeBadRequest("Esiste già un altro stato ordine con questo nome");

                await _repository.UpdateAsync(statoOrdineDto);

                // ✅ Audit trail e security event
                LogAuditTrail("UPDATE_STATO_ORDINE", "StatoOrdine", statoOrdineDto.StatoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineUpdated", new
                {
                    StatoOrdineId = statoOrdineDto.StatoOrdineId,
                    Nome = statoOrdineDto.StatoOrdine1,
                    Terminale = statoOrdineDto.Terminale,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"Nome: {existing.StatoOrdine1} → {statoOrdineDto.StatoOrdine1}, Terminale: {existing.Terminale} → {statoOrdineDto.Terminale}"
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento stato ordine {statoOrdineId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento stato ordine"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'aggiornamento dello stato ordine {statoOrdineId}: {ex.Message}"
                        : "Errore interno nell'aggiornamento stato ordine"
                );
            }
        }

        [HttpDelete("{statoOrdineId}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int statoOrdineId)
        {
            try
            {
                if (statoOrdineId <= 0)
                    return SafeBadRequest("ID stato ordine non valido");

                var existing = await _repository.GetByIdAsync(statoOrdineId);
                if (existing == null)
                    return SafeNotFound("Stato ordine");

                // ✅ Controlli avanzati con BubbleTeaContext - Verifica se ci sono ordini collegati
                var ordiniCollegati = await _context.Ordine
                    .AnyAsync(o => o.StatoOrdineId == statoOrdineId);
                if (ordiniCollegati)
                    return SafeBadRequest("Impossibile eliminare: esistono ordini collegati a questo stato");

                await _repository.DeleteAsync(statoOrdineId);

                // ✅ Audit trail e security event
                LogAuditTrail("DELETE_STATO_ORDINE", "StatoOrdine", statoOrdineId.ToString());
                LogSecurityEvent("StatoOrdineDeleted", new
                {
                    StatoOrdineId = statoOrdineId,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione stato ordine {statoOrdineId}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione stato ordine"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato ordine {StatoOrdineId}", statoOrdineId);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'eliminazione dello stato ordine {statoOrdineId}: {ex.Message}"
                        : "Errore interno nell'eliminazione stato ordine"
                );
            }
        }
    }
}