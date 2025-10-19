// BBltZen/Controllers/SecureBaseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ Protezione di default per tutti i controller
    public abstract class SecureBaseController : ControllerBase
    {
        protected readonly IWebHostEnvironment _environment;
        protected readonly ILogger _logger;

        protected SecureBaseController(IWebHostEnvironment environment, ILogger logger)
        {
            _environment = environment;
            _logger = logger;
        }

        // ✅ Metodo per risposte sicure
        protected IActionResult SafeNotFound(string entity = "Risorsa")
        {
            if (_environment.IsDevelopment())
                return NotFound($"{entity} non trovato");
            else
                return NotFound("Operazione completata");
        }

        // ✅ Logging sicurezza
        protected void LogSecurityEvent(string action, object details = null)
        {
            _logger.LogInformation(
                "SECURITY: {Action} - User: {User} - Details: {Details}",
                action, User.Identity?.Name, details
            );
        }
    }
}