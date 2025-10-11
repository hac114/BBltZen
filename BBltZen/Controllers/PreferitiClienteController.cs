using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreferitiClienteController : ControllerBase
    {
        private readonly IPreferitiClienteRepository _preferitiRepository;

        public PreferitiClienteController(IPreferitiClienteRepository preferitiRepository)
        {
            _preferitiRepository = preferitiRepository;
        }

        // GET: api/PreferitiCliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetAll()
        {
            try
            {
                var preferiti = await _preferitiRepository.GetAllAsync();
                return Ok(preferiti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PreferitiClienteDTO>> GetById(int id)
        {
            try
            {
                var preferito = await _preferitiRepository.GetByIdAsync(id);

                if (preferito == null)
                {
                    return NotFound($"Preferito con ID {id} non trovato");
                }

                return Ok(preferito);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/cliente/5
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetByClienteId(int clienteId)
        {
            try
            {
                var preferiti = await _preferitiRepository.GetByClienteIdAsync(clienteId);
                return Ok(preferiti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/bevanda/5
        [HttpGet("bevanda/{bevandaId}")]
        public async Task<ActionResult<IEnumerable<PreferitiClienteDTO>>> GetByBevandaId(int bevandaId)
        {
            try
            {
                var preferiti = await _preferitiRepository.GetByBevandaIdAsync(bevandaId);
                return Ok(preferiti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/cliente/5/bevanda/5
        [HttpGet("cliente/{clienteId}/bevanda/{bevandaId}")]
        public async Task<ActionResult<PreferitiClienteDTO>> GetByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                var preferito = await _preferitiRepository.GetByClienteAndBevandaAsync(clienteId, bevandaId);

                if (preferito == null)
                {
                    return NotFound($"Preferito non trovato per il cliente {clienteId} e bevanda {bevandaId}");
                }

                return Ok(preferito);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST: api/PreferitiCliente
        [HttpPost]
        public async Task<ActionResult<PreferitiClienteDTO>> Create(PreferitiClienteDTO preferitoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se il preferito esiste già
                var exists = await _preferitiRepository.ExistsByClienteAndBevandaAsync(
                    preferitoDto.ClienteId, preferitoDto.BevandaId);

                if (exists)
                {
                    return BadRequest($"Questa bevanda è già nei preferiti del cliente");
                }

                await _preferitiRepository.AddAsync(preferitoDto);

                return CreatedAtAction(nameof(GetById), new { id = preferitoDto.PreferitoId }, preferitoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiunta ai preferiti: {ex.Message}");
            }
        }

        // PUT: api/PreferitiCliente/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PreferitiClienteDTO preferitoDto)
        {
            try
            {
                if (id != preferitoDto.PreferitoId)
                {
                    return BadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se il preferito esiste
                var existingPreferito = await _preferitiRepository.GetByIdAsync(id);
                if (existingPreferito == null)
                {
                    return NotFound($"Preferito con ID {id} non trovato");
                }

                await _preferitiRepository.UpdateAsync(preferitoDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento del preferito: {ex.Message}");
            }
        }

        // DELETE: api/PreferitiCliente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verifica se il preferito esiste
                var existingPreferito = await _preferitiRepository.GetByIdAsync(id);
                if (existingPreferito == null)
                {
                    return NotFound($"Preferito con ID {id} non trovato");
                }

                await _preferitiRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione del preferito: {ex.Message}");
            }
        }

        // DELETE: api/PreferitiCliente/cliente/5/bevanda/5
        [HttpDelete("cliente/{clienteId}/bevanda/{bevandaId}")]
        public async Task<IActionResult> DeleteByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                // Verifica se il preferito esiste
                var existingPreferito = await _preferitiRepository.GetByClienteAndBevandaAsync(clienteId, bevandaId);
                if (existingPreferito == null)
                {
                    return NotFound($"Preferito non trovato per il cliente {clienteId} e bevanda {bevandaId}");
                }

                await _preferitiRepository.DeleteByClienteAndBevandaAsync(clienteId, bevandaId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione del preferito: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                var exists = await _preferitiRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/exists/cliente/5/bevanda/5
        [HttpGet("exists/cliente/{clienteId}/bevanda/{bevandaId}")]
        public async Task<ActionResult<bool>> ExistsByClienteAndBevanda(int clienteId, int bevandaId)
        {
            try
            {
                var exists = await _preferitiRepository.ExistsByClienteAndBevandaAsync(clienteId, bevandaId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/PreferitiCliente/count/cliente/5
        [HttpGet("count/cliente/{clienteId}")]
        public async Task<ActionResult<int>> GetCountByCliente(int clienteId)
        {
            try
            {
                var count = await _preferitiRepository.GetCountByClienteAsync(clienteId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}
