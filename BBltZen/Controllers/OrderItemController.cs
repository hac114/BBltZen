using Microsoft.AspNetCore.Mvc;
using DTO;
using Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // ✅ COMMENTATO PER TEST CON SWAGGER
    public class OrderItemController : SecureBaseController
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemController(
            IOrderItemRepository orderItemRepository,
            IWebHostEnvironment environment,
            ILogger<OrderItemController> logger)
            : base(environment, logger)
        {
            _orderItemRepository = orderItemRepository;
        }

        /// <summary>
        /// Ottiene tutti gli order items
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Recupero di tutti gli order items");
                var orderItems = await _orderItemRepository.GetAllAsync();
                return Ok(orderItems);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli order items");
                return SafeInternalError("Errore durante il recupero degli order items");
            }
        }

        /// <summary>
        /// Ottiene un order item specifico tramite ID
        /// </summary>
        /// <param name="id">ID dell'order item</param>
        [HttpGet("{id}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
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

                return Ok(orderItem);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError("Errore durante il recupero dell'order item");
            }
        }

        /// <summary>
        /// Crea un nuovo order item
        /// </summary>
        /// <param name="orderItemDto">Dati del nuovo order item</param>
        [HttpPost]
        //[Authorize(Roles = "admin,barista,cliente")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<OrderItemDTO>> Create(OrderItemDTO orderItemDto)
        {
            try
            {
                if (!IsModelValid(orderItemDto))
                    return SafeBadRequest<OrderItemDTO>("Dati order item non validi");

                // Verifica se esiste già un order item con lo stesso ID
                if (orderItemDto.OrderItemId > 0 && await _orderItemRepository.ExistsAsync(orderItemDto.OrderItemId))
                    return Conflict($"Esiste già un order item con ID {orderItemDto.OrderItemId}");

                _logger.LogInformation("Creazione nuovo order item per ordine: {OrdineId}", orderItemDto.OrdineId);
                await _orderItemRepository.AddAsync(orderItemDto);

                // ✅ Audit trail
                LogAuditTrail("CREATE_ORDER_ITEM", "OrderItem", orderItemDto.OrderItemId.ToString());
                LogSecurityEvent("OrderItemCreated", new
                {
                    OrderItemId = orderItemDto.OrderItemId,
                    OrdineId = orderItemDto.OrdineId,
                    ArticoloId = orderItemDto.ArticoloId,
                    Quantita = orderItemDto.Quantita,
                    PrezzoUnitario = orderItemDto.PrezzoUnitario,
                    User = User.Identity?.Name
                });

                _logger.LogInformation("Order item creato con ID: {OrderItemId}", orderItemDto.OrderItemId);
                return CreatedAtAction(nameof(GetById), new { id = orderItemDto.OrderItemId }, orderItemDto);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'order item");
                return SafeInternalError("Errore durante la creazione dell'order item");
            }
        }

        /// <summary>
        /// Aggiorna un order item esistente
        /// </summary>
        /// <param name="id">ID dell'order item da aggiornare</param>
        /// <param name="orderItemDto">Dati aggiornati dell'order item</param>
        [HttpPut("{id}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
        public async Task<ActionResult> Update(int id, OrderItemDTO orderItemDto)
        {
            try
            {
                if (id <= 0)
                    return SafeBadRequest("ID order item non valido");

                if (id != orderItemDto.OrderItemId)
                    return SafeBadRequest("ID order item non corrispondente");

                if (!IsModelValid(orderItemDto))
                    return SafeBadRequest("Dati order item non validi");

                var existing = await _orderItemRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato per l'aggiornamento", id);
                    return SafeNotFound("Order item");
                }

                _logger.LogInformation("Aggiornamento order item con ID: {OrderItemId}", id);
                await _orderItemRepository.UpdateAsync(orderItemDto);

                // ✅ Audit trail
                LogAuditTrail("UPDATE_ORDER_ITEM", "OrderItem", orderItemDto.OrderItemId.ToString());
                LogSecurityEvent("OrderItemUpdated", new
                {
                    OrderItemId = orderItemDto.OrderItemId,
                    OrdineId = orderItemDto.OrdineId,
                    User = User.Identity?.Name
                });

                _logger.LogInformation("Order item con ID {OrderItemId} aggiornato con successo", id);
                return NoContent();
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativo di aggiornamento di un order item non trovato {OrderItemId}", id);
                return SafeNotFound("Order item");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError("Errore durante l'aggiornamento dell'order item");
            }
        }

        /// <summary>
        /// Elimina un order item
        /// </summary>
        /// <param name="id">ID dell'order item da eliminare</param>
        [HttpDelete("{id}")]
        //[Authorize(Roles = "admin,barista")] // ✅ COMMENTATO PER TEST
        [AllowAnonymous] // ✅ TEMPORANEAMENTE PERMESSO A TUTTI PER TEST
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

                // ✅ Audit trail
                LogAuditTrail("DELETE_ORDER_ITEM", "OrderItem", id.ToString());
                LogSecurityEvent("OrderItemDeleted", new
                {
                    OrderItemId = id,
                    User = User.Identity?.Name
                });

                _logger.LogInformation("Order item con ID {OrderItemId} eliminato con successo", id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'order item con ID: {OrderItemId}", id);
                return SafeInternalError("Errore durante l'eliminazione dell'order item");
            }
        }

        /// <summary>
        /// Ottiene tutti gli order items di un ordine specifico
        /// </summary>
        /// <param name="ordineId">ID dell'ordine</param>
        [HttpGet("ordine/{ordineId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByOrderId(int ordineId)
        {
            try
            {
                if (ordineId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID ordine non valido");

                _logger.LogInformation("Recupero order items per ordine ID: {OrdineId}", ordineId);
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(ordineId);
                return Ok(orderItems);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per ordine ID: {OrdineId}", ordineId);
                return SafeInternalError("Errore durante il recupero degli order items per ordine");
            }
        }

        /// <summary>
        /// Ottiene tutti gli order items di un articolo specifico
        /// </summary>
        /// <param name="articoloId">ID dell'articolo</param>
        [HttpGet("articolo/{articoloId}")]
        [AllowAnonymous] // ✅ PERMESSO A TUTTI PER TEST
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByArticoloId(int articoloId)
        {
            try
            {
                if (articoloId <= 0)
                    return SafeBadRequest<IEnumerable<OrderItemDTO>>("ID articolo non valido");

                _logger.LogInformation("Recupero order items per articolo ID: {ArticoloId}", articoloId);
                var orderItems = await _orderItemRepository.GetByArticoloIdAsync(articoloId);
                return Ok(orderItems);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per articolo ID: {ArticoloId}", articoloId);
                return SafeInternalError("Errore durante il recupero degli order items per articolo");
            }
        }
    }
}