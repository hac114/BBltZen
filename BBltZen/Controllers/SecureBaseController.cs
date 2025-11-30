using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ Commentato per testing Swagger
    public abstract class SecureBaseController : ControllerBase
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly ILogger _logger;

        protected SecureBaseController(IWebHostEnvironment environment, ILogger logger)
        {
            _environment = environment;
            _logger = logger;
        }

        // ✅ METODI PER ActionResult<T> (GENERICI)
        protected ActionResult<T> SafeNotFound<T>(string entity = "Risorsa")
        {
            if (_environment.IsDevelopment())
                return NotFound($"{entity} non trovato");
            else
                return NotFound("Operazione completata");
        }

        protected ActionResult<T> SafeBadRequest<T>(string message)
        {
            if (_environment.IsDevelopment())
                return BadRequest(message);
            else
                return BadRequest("Richiesta non valida");
        }

        protected ActionResult<T> SafeInternalError<T>(string message)
        {
            if (_environment.IsDevelopment())
                return StatusCode(500, message);
            else
                return StatusCode(500, "Si è verificato un errore. Riprova più tardi.");
        }

        // ✅ Per metodi che restituiscono DTO specifici
        protected ActionResult<T> SafeBadRequest<T>(T resultDto) where T : class
        {
            return BadRequest(resultDto);
        }

        // ✅ Per metodi che non restituiscono dati (void)
        protected ActionResult SafeNotFound(string entity = "Risorsa")
        {
            if (_environment.IsDevelopment())
                return NotFound($"{entity} non trovato");
            else
                return NotFound("Operazione completata");
        }

        protected ActionResult SafeBadRequest(string message)
        {
            if (_environment.IsDevelopment())
                return BadRequest(message);
            else
                return BadRequest("Richiesta non valida");
        }

        protected ActionResult SafeInternalError(string message)
        {
            if (_environment.IsDevelopment())
                return StatusCode(500, message);
            else
                return StatusCode(500, "Si è verificato un errore. Riprova più tardi.");
        }

        // ✅ CORRETTO: Logging sicurezza con parametro nullable esplicito
        protected void LogSecurityEvent(string action, object? details = null)
        {
            string user = User.Identity?.Name ?? "Anonymous";
            string detailsText = details?.ToString() ?? "No details";

            _logger.LogInformation(
                "SECURITY: {Action} - User: {User} - Details: {Details}",
                action, user, detailsText
            );
        }

        protected void LogAuditTrail(string operation, string entityType, string entityId)
        {
            string user = User.Identity?.Name ?? "Anonymous";
            string safeEntityId = entityId ?? "Unknown";

            _logger.LogInformation(
                "AUDIT: {Operation} - Entity: {Entity} - ID: {EntityId} - User: {User} - Time: {Timestamp}",
                operation, entityType, safeEntityId, user, DateTime.UtcNow
            );
        }

        protected bool IsModelValid<T>(T model) where T : class
        {
            if (model == null)
            {
                _logger.LogWarning("Model validation failed: null model");
                return false;
            }

            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    _logger.LogWarning("Model validation failed: {Error}", validationResult.ErrorMessage);
                }
            }

            return isValid;
        }

        // ✅ CORRETTO: GetCurrentUserId con gestione null
        protected int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            _logger.LogWarning("Current user ID not found or invalid");
            return null;
        }

        // ✅ Metodo aggiuntivo per ottenere l'ID con fallback
        protected int GetCurrentUserIdOrDefault(int defaultValue = 0)
        {
            return GetCurrentUserId() ?? defaultValue;
        }
    }
}
