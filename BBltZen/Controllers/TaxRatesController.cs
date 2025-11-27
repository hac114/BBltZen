// BBltZen/Controllers/TaxRatesController.cs
using Database;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class TaxRatesController(
    ITaxRatesRepository repository,
    BubbleTeaContext context,
    IWebHostEnvironment environment,
    ILogger<TaxRatesController> logger)
    : SecureBaseController(environment, logger)
    {
        private readonly ITaxRatesRepository _repository = repository;
        private readonly BubbleTeaContext _context = context;

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

                // ✅ CONTROLLO VINCOLI REFERENZIALI NEL CONTROLLER
                bool hasOrderItems = await _context.OrderItem.AnyAsync(oi => oi.TaxRateId == id);

                if (hasOrderItems)
                    return SafeBadRequest("Impossibile eliminare l'aliquota: sono presenti order items associati");

                await _repository.DeleteAsync(id);

                // ✅ SEMPLIFICATO: Audit trail
                LogAuditTrail("DELETE", "TaxRates", id.ToString());
                LogSecurityEvent("TaxRateDeleted", new
                {
                    id,
                    taxRate.Aliquota,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (InvalidOperationException invOpEx) // ✅ INMEMORY EXCEPTION
            {
                if (_environment.IsDevelopment())
                    return SafeBadRequest($"Errore eliminazione: {invOpEx.Message}");
                else
                    return SafeBadRequest("Impossibile eliminare l'aliquota: sono presenti dipendenze");
            }
            catch (DbUpdateException dbEx) // ✅ DATABASE REAL EXCEPTION
            {
                _logger.LogError(dbEx, "Errore database durante l'eliminazione dell'aliquota fiscale {Id}", id);

                if (_environment.IsDevelopment())
                    return SafeBadRequest($"Errore database: {dbEx.Message}");
                else
                    return SafeBadRequest("Impossibile eliminare l'aliquota: sono presenti dipendenze");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'aliquota fiscale {Id}", id);
                return SafeInternalError("Errore durante l'eliminazione");
            }
        }

        // ✅ NUOVI ENDPOINT PER FRONTEND

        // GET: api/TaxRates/frontend
        [HttpGet("frontend")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<IEnumerable<TaxRatesFrontendDTO>>> GetAllPerFrontend()
        {
            try
            {
                var taxRates = await _repository.GetAllPerFrontendAsync();
                return Ok(taxRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle aliquote per frontend");

                if (_environment.IsDevelopment())
                    return SafeInternalError<IEnumerable<TaxRatesFrontendDTO>>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<IEnumerable<TaxRatesFrontendDTO>>("Errore durante il caricamento delle aliquote");
            }
        }

        // GET: api/TaxRates/frontend/aliquota/22.00
        [HttpGet("frontend/aliquota/{aliquota}")]
        [AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        public async Task<ActionResult<TaxRatesFrontendDTO>> GetByAliquotaPerFrontend(decimal aliquota)
        {
            try
            {
                if (aliquota < 0 || aliquota > 100)
                    return SafeBadRequest<TaxRatesFrontendDTO>("Valore aliquota non valido (deve essere tra 0 e 100)");

                var taxRate = await _repository.GetByAliquotaPerFrontendAsync(aliquota);

                if (taxRate == null)
                    return SafeNotFound<TaxRatesFrontendDTO>("Aliquota fiscale");

                return Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale con valore {Aliquota} per frontend", aliquota);

                if (_environment.IsDevelopment())
                    return SafeInternalError<TaxRatesFrontendDTO>($"Errore: {ex.Message}");
                else
                    return SafeInternalError<TaxRatesFrontendDTO>("Errore durante il caricamento dell'aliquota");
            }
        }
    }
}