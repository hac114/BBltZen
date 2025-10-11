using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtentiController : ControllerBase
    {
        private readonly IUtentiRepository _utentiRepository;

        public UtentiController(IUtentiRepository utentiRepository)
        {
            _utentiRepository = utentiRepository;
        }

        // GET: api/utenti
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetAll()
        {
            var utenti = await _utentiRepository.GetAllAsync();
            return Ok(utenti);
        }

        // GET: api/utenti/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UtentiDTO>> GetById(int id)
        {
            var utente = await _utentiRepository.GetByIdAsync(id);
            if (utente == null)
            {
                return NotFound($"Utente con ID {id} non trovato");
            }
            return Ok(utente);
        }

        // GET: api/utenti/email/test@example.com
        [HttpGet("email/{email}")]
        public async Task<ActionResult<UtentiDTO>> GetByEmail(string email)
        {
            var utente = await _utentiRepository.GetByEmailAsync(email);
            if (utente == null)
            {
                return NotFound($"Utente con email {email} non trovato");
            }
            return Ok(utente);
        }

        // GET: api/utenti/tipo/admin
        [HttpGet("tipo/{tipoUtente}")]
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetByTipo(string tipoUtente)
        {
            var utenti = await _utentiRepository.GetByTipoUtenteAsync(tipoUtente);
            return Ok(utenti);
        }

        // GET: api/utenti/attivi
        [HttpGet("attivi")]
        public async Task<ActionResult<IEnumerable<UtentiDTO>>> GetAttivi()
        {
            var utenti = await _utentiRepository.GetAttiviAsync();
            return Ok(utenti);
        }

        // POST: api/utenti
        [HttpPost]
        public async Task<ActionResult<UtentiDTO>> Create(UtentiDTO utenteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se l'email esiste già
            if (await _utentiRepository.EmailExistsAsync(utenteDto.Email))
            {
                return Conflict($"L'email {utenteDto.Email} è già registrata");
            }

            await _utentiRepository.AddAsync(utenteDto);

            return CreatedAtAction(nameof(GetById), new { id = utenteDto.UtenteId }, utenteDto);
        }

        // PUT: api/utenti/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UtentiDTO utenteDto)
        {
            if (id != utenteDto.UtenteId)
            {
                return BadRequest("ID nell'URL non corrisponde all'ID nel body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _utentiRepository.UpdateAsync(utenteDto);
            }
            catch (ArgumentException)
            {
                return NotFound($"Utente con ID {id} non trovato");
            }

            return NoContent();
        }

        // DELETE: api/utenti/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _utentiRepository.ExistsAsync(id);
            if (!exists)
            {
                return NotFound($"Utente con ID {id} non trovato");
            }

            await _utentiRepository.DeleteAsync(id);
            return NoContent();
        }

        // GET: api/utenti/check-email/test@example.com
        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            var exists = await _utentiRepository.EmailExistsAsync(email);
            return Ok(exists);
        }
    }
}