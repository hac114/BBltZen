using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessioniQrController : ControllerBase
    {
        private readonly ISessioniQrRepository _sessioniQrRepository;

        public SessioniQrController(ISessioniQrRepository sessioniQrRepository)
        {
            _sessioniQrRepository = sessioniQrRepository;
        }

        // GET: api/SessioniQr
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetAll()
        {
            try
            {
                var sessioni = await _sessioniQrRepository.GetAllAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle sessioni QR: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SessioniQrDTO>> GetById(Guid id)
        {
            try
            {
                var sessione = await _sessioniQrRepository.GetByIdAsync(id);

                if (sessione == null)
                {
                    return NotFound($"Sessione QR con ID {id} non trovata");
                }

                return Ok(sessione);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero della sessione QR: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/qrcode/{qrCode}
        [HttpGet("qrcode/{qrCode}")]
        public async Task<ActionResult<SessioniQrDTO>> GetByQrCode(string qrCode)
        {
            try
            {
                var sessione = await _sessioniQrRepository.GetByQrCodeAsync(qrCode);

                if (sessione == null)
                {
                    return NotFound($"Sessione QR con codice {qrCode} non trovata");
                }

                return Ok(sessione);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero della sessione QR: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                var sessioni = await _sessioniQrRepository.GetByClienteIdAsync(clienteId);
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle sessioni QR per il cliente: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/nonutilizzate
        [HttpGet("nonutilizzate")]
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetNonUtilizzate()
        {
            try
            {
                var sessioni = await _sessioniQrRepository.GetNonutilizzateAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle sessioni QR non utilizzate: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/scadute
        [HttpGet("scadute")]
        public async Task<ActionResult<IEnumerable<SessioniQrDTO>>> GetScadute()
        {
            try
            {
                var sessioni = await _sessioniQrRepository.GetScaduteAsync();
                return Ok(sessioni);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle sessioni QR scadute: {ex.Message}");
            }
        }

        // POST: api/SessioniQr
        [HttpPost]
        public async Task<ActionResult<SessioniQrDTO>> Create(SessioniQrDTO sessioneQrDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessioneQrDto.QrCode))
                {
                    return BadRequest("Il campo QrCode è obbligatorio");
                }

                if (sessioneQrDto.ClienteId <= 0)
                {
                    return BadRequest("ClienteId deve essere un valore valido");
                }

                await _sessioniQrRepository.AddAsync(sessioneQrDto);

                return CreatedAtAction(nameof(GetById), new { id = sessioneQrDto.SessioneId }, sessioneQrDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione della sessione QR: {ex.Message}");
            }
        }

        // PUT: api/SessioniQr/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, SessioniQrDTO sessioneQrDto)
        {
            try
            {
                if (id != sessioneQrDto.SessioneId)
                {
                    return BadRequest("L'ID della sessione non corrisponde");
                }

                // Verifica se la sessione esiste
                var exists = await _sessioniQrRepository.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound($"Sessione QR con ID {id} non trovata");
                }

                await _sessioniQrRepository.UpdateAsync(sessioneQrDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento della sessione QR: {ex.Message}");
            }
        }

        // DELETE: api/SessioniQr/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var exists = await _sessioniQrRepository.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound($"Sessione QR con ID {id} non trovata");
                }

                await _sessioniQrRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione della sessione QR: {ex.Message}");
            }
        }

        // GET: api/SessioniQr/{id}/exists
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> Exists(Guid id)
        {
            try
            {
                var exists = await _sessioniQrRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la verifica dell'esistenza della sessione QR: {ex.Message}");
            }
        }
    }
}