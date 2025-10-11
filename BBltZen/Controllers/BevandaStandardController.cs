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
    public class BevandaStandardController : ControllerBase
    {
        private readonly IBevandaStandardRepository _bevandaStandardRepository;

        public BevandaStandardController(IBevandaStandardRepository bevandaStandardRepository)
        {
            _bevandaStandardRepository = bevandaStandardRepository;
        }

        /// <summary>
        /// Ottiene tutte le bevande standard
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetAll()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetAllAsync();
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle bevande standard: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene solo le bevande standard disponibili per l'ordinazione
        /// </summary>
        [HttpGet("disponibili")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetDisponibili()
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetDisponibiliAsync();
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle bevande disponibili: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene una bevanda standard specifica tramite ID articolo
        /// </summary>
        [HttpGet("{articoloId}")]
        public async Task<ActionResult<BevandaStandardDTO>> GetById(int articoloId)
        {
            try
            {
                var bevanda = await _bevandaStandardRepository.GetByIdAsync(articoloId);

                if (bevanda == null)
                {
                    return NotFound($"Bevanda standard con ArticoloId {articoloId} non trovata");
                }

                return Ok(bevanda);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero della bevanda standard: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene le bevande standard per dimensione bicchiere
        /// </summary>
        [HttpGet("dimensione/{dimensioneBicchiereId}")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByDimensioneBicchiere(int dimensioneBicchiereId)
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetByDimensioneBicchiereAsync(dimensioneBicchiereId);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle bevande per dimensione bicchiere: {ex.Message}");
            }
        }

        /// <summary>
        /// Ottiene le bevande standard per tipo di personalizzazione
        /// </summary>
        [HttpGet("personalizzazione/{personalizzazioneId}")]
        public async Task<ActionResult<IEnumerable<BevandaStandardDTO>>> GetByPersonalizzazione(int personalizzazioneId)
        {
            try
            {
                var bevande = await _bevandaStandardRepository.GetByPersonalizzazioneAsync(personalizzazioneId);
                return Ok(bevande);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero delle bevande per personalizzazione: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea una nuova bevanda standard
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BevandaStandardDTO>> Create(BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (bevandaStandardDto == null)
                {
                    return BadRequest("Dati bevanda standard non validi");
                }

                // Verifica se esiste già una bevanda con lo stesso ArticoloId
                if (await _bevandaStandardRepository.ExistsAsync(bevandaStandardDto.ArticoloId))
                {
                    return Conflict($"Esiste già una bevanda standard con ArticoloId {bevandaStandardDto.ArticoloId}");
                }

                // Verifica se esiste già la stessa combinazione
                if (await _bevandaStandardRepository.ExistsByCombinazioneAsync(
                    bevandaStandardDto.PersonalizzazioneId, bevandaStandardDto.DimensioneBicchiereId))
                {
                    return Conflict("Esiste già una bevanda standard con la stessa combinazione di personalizzazione e dimensione bicchiere");
                }

                await _bevandaStandardRepository.AddAsync(bevandaStandardDto);

                return CreatedAtAction(nameof(GetById), new { articoloId = bevandaStandardDto.ArticoloId }, bevandaStandardDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la creazione della bevanda standard: {ex.Message}");
            }
        }

        /// <summary>
        /// Aggiorna una bevanda standard esistente
        /// </summary>
        [HttpPut("{articoloId}")]
        public async Task<ActionResult> Update(int articoloId, BevandaStandardDTO bevandaStandardDto)
        {
            try
            {
                if (bevandaStandardDto == null || articoloId != bevandaStandardDto.ArticoloId)
                {
                    return BadRequest("ID della bevanda standard non corrisponde");
                }

                if (!await _bevandaStandardRepository.ExistsAsync(articoloId))
                {
                    return NotFound($"Bevanda standard con ArticoloId {articoloId} non trovata");
                }

                await _bevandaStandardRepository.UpdateAsync(bevandaStandardDto);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'aggiornamento della bevanda standard: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una bevanda standard
        /// </summary>
        [HttpDelete("{articoloId}")]
        public async Task<ActionResult> Delete(int articoloId)
        {
            try
            {
                if (!await _bevandaStandardRepository.ExistsAsync(articoloId))
                {
                    return NotFound($"Bevanda standard con ArticoloId {articoloId} non trovata");
                }

                await _bevandaStandardRepository.DeleteAsync(articoloId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione della bevanda standard: {ex.Message}");
            }
        }
    }
}