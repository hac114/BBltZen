using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using BBltZen;

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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetAll()
        {
            try
            {
                var orderItems = await _orderItemRepository.GetAllAsync();
                LogAuditTrail("GET_ALL_ORDER_ITEMS", "OrderItem", "All");
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli order items");
                return SafeInternalError<IEnumerable<OrderItemDTO>>("Errore durante il recupero degli order items");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<OrderItemDTO>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest<OrderItemDTO>("ID order item non valido");

                var orderItem = await _orderItemRepository.GetByIdAsync(id);

                if (orderItem == null)
                    return SafeNotFound<OrderItemDTO>("Order item");

                LogAuditTrail("GET_ORDER_ITEM_BY_ID", "OrderItem", id.ToString());
                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError<OrderItemDTO>("Errore durante il recupero dell'order item");
            }
        }

        [HttpPost]
        //[Authorize(Roles = "admin,barista,cliente")]
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

                _logger.LogInformation("Creazione nuovo order item per ordine: {OrdineId}", orderItemDto.OrdineId);

                // ✅ CORRETTO: Assegna il risultato di AddAsync
                var result = await _orderItemRepository.AddAsync(orderItemDto);

                // ✅ Audit trail e security event
                LogAuditTrail("CREATE_ORDER_ITEM", "OrderItem", result.OrderItemId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("OrderItemCreated", new
                {
                    result.OrderItemId,
                    result.OrdineId,
                    result.ArticoloId,
                    result.Quantita,
                    result.PrezzoUnitario,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                _logger.LogInformation("Order item creato con ID: {OrderItemId}", result.OrderItemId);
                return CreatedAtAction(nameof(GetById), new { id = result.OrderItemId }, result);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nella creazione order item");
                return SafeInternalError<OrderItemDTO>("Errore durante il salvataggio dei dati");
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "Argomento non valido nella creazione order item");
                return SafeBadRequest<OrderItemDTO>(argEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'order item");
                return SafeInternalError<OrderItemDTO>("Errore durante la creazione dell'order item");
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,barista")]
        public async Task<ActionResult> Update(int id, [FromBody] OrderItemDTO orderItemDto)
        {
            try
            {
                if (id <= 0 || id != orderItemDto.OrderItemId || !IsModelValid(orderItemDto))
                    return SafeBadRequest("Dati order item non validi");

                // ✅ Controlli avanzati combinati
                if (!await _context.Ordine.AnyAsync(o => o.OrdineId == orderItemDto.OrdineId) ||
                    !await _context.Articolo.AnyAsync(a => a.ArticoloId == orderItemDto.ArticoloId) ||
                    !await _context.TaxRates.AnyAsync(t => t.TaxRateId == orderItemDto.TaxRateId))
                    return SafeBadRequest("Ordine, articolo o tax rate non trovato");

                var existing = await _orderItemRepository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Order item");

                await _orderItemRepository.UpdateAsync(orderItemDto);

                LogAuditTrail("UPDATE_ORDER_ITEM", "OrderItem", orderItemDto.OrderItemId.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("OrderItemUpdated", new
                {
                    orderItemDto.OrderItemId,
                    orderItemDto.OrdineId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Changes = $"Quantità: {existing.Quantita} → {orderItemDto.Quantita}, Prezzo: {existing.PrezzoUnitario} → {orderItemDto.PrezzoUnitario}"
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'aggiornamento order item {OrderItemId}", id);
                return SafeInternalError("Errore durante l'aggiornamento dei dati");
            }
            catch (ArgumentException)
            {
                return SafeBadRequest("Order item non trovato");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'order item");
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin,barista")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID order item non valido");

                var existing = await _orderItemRepository.GetByIdAsync(id);
                if (existing == null)
                    return SafeNotFound("Order item");

                await _orderItemRepository.DeleteAsync(id);

                LogAuditTrail("DELETE_ORDER_ITEM", "OrderItem", id.ToString());

                // ✅ CORRETTO: Sintassi semplificata
                LogSecurityEvent("OrderItemDeleted", new
                {
                    OrderItemId = id,
                    existing.OrdineId,
                    User = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });

                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Errore database nell'eliminazione order item {OrderItemId}", id);
                return SafeInternalError("Errore durante l'eliminazione dei dati");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'order item");
            }
        }

        [HttpGet("ordine/{ordineId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByOrderId(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID ordine non valido");

                // ✅ Controllo esistenza ordine
                if (!await _context.Ordine.AnyAsync(o => o.OrdineId == ordineId))
                    return SafeNotFound<IEnumerable<OrderItemDTO>>("Ordine non trovato");

                var orderItems = await _orderItemRepository.GetByOrderIdAsync(ordineId);
                LogAuditTrail("GET_ORDER_ITEMS_BY_ORDER", "OrderItem", ordineId.ToString());
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per ordine ID: {OrdineId}", ordineId);
                return SafeInternalError<IEnumerable<OrderItemDTO>>("Errore durante il recupero degli order items");
            }
        }

        [HttpGet("articolo/{articoloId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByArticoloId(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID articolo non valido");

                // ✅ Controllo esistenza articolo
                if (!await _context.Articolo.AnyAsync(a => a.ArticoloId == articoloId))
                    return SafeNotFound<IEnumerable<OrderItemDTO>>("Articolo non trovato");

                var orderItems = await _orderItemRepository.GetByArticoloIdAsync(articoloId);
                LogAuditTrail("GET_ORDER_ITEMS_BY_ARTICOLO", "OrderItem", articoloId.ToString());
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per articolo ID: {ArticoloId}", articoloId);
                return SafeInternalError<IEnumerable<OrderItemDTO>>("Errore durante il recupero degli order items");
            }
        }
    }    
}