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
    public class DolceController : ControllerBase
    {
        private readonly IDolceRepository _repository;

        public DolceController(IDolceRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Ottiene tutti i dolci
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetAll()
        {
            try
            {
                var result = await _repository.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dei dolci: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene un dolce specifico tramite ID articolo
        /// </summary>
        [HttpGet("{articoloId}")]
        public async Task<ActionResult<DolceDTO>> GetById(int articoloId)
        {
            try
            {
                var result = await _repository.GetByIdAsync(articoloId);

                if (result == null)
                {
                    return NotFound($"Dolce con ArticoloId {articoloId} non trovato");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero del dolce: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene solo i dolci disponibili
        /// </summary>
        [HttpGet("disponibili")]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetDisponibili()
        {
            try
            {
                var result = await _repository.GetDisponibiliAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dei dolci disponibili: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene i dolci per priorità
        /// </summary>
        [HttpGet("priorita/{priorita}")]
        public async Task<ActionResult<IEnumerable<DolceDTO>>> GetByPriorita(int priorita)
        {
            try
            {
                var result = await _repository.GetByPrioritaAsync(priorita);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dei dolci per priorità: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un nuovo dolce
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DolceDTO>> Create(DolceDTO dolceDto)
        {
            try
            {
                if (dolceDto == null)
                {
                    return BadRequest("Dati dolce non validi");
                }

                // Verifica se esiste già un dolce con lo stesso ArticoloId
                if (await _repository.ExistsAsync(dolceDto.ArticoloId))
                {
                    return Conflict($"Esiste già un dolce con ArticoloId {dolceDto.ArticoloId}");
                }

                await _repository.AddAsync(dolceDto);

                return CreatedAtAction(nameof(GetById), new { articoloId = dolceDto.ArticoloId }, dolceDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione del dolce: {ex.Message}");
            }
        }

        /// <summary>
        /// Aggiorna un dolce esistente
        /// </summary>
        [HttpPut("{articoloId}")]
        public async Task<ActionResult> Update(int articoloId, DolceDTO dolceDto)
        {
            try
            {
                if (dolceDto == null || articoloId != dolceDto.ArticoloId)
                {
                    return BadRequest("ID del dolce non corrisponde");
                }

                if (!await _repository.ExistsAsync(articoloId))
                {
                    return NotFound($"Dolce con ArticoloId {articoloId} non trovato");
                }

                await _repository.UpdateAsync(dolceDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento del dolce: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina un dolce
        /// </summary>
        [HttpDelete("{articoloId}")]
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (!await _repository.ExistsAsync(articoloId))
                {
                    return NotFound($"Dolce con ArticoloId {articoloId} non trovato");
                }

                await _repository.DeleteAsync(articoloId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione del dolce: {ex.Message}");
            }
        }
    }
}