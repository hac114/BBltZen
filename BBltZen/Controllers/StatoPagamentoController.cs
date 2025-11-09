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
    public class StatoPagamentoController : SecureBaseController
    {
        private readonly IStatoPagamentoRepository _repository;
        private readonly BubbleTeaContext _context;

        public StatoPagamentoController(
            IStatoPagamentoRepository repository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<StatoPagamentoController> logger)
            : base(environment, logger)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<StatoPagamentoDTO>>> GetAll()
        {
            try
            {
                var statiPagamento = await _repository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_STATI_PAGAMENTO", "StatoPagamento", "All");

                return Ok(statiPagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli stati pagamento");
                return SafeInternalError<IEnumerable<StatoPagamentoDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli stati pagamento: {ex.Message}"
                        : "Errore interno nel recupero stati pagamento"
                );
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<StatoPagamentoDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<StatoPagamentoDTO>("ID stato pagamento non valido");

                var statoPagamento = await _repository.GetByIdAsync(id);

                if (statoPagamento == null)
                    return SafeNotFound<StatoPagamentoDTO>("Stato pagamento");

                // ✅ Log per audit
                LogAuditTrail("GET_STATO_PAGAMENTO_BY_ID", "StatoPagamento", id.ToString());

                return Ok(statoPagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato pagamento {Id}", id);
                return SafeInternalError<StatoPagamentoDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato pagamento {id}: {ex.Message}"
                        : "Errore interno nel recupero stato pagamento"
                );
            }
        }

        [HttpGet("nome/{nomeStatoPagamento}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<StatoPagamentoDTO>> GetByNome(string nomeStatoPagamento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeStatoPagamento))
                    return SafeBadRequest<StatoPagamentoDTO>("Nome stato pagamento non valido");

                var statoPagamento = await _repository.GetByNomeAsync(nomeStatoPagamento);

                if (statoPagamento == null)
                    return SafeNotFound<StatoPagamentoDTO>("Stato pagamento");

                // ✅ Log per audit
                LogAuditTrail("GET_STATO_PAGAMENTO_BY_NOME", "StatoPagamento", nomeStatoPagamento);

                return Ok(statoPagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato pagamento per nome {Nome}", nomeStatoPagamento);
                return SafeInternalError<StatoPagamentoDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dello stato pagamento per nome {nomeStatoPagamento}: {ex.Message}"
                        : "Errore interno nel recupero stato pagamento per nome"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<StatoPagamentoDTO>> Create([FromBody] StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                if (!IsModelValid(statoPagamentoDto))
                    return SafeBadRequest<StatoPagamentoDTO>("Dati stato pagamento non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var nomeEsistente = await _context.StatoPagamento
                    .AnyAsync(s => s.StatoPagamento1 == statoPagamentoDto.StatoPagamento1);
                if (nomeEsistente)
                    return SafeBadRequest<StatoPagamentoDTO>("Esiste già uno stato pagamento con questo nome");

                // Verifica se esiste già uno stato con lo stesso ID
                if (statoPagamentoDto.StatoPagamentoId > 0 && await _repository.ExistsAsync(statoPagamentoDto.StatoPagamentoId))
                    return SafeBadRequest<StatoPagamentoDTO>("Esiste già uno stato pagamento con questo ID");

                await _repository.AddAsync(statoPagamentoDto);

                // ✅ Audit trail e security event
                LogAuditTrail("CREATE_STATO_PAGAMENTO", "StatoPagamento", statoPagamentoDto.StatoPagamentoId.ToString());
                LogSecurityEvent("StatoPagamentoCreated", new
                {
                    StatoPagamentoId = statoPagamentoDto.StatoPagamentoId,
                    Nome = statoPagamentoDto.StatoPagamento1,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = statoPagamentoDto.StatoPagamentoId },
                    statoPagamentoDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione stato pagamento");
                return SafeInternalError<StatoPagamentoDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione stato pagamento: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione stato pagamento"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione stato pagamento");
                return SafeBadRequest<StatoPagamentoDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato pagamento");
                return SafeInternalError<StatoPagamentoDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante la creazione dello stato pagamento: {ex.Message}"
                        : "Errore interno nella creazione stato pagamento"
                );
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID stato pagamento non valido");

                if (id != statoPagamentoDto.StatoPagamentoId)
                    return SafeBadRequest("ID stato pagamento non corrispondente");

                if (!IsModelValid(statoPagamentoDto))
                    return SafeBadRequest("Dati stato pagamento non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Stato pagamento");

                // ✅ Controlli avanzati con BubbleTeaContext
                var nomeEsistenteAltro = await _context.StatoPagamento
                    .AnyAsync(s => s.StatoPagamento1 == statoPagamentoDto.StatoPagamento1 && s.StatoPagamentoId != id);
                if (nomeEsistenteAltro)
                    return SafeBadRequest("Esiste già un altro stato pagamento con questo nome");

                await _repository.UpdateAsync(statoPagamentoDto);

                // ✅ Audit trail e security event
                LogAuditTrail("UPDATE_STATO_PAGAMENTO", "StatoPagamento", statoPagamentoDto.StatoPagamentoId.ToString());
                LogSecurityEvent("StatoPagamentoUpdated", new
                {
                    StatoPagamentoId = statoPagamentoDto.StatoPagamentoId,
                    Nome = statoPagamentoDto.StatoPagamento1,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"Nome: {existing.StatoPagamento1} → {statoPagamentoDto.StatoPagamento1}"
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento stato pagamento {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento stato pagamento {id}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento stato pagamento"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento stato pagamento {Id}", id);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato pagamento {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'aggiornamento dello stato pagamento {id}: {ex.Message}"
                        : "Errore interno nell'aggiornamento stato pagamento"
                );
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID stato pagamento non valido");

                var statoPagamento = await _repository.GetByIdAsync(id);
                if (statoPagamento == null)
                    return SafeNotFound("Stato pagamento");

                // ✅ Controlli avanzati con BubbleTeaContext - Verifica se ci sono ordini collegati
                var ordiniCollegati = await _context.Ordine
                    .AnyAsync(o => o.StatoPagamentoId == id);
                if (ordiniCollegati)
                    return SafeBadRequest("Impossibile eliminare: esistono ordini collegati a questo stato pagamento");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail e security event
                LogAuditTrail("DELETE_STATO_PAGAMENTO", "StatoPagamento", id.ToString());
                LogSecurityEvent("StatoPagamentoDeleted", new
                {
                    StatoPagamentoId = id,
                    User = User.Identity?.Name ?? "Anonymous",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione stato pagamento {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione stato pagamento {id}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione stato pagamento"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato pagamento {Id}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'eliminazione dello stato pagamento {id}: {ex.Message}"
                        : "Errore interno nell'eliminazione stato pagamento"
                );
            }
        }
    }
}