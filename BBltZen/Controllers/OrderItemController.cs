using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Database;
using Microsoft.EntityFrameworkCore;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class OrderItemController : SecureBaseController
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly BubbleTeaContext _context;

        public OrderItemController(
            IOrderItemRepository orderItemRepository,
            BubbleTeaContext context,
            IWebHostEnvironment environment,
            ILogger<OrderItemController> logger)
            : base(environment, logger)
        {
            _orderItemRepository = orderItemRepository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Recupero di tutti gli order items");
                var orderItems = await _orderItemRepository.GetAllAsync();

                // ✅ Log per audit
                LogAuditTrail("GET_ALL_ORDER_ITEMS", "OrderItem", "All");

                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli order items");
                return SafeInternalError<IEnumerable<OrderItemDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli order items: {ex.Message}"
                        : "Errore interno nel recupero order items"
                );
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<OrderItemDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<OrderItemDTO>("ID order item non valido");

                _logger.LogInformation("Recupero order item con ID: {OrderItemId}", id);
                var orderItem = await _orderItemRepository.GetByIdAsync(id);

                if (orderItem == null)
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato", id);
                    return SafeNotFound<OrderItemDTO>("Order item");
                }

                // ✅ Log per audit
                LogAuditTrail("GET_ORDER_ITEM_BY_ID", "OrderItem", id.ToString());

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError<OrderItemDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero dell'order item {id}: {ex.Message}"
                        : "Errore interno nel recupero order item"
                );
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult<OrderItemDTO>> Create([FromBody] OrderItemDTO orderItemDto)
        {
            try
            {
                if (!IsModelValid(orderItemDto))
                    return SafeBadRequest<OrderItemDTO>("Dati order item non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var ordineExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderItemDto.OrdineId);
                if (!ordineExists)
                    return SafeBadRequest<OrderItemDTO>("Ordine non trovato");

                var articoloExists = await _context.Articolo.AnyAsync(a => a.ArticoloId == orderItemDto.ArticoloId);
                if (!articoloExists)
                    return SafeBadRequest<OrderItemDTO>("Articolo non trovato");

                var taxRateExists = await _context.TaxRates.AnyAsync(t => t.TaxRateId == orderItemDto.TaxRateId);
                if (!taxRateExists)
                    return SafeBadRequest<OrderItemDTO>("Tax rate non trovato");

                // Verifica se esiste già un order item con lo stesso ID
                if (orderItemDto.OrderItemId > 0 && await _orderItemRepository.ExistsAsync(orderItemDto.OrderItemId))
                    return SafeBadRequest<OrderItemDTO>("Esiste già un order item con questo ID");

                _logger.LogInformation("Creazione nuovo order item per ordine: {OrdineId}", orderItemDto.OrdineId);
                await _orderItemRepository.AddAsync(orderItemDto);

                // ✅ Audit trail e security event
                LogAuditTrail("CREATE_ORDER_ITEM", "OrderItem", orderItemDto.OrderItemId.ToString());
                LogSecurityEvent("OrderItemCreated", new
                {
                    OrderItemId = orderItemDto.OrderItemId,
                    OrdineId = orderItemDto.OrdineId,
                    ArticoloId = orderItemDto.ArticoloId,
                    Quantita = orderItemDto.Quantita,
                    PrezzoUnitario = orderItemDto.PrezzoUnitario,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                _logger.LogInformation("Order item creato con ID: {OrderItemId}", orderItemDto.OrderItemId);
                return CreatedAtAction(nameof(GetById), new { id = orderItemDto.OrderItemId }, orderItemDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione order item");
                return SafeInternalError<OrderItemDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore database nella creazione order item: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nella creazione order item"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione order item");
                return SafeBadRequest<OrderItemDTO>(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'order item");
                return SafeInternalError<OrderItemDTO>(
                    _environment.IsDevelopment()
                        ? $"Errore durante la creazione dell'order item: {ex.Message}"
                        : "Errore interno nella creazione order item"
                );
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Update(int id, [FromBody] OrderItemDTO orderItemDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID order item non valido");

                if (id != orderItemDto.OrderItemId)
                    return SafeBadRequest("ID order item non corrispondente");

                if (!IsModelValid(orderItemDto))
                    return SafeBadRequest("Dati order item non validi");

                // ✅ Controlli avanzati con BubbleTeaContext
                var ordineExists = await _context.Ordine.AnyAsync(o => o.OrdineId == orderItemDto.OrdineId);
                if (!ordineExists)
                    return SafeBadRequest("Ordine non trovato");

                var articoloExists = await _context.Articolo.AnyAsync(a => a.ArticoloId == orderItemDto.ArticoloId);
                if (!articoloExists)
                    return SafeBadRequest("Articolo non trovato");

                var taxRateExists = await _context.TaxRates.AnyAsync(t => t.TaxRateId == orderItemDto.TaxRateId);
                if (!taxRateExists)
                    return SafeBadRequest("Tax rate non trovato");

                var existing = await _orderItemRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato per l'aggiornamento", id);
                    return SafeNotFound("Order item");
                }

                _logger.LogInformation("Aggiornamento order item con ID: {OrderItemId}", id);
                await _orderItemRepository.UpdateAsync(orderItemDto);

                // ✅ Audit trail e security event
                LogAuditTrail("UPDATE_ORDER_ITEM", "OrderItem", orderItemDto.OrderItemId.ToString());
                LogSecurityEvent("OrderItemUpdated", new
                {
                    OrderItemId = orderItemDto.OrderItemId,
                    OrdineId = orderItemDto.OrdineId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"Quantità: {existing.Quantita} → {orderItemDto.Quantita}, Prezzo: {existing.PrezzoUnitario} → {orderItemDto.PrezzoUnitario}"
                });

                _logger.LogInformation("Order item con ID {OrderItemId} aggiornato con successo", id);
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento order item {OrderItemId}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'aggiornamento order item {id}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'aggiornamento order item"
                );
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nell'aggiornamento order item {OrderItemId}", id);
                return SafeBadRequest(
                    _environment.IsDevelopment()
                        ? $"Dati non validi: {argEx.Message}"
                        : "Dati non validi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'aggiornamento dell'order item {id}: {ex.Message}"
                        : "Errore interno nell'aggiornamento order item"
                );
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID order item non valido");

                var existing = await _orderItemRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato per l'eliminazione", id);
                    return SafeNotFound("Order item");
                }

                _logger.LogInformation("Eliminazione order item con ID: {OrderItemId}", id);
                await _orderItemRepository.DeleteAsync(id);

                // ✅ Audit trail e security event
                LogAuditTrail("DELETE_ORDER_ITEM", "OrderItem", id.ToString());
                LogSecurityEvent("OrderItemDeleted", new
                {
                    OrderItemId = id,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                _logger.LogInformation("Order item con ID {OrderItemId} eliminato con successo", id);
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione order item {OrderItemId}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore database nell'eliminazione order item {id}: {dbEx.InnerException?.Message ?? dbEx.Message}"
                        : "Errore di sistema nell'eliminazione order item"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError(
                    _environment.IsDevelopment()
                        ? $"Errore durante l'eliminazione dell'order item {id}: {ex.Message}"
                        : "Errore interno nell'eliminazione order item"
                );
            }
        }

        [HttpGet("ordine/{ordineId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByOrderId(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID ordine non valido");

                // ✅ Controllo esistenza ordine
                var ordineExists = await _context.Ordine.AnyAsync(o => o.OrdineId == ordineId);
                if (!ordineExists)
                    return SafeNotFound<IEnumerable<OrderItemDTO>>("Ordine non trovato");

                _logger.LogInformation("Recupero order items per ordine ID: {OrdineId}", ordineId);
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(ordineId);

                // ✅ Log per audit
                LogAuditTrail("GET_ORDER_ITEMS_BY_ORDER", "OrderItem", ordineId.ToString());

                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per ordine ID: {OrdineId}", ordineId);
                return SafeInternalError<IEnumerable<OrderItemDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli order items per ordine {ordineId}: {ex.Message}"
                        : "Errore interno nel recupero order items per ordine"
                );
            }
        }

        [HttpGet("articolo/{articoloId}")]
        [AllowAnonymous] // ✅ AGGIUNTO ESPLICITAMENTE
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByArticoloId(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID articolo non valido");

                // ✅ Controllo esistenza articolo
                var articoloExists = await _context.Articolo.AnyAsync(a => a.ArticoloId == articoloId);
                if (!articoloExists)
                    return SafeNotFound<IEnumerable<OrderItemDTO>>("Articolo non trovato");

                _logger.LogInformation("Recupero order items per articolo ID: {ArticoloId}", articoloId);
                var orderItems = await _orderItemRepository.GetByArticoloIdAsync(articoloId);

                // ✅ Log per audit
                LogAuditTrail("GET_ORDER_ITEMS_BY_ARTICOLO", "OrderItem", articoloId.ToString());

                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per articolo ID: {ArticoloId}", articoloId);
                return SafeInternalError<IEnumerable<OrderItemDTO>>(
                    _environment.IsDevelopment()
                        ? $"Errore durante il recupero degli order items per articolo {articoloId}: {ex.Message}"
                        : "Errore interno nel recupero order items per articolo"
                );
            }
        }
    }
}