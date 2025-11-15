// BBltZen/Controllers/TaxRatesController.cs
using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class TaxRatesController : SecureBaseController
    {
        private readonly ITaxRatesRepository _repository;

        public TaxRatesController(
            ITaxRatesRepository repository,
            IWebHostEnvironment environment,
            ILogger<TaxRatesController> logger)
            : base(environment, logger)
        {
            _repository = repository;
        }

        // GET: api/TaxRates
        [HttpGet]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<IEnumerable<TaxRatesDTO>>> GetAll()
        {
            try
            {
                var taxRates = await _repository.GetAllAsync();
                return Ok(taxRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutte le aliquote fiscali");
                return SafeInternalError<IEnumerable<TaxRatesDTO>>(ex.Message);
            }
        }

        // GET: api/TaxRates/5
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<TaxRatesDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<TaxRatesDTO>("ID aliquota non valido");

                var taxRate = await _repository.GetByIdAsync(id);

                if (taxRate == null)
                    return SafeNotFound<TaxRatesDTO>("Aliquota fiscale");

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale {Id}", id);
                return SafeInternalError<TaxRatesDTO>(ex.Message);
            }
        }

        // GET: api/TaxRates/aliquota/22.00
        [HttpGet("aliquota/{aliquota}")]
        [AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        public async Task<ActionResult<TaxRatesDTO>> GetByAliquota(decimal aliquota)
        {
            try
            {
                if (aliquota < 0 || aliquota > 100)
                    return SafeBadRequest<TaxRatesDTO>("Valore aliquota non valido (deve essere tra 0 e 100)");

                var taxRate = await _repository.GetByAliquotaAsync(aliquota);

                if (taxRate == null)
                    return SafeNotFound<TaxRatesDTO>("Aliquota fiscale");

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale con valore {Aliquota}", aliquota);
                return SafeInternalError<TaxRatesDTO>(ex.Message);
            }
        }

        // POST: api/TaxRates
        [HttpPost]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult<TaxRatesDTO>> Create([FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                if (!IsModelValid(taxRateDto))
                    return SafeBadRequest<TaxRatesDTO>("Dati aliquota non validi");

                // Validazione aliquota univoca
                if (await _repository.ExistsByAliquotaAsync(taxRateDto.Aliquota))
                    return SafeBadRequest<TaxRatesDTO>("Aliquota già esistente");

                // ✅ CORREZIONE: AddAsync ora ritorna il DTO con i valori aggiornati
                var createdTaxRate = await _repository.AddAsync(taxRateDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("CREATE", "TaxRates", createdTaxRate.TaxRateId.ToString());
                LogSecurityEvent("TaxRateCreated", $"Created TaxRate ID: {createdTaxRate.TaxRateId}");

                return CreatedAtAction(nameof(GetById), new { id = createdTaxRate.TaxRateId }, createdTaxRate);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'aliquota fiscale");
                return SafeInternalError<TaxRatesDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante la creazione dell'aliquota fiscale");
                return SafeBadRequest<TaxRatesDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'aliquota fiscale");
                return SafeInternalError<TaxRatesDTO>(ex.Message);
            }
        }

        // PUT: api/TaxRates/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Update(int id, [FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID aliquota non valido");

                if (id != taxRateDto.TaxRateId)
                    return SafeBadRequest("ID aliquota non corrispondente");

                if (!IsModelValid(taxRateDto))
                    return SafeBadRequest("Dati aliquota non validi");

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Aliquota fiscale");

                // Validazione aliquota univoca (escludendo l'ID corrente)
                if (await _repository.ExistsByAliquotaAsync(taxRateDto.Aliquota, id))
                    return SafeBadRequest("Aliquota già esistente");

                // ✅ CORREZIONE: UpdateAsync ora aggiorna automaticamente DataAggiornamento
                await _repository.UpdateAsync(taxRateDto);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("UPDATE", "TaxRates", taxRateDto.TaxRateId.ToString());
                LogSecurityEvent("TaxRateUpdated", $"Updated TaxRate ID: {taxRateDto.TaxRateId}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'aliquota fiscale {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido durante l'aggiornamento dell'aliquota fiscale {Id}", id);
                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'aliquota fiscale {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }

        // DELETE: api/TaxRates/5
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")] // ✅ COMMENTATO PER TEST CON SWAGGER
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID aliquota non valido");

                var taxRate = await _repository.GetByIdAsync(id);
                if (taxRate == null)
                    return SafeNotFound("Aliquota fiscale");

                await _repository.DeleteAsync(id);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("DELETE", "TaxRates", id.ToString());
                LogSecurityEvent("TaxRateDeleted", $"Deleted TaxRate ID: {id}");

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'aliquota fiscale {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'aliquota fiscale {Id}", id);
                return SafeInternalError(ex.Message);
            }
        }
    }
}