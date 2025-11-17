// BBltZen/Controllers/SessioniQrController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class SessioniQrController : SecureBaseController
    {
        private readonly ISessioniQrRepository _repository;

        public SessioniQrController(
            ISessioniQrRepository repository,
            IWebHostEnvironment environment,
            ILogger<SessioniQrController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/SessioniQr
        [HttpGet]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetAll()
        {
            try
            {
                var sessioni = await _repository.GetAllAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le sessioni QR");
                return SafeInternalError("Errore durante il recupero delle sessioni QR");
            }
        }

        // GET: api/SessioniQr/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SessioniQrDTO>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return SafeBadRequest<SessioniQrDTO>("ID sessione non valido");

                var sessione = await _repository.GetByIdAsync(id);
                return sessione == null ? SafeNotFound<SessioniQrDTO>("Sessione QR") : Ok(sessione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della sessione QR {Id}", id);
                return SafeInternalError("Errore durante il recupero della sessione QR");
            }
        }

        // GET: api/SessioniQr/qrcode/{qrCode}
        [HttpGet("qrcode/{qrCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<SessioniQrDTO>> GetByQrCode(string qrCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qrCode))
                    return SafeBadRequest<SessioniQrDTO>("Codice QR non valido");

                var sessione = await _repository.GetByQrCodeAsync(qrCode);
                return sessione == null ? SafeNotFound<SessioniQrDTO>("Sessione QR") : Ok(sessione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della sessione QR per codice {QrCode}", qrCode);
                return SafeInternalError("Errore durante il recupero della sessione QR");
            }
        }

        // GET: api/SessioniQr/codice/{codiceSessione}
        [HttpGet("codice/{codiceSessione}")]
        [AllowAnonymous]
        public async Task<ActionResult<SessioniQrDTO>> GetByCodiceSessione(string codiceSessione)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codiceSessione))
                    return SafeBadRequest<SessioniQrDTO>("Codice sessione non valido");

                var sessione = await _repository.GetByCodiceSessioneAsync(codiceSessione);
                return sessione == null ? SafeNotFound<SessioniQrDTO>("Sessione QR") : Ok(sessione);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero della sessione QR per codice {CodiceSessione}", codiceSessione);
                return SafeInternalError("Errore durante il recupero della sessione QR");
            }
        }

        // GET: api/SessioniQr/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return SafeBadRequest<IEnumerable<SessioniQrDTO>>("ID cliente non valido");

                var sessioni = await _repository.GetByClienteIdAsync(clienteId);
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle sessioni QR per cliente {ClienteId}", clienteId);
                return SafeInternalError("Errore durante il recupero delle sessioni QR");
            }
        }

        // GET: api/SessioniQr/tavolo/{tavoloId}
        [HttpGet("tavolo/{tavoloId}")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetByTavoloId(int tavoloId)
        {
            try
            {
                if (tavoloId <= 0)
                    return SafeBadRequest<IEnumerable<SessioniQrDTO>>("ID tavolo non valido");

                var sessioni = await _repository.GetByTavoloIdAsync(tavoloId);
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle sessioni QR per tavolo {TavoloId}", tavoloId);
                return SafeInternalError("Errore durante il recupero delle sessioni QR");
            }
        }

        // GET: api/SessioniQr/nonutilizzate
        [HttpGet("nonutilizzate")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetNonUtilizzate()
        {
            try
            {
                var sessioni = await _repository.GetNonutilizzateAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle sessioni QR non utilizzate");
                return SafeInternalError("Errore durante il recupero delle sessioni QR");
            }
        }

        // GET: api/SessioniQr/scadute
        [HttpGet("scadute")]
        //[Authorize(Roles = "admin,staff")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetScadute()
        {
            try
            {
                var sessioni = await _repository.GetScaduteAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle sessioni QR scadute");
                return SafeInternalError("Errore durante il recupero delle sessioni QR");
            }
        }

        // POST: api/SessioniQr/genera/{tavoloId}
        [HttpPost("genera/{tavoloId}")]
        //[Authorize(Roles = "admin,staff")]
        public async Task<ActionResult<SessioniQrDTO>> GeneraSessioneQr(int tavoloId, [FromQuery] string frontendUrl = "https://bbzen.it")
        {
            try
            {
                if (tavoloId <= 0 || string.IsNullOrWhiteSpace(frontendUrl))
                    return SafeBadRequest<SessioniQrDTO>("Dati richiesta non validi");

                var sessione = await _repository.GeneraSessioneQrAsync(tavoloId, frontendUrl);

                LogAuditTrail("GENERATE_QR_SESSION", "SessioniQr", sessione.SessioneId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("QRSessionGenerated", new
                {
                    sessione.SessioneId,
                    tavoloId,
                    sessione.CodiceSessione,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = sessione.SessioneId },
                    sessione);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tavolo non trovato per generazione QR: {TavoloId}", tavoloId);
                return SafeBadRequest<SessioniQrDTO>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione della sessione QR per tavolo {TavoloId}", tavoloId);
                return SafeInternalError("Errore durante la generazione della sessione QR");
            }
        }

        // POST: api/SessioniQr
        [HttpPost]
        //[Authorize(Roles = "admin,staff")]
        public async Task<ActionResult<SessioniQrDTO>> Create(SessioniQrDTO sessioneQrDto)
        {
            try
            {
                if (!IsModelValid(sessioneQrDto))
                    return SafeBadRequest<SessioniQrDTO>("Dati sessione QR non validi");

                if (string.IsNullOrWhiteSpace(sessioneQrDto.QrCode))
                    return SafeBadRequest<SessioniQrDTO>("Codice QR obbligatorio");

                if (sessioneQrDto.TavoloId <= 0)
                    return SafeBadRequest<SessioniQrDTO>("ID tavolo non valido");

                // ✅ CORRETTO: Assegna il risultato di AddAsync
                var result = await _repository.AddAsync(sessioneQrDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_QR_SESSION", "SessioniQr", result.SessioneId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("QRSessionCreated", new
                {
                    result.SessioneId,
                    result.TavoloId,
                    User = User.Identity?.Name
                });

                return CreatedAtAction(nameof(GetById),
                    new { id = result.SessioneId },
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione della sessione QR");
                return SafeInternalError("Errore durante la creazione della sessione QR");
            }
        }

        // PUT: api/SessioniQr/{id}
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,staff")]
        public async Task<ActionResult> Update(Guid id, SessioniQrDTO sessioneQrDto)
        {
            try
            {
                if (id == Guid.Empty || id != sessioneQrDto.SessioneId || !IsModelValid(sessioneQrDto))
                    return SafeBadRequest("Dati sessione QR non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Sessione QR");

                await _repository.UpdateAsync(sessioneQrDto);

                LogAuditTrail("UPDATE_QR_SESSION", "SessioniQr", sessioneQrDto.SessioneId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("QRSessionUpdated", new
                {
                    sessioneQrDto.SessioneId,
                    sessioneQrDto.Stato,
                    sessioneQrDto.Utilizzato,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (ArgumentException)
            {
                return SafeNotFound("Sessione QR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento della sessione QR {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento della sessione QR");
            }
        }

        // DELETE: api/SessioniQr/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin,staff")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return SafeBadRequest("ID sessione non valido");

                var sessione = await _repository.GetByIdAsync(id);
                if (sessione == null)
                    return SafeNotFound("Sessione QR");

                await _repository.DeleteAsync(id);

                LogAuditTrail("DELETE_QR_SESSION", "SessioniQr", id.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("QRSessionDeleted", new
                {
                    SessioneId = id,
                    User = User.Identity?.Name
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della sessione QR {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione della sessione QR");
            }
        }

        // GET: api/SessioniQr/{id}/exists
        [HttpGet("{id}/exists")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> Exists(Guid id)
        {
            try
            {
                return id == Guid.Empty
                    ? SafeBadRequest<bool>("ID sessione non valido")
                    : Ok(await _repository.ExistsAsync(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la verifica dell'esistenza della sessione QR {Id}", id);
                return SafeInternalError("Errore durante la verifica della sessione QR");
            }
        }
    }
}