// BBltZen/Controllers/StatoPagamentoController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class StatoPagamentoController : SecureBaseController
    {
        private readonly IStatoPagamentoRepository _repository;

        public StatoPagamentoController(
            IStatoPagamentoRepository repository,
            IWebHostEnvironment environment,
            ILogger<StatoPagamentoController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/StatoPagamento
        [HttpGet]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<StatoPagamentoDTO>>> GetAll()
        {
            try
            {
                var statiPagamento = await _repository.GetAllAsync();
                return Ok(statiPagamento);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli stati pagamento");
                return SafeInternalError("Errore durante il recupero degli stati pagamento");
            }
        }

        // GET: api/StatoPagamento/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<StatoPagamentoDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<StatoPagamentoDTO>("ID stato pagamento non valido");

                var statoPagamento = await _repository.GetByIdAsync(id);

                if (statoPagamento == null)
                    return SafeNotFound<StatoPagamentoDTO>("Stato pagamento");

                return Ok(statoPagamento);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato pagamento {Id}", id);
                return SafeInternalError("Errore durante il recupero dello stato pagamento");
            }
        }

        // GET: api/StatoPagamento/nome/{nomeStatoPagamento}
        [HttpGet("nome/{nomeStatoPagamento}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<StatoPagamentoDTO>> GetByNome(string nomeStatoPagamento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeStatoPagamento))
                    return SafeBadRequest<StatoPagamentoDTO>("Nome stato pagamento non valido");

                var statoPagamento = await _repository.GetByNomeAsync(nomeStatoPagamento);

                if (statoPagamento == null)
                    return SafeNotFound<StatoPagamentoDTO>("Stato pagamento");

                return Ok(statoPagamento);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dello stato pagamento per nome {Nome}", nomeStatoPagamento);
                return SafeInternalError("Errore durante il recupero dello stato pagamento");
            }
        }

        // POST: api/StatoPagamento
        [HttpPost]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<StatoPagamentoDTO>> Create(StatoPagamentoDTO statoPagamentoDto)
        {
            try
            {
                if (!IsModelValid(statoPagamentoDto))
                    return SafeBadRequest<StatoPagamentoDTO>("Dati stato pagamento non validi");

                // Verifica se esiste già uno stato con lo stesso nome
                var existing = await _repository.GetByNomeAsync(statoPagamentoDto.StatoPagamento1);
                if (existing != null)
                    return SafeBadRequest<StatoPagamentoDTO>("Esiste già uno stato pagamento con questo nome");

                await _repository.AddAsync(statoPagamentoDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_STATO_PAGAMENTO", "StatoPagamento", statoPagamentoDto.StatoPagamentoId.ToString());
                LogSecurityEvent("StatoPagamentoCreated", new
                {
                    StatoPagamentoId = statoPagamentoDto.StatoPagamentoId,
                    Nome = statoPagamentoDto.StatoPagamento1,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = statoPagamentoDto.StatoPagamentoId },
                    statoPagamentoDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dello stato pagamento");
                return SafeInternalError("Errore durante la creazione dello stato pagamento");
            }
        }

        // PUT: api/StatoPagamento/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, StatoPagamentoDTO statoPagamentoDto)
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

                // Verifica se esiste già un altro stato con lo stesso nome
                var existingByName = await _repository.GetByNomeAsync(statoPagamentoDto.StatoPagamento1);
                if (existingByName != null && existingByName.StatoPagamentoId != id)
                    return SafeBadRequest("Esiste già uno stato pagamento con questo nome");

                await _repository.UpdateAsync(statoPagamentoDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_STATO_PAGAMENTO", "StatoPagamento", statoPagamentoDto.StatoPagamentoId.ToString());
                LogSecurityEvent("StatoPagamentoUpdated", new
                {
                    StatoPagamentoId = statoPagamentoDto.StatoPagamentoId,
                    Nome = statoPagamentoDto.StatoPagamento1,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dello stato pagamento {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dello stato pagamento");
            }
        }

        // DELETE: api/StatoPagamento/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID stato pagamento non valido");

                var statoPagamento = await _repository.GetByIdAsync(id);
                if (statoPagamento == null)
                    return SafeNotFound("Stato pagamento");

                await _repository.DeleteAsync(id);

                // ✅ Audit trail
                LogAuditTrail("DELETE_STATO_PAGAMENTO", "StatoPagamento", id.ToString());
                LogSecurityEvent("StatoPagamentoDeleted", new
                {
                    StatoPagamentoId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dello stato pagamento {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dello stato pagamento");
            }
        }
    }
}