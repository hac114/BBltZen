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
    public class ClienteController : ControllerBase
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteController(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        // GET: api/Cliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetAll()
        {
            try
            {
                var clienti = await _clienteRepository.GetAllAsync();
                return Ok(clienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Cliente/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetById(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {id} non trovato");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Cliente/tavolo/5
        [HttpGet("tavolo/{tavoloId}")]
        public async Task<ActionResult<ClienteDTO>> GetByTavoloId(int tavoloId)
        {
            try
            {
                var cliente = await _clienteRepository.GetByTavoloIdAsync(tavoloId);

                if (cliente == null)
                {
                    return NotFound($"Nessun cliente trovato per il tavolo con ID {tavoloId}");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // POST: api/Cliente
        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> Create(ClienteDTO clienteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se esiste già un cliente per questo tavolo
                var existingCliente = await _clienteRepository.ExistsByTavoloIdAsync(clienteDto.TavoloId);
                if (existingCliente)
                {
                    return BadRequest($"Esiste già un cliente per il tavolo con ID {clienteDto.TavoloId}");
                }

                await _clienteRepository.AddAsync(clienteDto);

                return CreatedAtAction(nameof(GetById), new { id = clienteDto.ClienteId }, clienteDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione del cliente: {ex.Message}");
            }
        }

        // PUT: api/Cliente/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClienteDTO clienteDto)
        {
            try
            {
                if (id != clienteDto.ClienteId)
                {
                    return BadRequest("ID nell'URL non corrisponde all'ID nel corpo della richiesta");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verifica se il cliente esiste
                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                {
                    return NotFound($"Cliente con ID {id} non trovato");
                }

                // Verifica se esiste già un altro cliente per questo tavolo
                var clienteByTavolo = await _clienteRepository.GetByTavoloIdAsync(clienteDto.TavoloId);
                if (clienteByTavolo != null && clienteByTavolo.ClienteId != id)
                {
                    return BadRequest($"Esiste già un altro cliente per il tavolo con ID {clienteDto.TavoloId}");
                }

                await _clienteRepository.UpdateAsync(clienteDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento del cliente: {ex.Message}");
            }
        }

        // DELETE: api/Cliente/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Verifica se il cliente esiste
                var existingCliente = await _clienteRepository.GetByIdAsync(id);
                if (existingCliente == null)
                {
                    return NotFound($"Cliente con ID {id} non trovato");
                }

                await _clienteRepository.DeleteAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione del cliente: {ex.Message}");
            }
        }

        // GET: api/Cliente/exists/5
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> Exists(int id)
        {
            try
            {
                var exists = await _clienteRepository.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }

        // GET: api/Cliente/exists/tavolo/5
        [HttpGet("exists/tavolo/{tavoloId}")]
        public async Task<ActionResult<bool>> ExistsByTavoloId(int tavoloId)
        {
            try
            {
                var exists = await _clienteRepository.ExistsByTavoloIdAsync(tavoloId);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore interno del server: {ex.Message}");
            }
        }
    }
}