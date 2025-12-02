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
        //[HttpGet]
        //[AllowAnonymous] // ✅ OVERRIDE PER ACCESSO PUBBLICO
        //public async Task<ActionResult<IEnumerable<TaxRatesDTO>>> GetAll()
        //{
        //    try
        //    {
        //        var taxRates = await _repository.GetAllAsync();
        //        return Ok(taxRates);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante il recupero di tutte le aliquote fiscali");
        //        return SafeInternalError<IEnumerable<TaxRatesDTO>>(ex.Message);
        //    }
        //}

        // GET: api/TaxRates/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetById([FromQuery] int? id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE ID NULL → LISTA COMPLETA PAGINATA
                if (!id.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovate {result.TotalCount} aliquote",
                        result.Data,
                        Pagination = new { result.Page, result.PageSize, result.TotalCount, result.TotalPages }
                    });
                }

                // ✅ SE ID VALORIZZATO → SINGOLO ELEMENTO
                if (id <= 0) return SafeBadRequest("ID aliquota non valido");

                var taxRate = await _repository.GetByIdAsync(id.Value);
                return taxRate == null ? SafeNotFound("Aliquota fiscale") : Ok(taxRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'aliquota fiscale {Id}", id);
                return SafeInternalError("Errore durante il recupero dell'aliquota");
            }
        }

        // GET: api/TaxRates/aliquota/22.00
        [HttpGet("aliquota")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByAliquota([FromQuery] decimal? aliquota, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE ALIQUOTA NULL → LISTA COMPLETA PAGINATA
                if (!aliquota.HasValue)
                {
                    var result = await _repository.GetAllAsync(page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovate {result.TotalCount} aliquote",
                        result.Data,
                        Pagination = new { result.Page, result.PageSize, result.TotalCount, result.TotalPages }
                    });
                }

                // ✅ VALIDAZIONE ALIQUOTA
                if (aliquota < 0 || aliquota > 100)
                    return SafeBadRequest("Valore aliquota non valido (deve essere tra 0 e 100)");

                // ✅ RICERCA PAGINATA PER ALIQUOTA SPECIFICA
                var resultByAliquota = await _repository.GetByAliquotaAsync(aliquota, page, pageSize);

                resultByAliquota.Message = resultByAliquota.TotalCount > 0
                    ? $"Trovate {resultByAliquota.TotalCount} aliquote al {aliquota}%"
                    : $"Nessuna aliquota trovata al {aliquota}%";

                return Ok(resultByAliquota);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle aliquote con valore {Aliquota}", aliquota);
                return SafeInternalError("Errore durante il recupero delle aliquote");
            }
        }

        // POST: api/TaxRates
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<TaxRatesDTO>> Create([FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                if (!IsModelValid(taxRateDto))
                    return SafeBadRequest("Dati aliquota non validi");

                // ✅ VALIDAZIONE DUPLICATI (aliquota + descrizione)
                if (await _repository.ExistsByAliquotaDescrizioneAsync(taxRateDto.Aliquota, taxRateDto.Descrizione))
                    return SafeBadRequest($"Esiste già un'aliquota {taxRateDto.Aliquota}% con descrizione '{taxRateDto.Descrizione}'");

                var createdTaxRate = await _repository.AddAsync(taxRateDto);

                LogAuditTrail("CREATE", "TaxRates", createdTaxRate.TaxRateId.ToString());
                LogSecurityEvent("TaxRateCreated", new
                {
                    createdTaxRate.TaxRateId,
                    createdTaxRate.Aliquota,
                    createdTaxRate.Descrizione,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return CreatedAtAction(nameof(GetById), new { id = createdTaxRate.TaxRateId }, createdTaxRate);
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante la creazione dell'aliquota");
                return SafeInternalError("Errore durante il salvataggio");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'aliquota");
                return SafeInternalError("Errore durante la creazione");
            }
        }

        // PUT: api/TaxRates/5
        [HttpPut("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(int id, [FromBody] TaxRatesDTO taxRateDto)
        {
            try
            {
                // ✅ 1. VALIDAZIONE ID
                if (id <= 0)
                    return SafeBadRequest("ID aliquota non valido");

                if (id != taxRateDto.TaxRateId)
                    return SafeBadRequest("ID aliquota non corrispondente");

                // ✅ 2. VALIDAZIONE MODEL
                if (!IsModelValid(taxRateDto))
                    return SafeBadRequest("Dati aliquota non validi");

                // ✅ 3. VERIFICA ESISTENZA
                if (!await _repository.ExistsAsync(id))
                    return SafeNotFound("Aliquota fiscale");

                // ✅ 4. CONTROLLO DUPLICATI (aliquota + descrizione) ESCLUDENDO QUESTO ID
                if (await _repository.ExistsByAliquotaDescrizioneAsync(taxRateDto.Aliquota, taxRateDto.Descrizione, id))
                    return SafeBadRequest($"Esiste già un'aliquota {taxRateDto.Aliquota}% con descrizione '{taxRateDto.Descrizione}'");

                // ✅ 5. ESEGUI UPDATE
                await _repository.UpdateAsync(taxRateDto);

                // ✅ 6. AUDIT TRAIL
                LogAuditTrail("UPDATE", "TaxRates", taxRateDto.TaxRateId.ToString());
                LogSecurityEvent("TaxRateUpdated", new
                {
                    taxRateDto.TaxRateId,
                    taxRateDto.Aliquota,
                    UserId = GetCurrentUserIdOrDefault()
                });

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return SafeBadRequest(argEx.Message);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database durante l'aggiornamento dell'aliquota {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'aliquota {Id}", id);
                return SafeInternalError("Errore durante l'aggiornamento");
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0) return SafeBadRequest("ID non valido");

                var taxRate = await _repository.GetByIdAsync(id);
                if (taxRate == null) return SafeNotFound("Aliquota");

                // ✅ CONTROLLO DIPENDENZE QUI, come Tavolo
                bool hasOrderItems = await _context.OrderItem.AnyAsync(oi => oi.TaxRateId == id);
                if (hasOrderItems) return SafeBadRequest("Ci sono order items associati");

                await _repository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore eliminazione ID {Id}", id);
                return SafeInternalError("Errore");
            }
        }

        // ✅ NUOVI ENDPOINT PER FRONTEND

        // GET: api/TaxRates/frontend
        //[HttpGet("frontend")]
        //[AllowAnonymous] // ✅ ACCESSO PUBBLICO PER CLIENTI
        //public async Task<ActionResult<IEnumerable<TaxRatesFrontendDTO>>> GetAllPerFrontend()
        //{
        //    try
        //    {
        //        var taxRates = await _repository.GetAllPerFrontendAsync();
        //        return Ok(taxRates);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Errore durante il recupero delle aliquote per frontend");

        //        if (_environment.IsDevelopment())
        //            return SafeInternalError<IEnumerable<TaxRatesFrontendDTO>>($"Errore: {ex.Message}");
        //        else
        //            return SafeInternalError<IEnumerable<TaxRatesFrontendDTO>>("Errore durante il caricamento delle aliquote");
        //    }
        //}

        // GET: api/TaxRates/frontend/aliquota/22.00
        [HttpGet("frontend/aliquota")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByAliquotaPerFrontend([FromQuery] decimal? aliquota, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // ✅ SE ALIQUOTA NULL → LISTA FRONTEND COMPLETA PAGINATA
                if (!aliquota.HasValue)
                {
                    var res = await _repository.GetByAliquotaPerFrontendAsync(null, page, pageSize);
                    return Ok(new
                    {
                        Message = $"Trovate {res.TotalCount} aliquote",
                        res.Data,
                        Pagination = new { res.Page, res.PageSize, res.TotalCount, res.TotalPages }
                    });
                }

                // ✅ VALIDAZIONE ALIQUOTA
                if (aliquota < 0 || aliquota > 100)
                    return SafeBadRequest("Valore aliquota non valido (deve essere tra 0 e 100)");

                // ✅ RICERCA FRONTEND PAGINATA PER ALIQUOTA SPECIFICA
                var result = await _repository.GetByAliquotaPerFrontendAsync(aliquota, page, pageSize);

                result.Message = result.TotalCount > 0
                    ? $"Trovate {result.TotalCount} aliquote al {aliquota}%"
                    : $"Nessuna aliquota trovata al {aliquota}%";

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle aliquote frontend con valore {Aliquota}", aliquota);
                return SafeInternalError("Errore durante il caricamento delle aliquote");
            }
        }
    }
}