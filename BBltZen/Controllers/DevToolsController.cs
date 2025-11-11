using BBltZen.Services;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ Commentato per testing Swagger
    public class DevToolsController : SecureBaseController
    {
        private readonly DatabaseSeeder _seeder;
        private readonly BubbleTeaContext _context;

        public DevToolsController(
            DatabaseSeeder seeder,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<DevToolsController> logger)
            : base(environment, logger)
        {
            _seeder = seeder;
            _context = context;
        }

        [HttpPost("seed")]
        [AllowAnonymous] // ✅ Esplicito per testing Swagger
        public async Task<ActionResult> SeedDatabase([FromQuery] bool forceReset = false)
        {
            const string operation = "DatabaseSeeding";
            const string entityType = "Database";
            const string entityId = "ALL";

            try
            {
                LogSecurityEvent("SeedDatabase_Attempt", new { ForceReset = forceReset });
                LogAuditTrail(operation, entityType, entityId);

                // ✅ Controllo ambiente
                if (!_environment.IsDevelopment())
                {
                    LogSecurityEvent("SeedDatabase_EnvironmentViolation");
                    return SafeBadRequest("Seeding allowed only in development environment");
                }

                // ✅ Validazione stato database
                if (!await IsDatabaseAccessible())
                {
                    return SafeInternalError("Database non accessibile");
                }

                // ✅ Esecuzione seeding
                await _seeder.SeedAsync(forceReset);

                LogSecurityEvent("SeedDatabase_Success", new { ForceReset = forceReset, Timestamp = DateTime.UtcNow });

                // ✅ Messaggi differenziati per ambiente
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        message = "Database seeded successfully!",
                        details = $"Force reset: {forceReset}",
                        timestamp = DateTime.UtcNow,
                        recordsAffected = await GetTotalRecordsCount()
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Operazione completata",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error during seeding");
                LogSecurityEvent("SeedDatabase_DbUpdateError", new { Error = dbEx.Message });

                if (_environment.IsDevelopment())
                {
                    return SafeInternalError($"Errore database: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                else
                {
                    return SafeInternalError("Errore durante l'operazione di seeding");
                }
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argument error during seeding");
                LogSecurityEvent("SeedDatabase_ArgumentError", new { Error = argEx.Message });

                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during database seeding");
                LogSecurityEvent("SeedDatabase_UnexpectedError", new { Error = ex.Message });

                return SafeInternalError($"Errore imprevisto: {ex.Message}");
            }
        }

        [HttpPost("reset")]
        [AllowAnonymous] // ✅ Esplicito per testing Swagger
        public async Task<ActionResult> ResetDatabase()
        {
            const string operation = "DatabaseReset";
            const string entityType = "Database";
            const string entityId = "ALL";

            try
            {
                LogSecurityEvent("ResetDatabase_Attempt");
                LogAuditTrail(operation, entityType, entityId);

                // ✅ Controllo ambiente
                if (!_environment.IsDevelopment())
                {
                    LogSecurityEvent("ResetDatabase_EnvironmentViolation");
                    return SafeBadRequest("Reset allowed only in development environment");
                }

                // ✅ Validazione stato database
                if (!await IsDatabaseAccessible())
                {
                    return SafeInternalError("Database non accessibile");
                }

                // ✅ Esecuzione reset completo
                await _seeder.SeedAsync(forceReset: true);

                LogSecurityEvent("ResetDatabase_Success", new { Timestamp = DateTime.UtcNow });

                // ✅ Messaggi differenziati per ambiente
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        message = "Database reset and seeded successfully!",
                        timestamp = DateTime.UtcNow,
                        recordsAffected = await GetTotalRecordsCount(),
                        resetType = "FullReset"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Operazione di reset completata",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error during reset");
                LogSecurityEvent("ResetDatabase_DbUpdateError", new { Error = dbEx.Message });

                if (_environment.IsDevelopment())
                {
                    return SafeInternalError($"Errore database durante reset: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                else
                {
                    return SafeInternalError("Errore durante l'operazione di reset");
                }
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argument error during reset");
                LogSecurityEvent("ResetDatabase_ArgumentError", new { Error = argEx.Message });

                return SafeBadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during database reset");
                LogSecurityEvent("ResetDatabase_UnexpectedError", new { Error = ex.Message });

                return SafeInternalError($"Errore imprevisto durante reset: {ex.Message}");
            }
        }

        [HttpGet("status")]
        [AllowAnonymous] // ✅ Esplicito per testing Swagger
        public async Task<ActionResult> GetDatabaseStatus()
        {
            try
            {
                LogSecurityEvent("DatabaseStatus_Check");

                var status = new
                {
                    Environment = _environment.EnvironmentName,
                    IsDevelopment = _environment.IsDevelopment(),
                    DatabaseAccessible = await IsDatabaseAccessible(),
                    TotalRecords = await GetTotalRecordsCount(),
                    Timestamp = DateTime.UtcNow,
                };

                // ✅ Messaggi differenziati per ambiente
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        message = "Database status retrieved successfully",
                        status = status,
                        details = "Development environment - detailed info available"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        message = "Stato sistema verificato",
                        status = new
                        {
                            status.Environment,
                            status.DatabaseAccessible,
                            status.Timestamp
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database status");
                LogSecurityEvent("DatabaseStatus_Error", new { Error = ex.Message });

                return SafeInternalError($"Errore durante il check dello status: {ex.Message}");
            }
        }

        // ✅ METODI PRIVATI DI SUPPORTO - CORRETTI

        private async Task<bool> IsDatabaseAccessible()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private async Task<int> GetTotalRecordsCount()
        {
            try
            {
                // ✅ CORREZIONE: Usa i nomi corretti al singolare
                var counts = new
                {
                    Articoli = await _context.Articolo.CountAsync(),
                    Clienti = await _context.Cliente.CountAsync(),
                    Ordini = await _context.Ordine.CountAsync(),
                    Ingredienti = await _context.Ingrediente.CountAsync()
                };

                return counts.Articoli + counts.Clienti + counts.Ordini + counts.Ingredienti;
            }
            catch
            {
                return -1; // Indica errore nel conteggio
            }
        }
    }
}