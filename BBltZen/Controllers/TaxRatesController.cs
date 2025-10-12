using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxRatesController : ControllerBase
    {
        private readonly ITaxRatesRepository _taxRatesRepository;
        private readonly ILogger<TaxRatesController> _logger;

        public TaxRatesController(ITaxRatesRepository taxRatesRepository, ILogger<TaxRatesController> logger)
        {
            _taxRatesRepository = taxRatesRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutte le aliquote fiscali
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaxRatesDTO>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Recupero di tutte le aliquote fiscali");
                var taxRates = await _taxRatesRepository.GetAllAsync();
                return Ok(taxRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le aliquote fiscali");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottiene un'aliquota fiscale specifica tramite ID
        /// </summary>
        /// <param name="id">ID dell'aliquota fiscale</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaxRatesDTO>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Recupero aliquota fiscale con ID: {TaxRateId}", id);
                var taxRate = await _taxRatesRepository.GetByIdAsync(id);

                if (taxRate == null)
                {
                    _logger.LogWarning("Aliquota fiscale con ID {TaxRateId} non trovata", id);
                    return NotFound($"Aliquota fiscale con ID {id} non trovata.");
                }

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale con ID: {TaxRateId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottiene un'aliquota fiscale tramite valore aliquota
        /// </summary>
        /// <param name="aliquota">Valore dell'aliquota (es. 22.00)</param>
        [HttpGet("aliquota/{aliquota}")]
        public async Task<ActionResult<TaxRatesDTO>> GetByAliquota(decimal aliquota)
        {
            try
            {
                _logger.LogInformation("Recupero aliquota fiscale con valore: {Aliquota}", aliquota);
                var taxRate = await _taxRatesRepository.GetByAliquotaAsync(aliquota);

                if (taxRate == null)
                {
                    _logger.LogWarning("Aliquota fiscale con valore {Aliquota} non trovata", aliquota);
                    return NotFound($"Aliquota fiscale con valore {aliquota} non trovata.");
                }

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale con valore: {Aliquota}", aliquota);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Crea una nuova aliquota fiscale
        /// </summary>
        /// <param name="taxRateDto">Dati della nuova aliquota fiscale</param>
        [HttpPost]
        public async Task<ActionResult<TaxRatesDTO>> Create(TaxRatesDTO taxRateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dati non validi per la creazione dell'aliquota fiscale");
                    return BadRequest(ModelState);
                }

                // Verifica se esiste già un'aliquota con lo stesso valore
                if (await _taxRatesRepository.ExistsByAliquotaAsync(taxRateDto.Aliquota))
                {
                    _logger.LogWarning("Tentativo di creare un'aliquota con valore già esistente: {Aliquota}", taxRateDto.Aliquota);
                    return BadRequest($"Esiste già un'aliquota con valore {taxRateDto.Aliquota}.");
                }

                _logger.LogInformation("Creazione nuova aliquota fiscale con valore: {Aliquota}", taxRateDto.Aliquota);
                await _taxRatesRepository.AddAsync(taxRateDto);

                _logger.LogInformation("Aliquota fiscale creata con ID: {TaxRateId}", taxRateDto.TaxRateId);
                return CreatedAtAction(nameof(GetById), new { id = taxRateDto.TaxRateId }, taxRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'aliquota fiscale");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Aggiorna un'aliquota fiscale esistente
        /// </summary>
        /// <param name="id">ID dell'aliquota fiscale da aggiornare</param>
        /// <param name="taxRateDto">Dati aggiornati dell'aliquota fiscale</param>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, TaxRatesDTO taxRateDto)
        {
            try
            {
                if (id != taxRateDto.TaxRateId)
                {
                    _logger.LogWarning("ID non corrispondente per l'aggiornamento. Richiesto: {Id}, Fornito: {TaxRateId}", id, taxRateDto.TaxRateId);
                    return BadRequest("ID non corrispondente.");
                }

                if (!await _taxRatesRepository.ExistsAsync(id))
                {
                    _logger.LogWarning("Aliquota fiscale con ID {TaxRateId} non trovata per l'aggiornamento", id);
                    return NotFound($"Aliquota fiscale con ID {id} non trovata.");
                }

                // Verifica se esiste già un'altra aliquota con lo stesso valore
                if (await _taxRatesRepository.ExistsByAliquotaAsync(taxRateDto.Aliquota, id))
                {
                    _logger.LogWarning("Tentativo di aggiornare l'aliquota con un valore già esistente: {Aliquota}", taxRateDto.Aliquota);
                    return BadRequest($"Esiste già un'altra aliquota con valore {taxRateDto.Aliquota}.");
                }

                _logger.LogInformation("Aggiornamento aliquota fiscale con ID: {TaxRateId}", id);
                await _taxRatesRepository.UpdateAsync(taxRateDto);

                _logger.LogInformation("Aliquota fiscale con ID {TaxRateId} aggiornata con successo", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'aliquota fiscale con ID: {TaxRateId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Elimina un'aliquota fiscale
        /// </summary>
        /// <param name="id">ID dell'aliquota fiscale da eliminare</param>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (!await _taxRatesRepository.ExistsAsync(id))
                {
                    _logger.LogWarning("Aliquota fiscale con ID {TaxRateId} non trovata per l'eliminazione", id);
                    return NotFound($"Aliquota fiscale con ID {id} non trovata.");
                }

                _logger.LogInformation("Eliminazione aliquota fiscale con ID: {TaxRateId}", id);
                await _taxRatesRepository.DeleteAsync(id);

                _logger.LogInformation("Aliquota fiscale con ID {TaxRateId} eliminata con successo", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'aliquota fiscale con ID: {TaxRateId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }
    }
}
