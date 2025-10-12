using DTO;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;

namespace BBltZen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(IOrderItemRepository orderItemRepository, ILogger<OrderItemController> logger)
        {
            _orderItemRepository = orderItemRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ottiene tutti gli order items
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Recupero di tutti gli order items");
                var orderItems = await _orderItemRepository.GetAllAsync();
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero di tutti gli order items");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottiene un order item specifico tramite ID
        /// </summary>
        /// <param name="id">ID dell'order item</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemDTO>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Recupero order item con ID: {OrderItemId}", id);
                var orderItem = await _orderItemRepository.GetByIdAsync(id);

                if (orderItem == null)
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato", id);
                    return NotFound($"OrderItem con ID {id} non trovato.");
                }

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dell'order item con ID: {OrderItemId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Crea un nuovo order item
        /// </summary>
        /// <param name="orderItemDto">Dati del nuovo order item</param>
        [HttpPost]
        public async Task<ActionResult<OrderItemDTO>> Create(OrderItemDTO orderItemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dati non validi per la creazione dell'order item");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creazione nuovo order item per ordine: {OrdineId}", orderItemDto.OrdineId);
                await _orderItemRepository.AddAsync(orderItemDto);

                _logger.LogInformation("Order item creato con ID: {OrderItemId}", orderItemDto.OrderItemId);
                return CreatedAtAction(nameof(GetById), new { id = orderItemDto.OrderItemId }, orderItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione dell'order item");
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Aggiorna un order item esistente
        /// </summary>
        /// <param name="id">ID dell'order item da aggiornare</param>
        /// <param name="orderItemDto">Dati aggiornati dell'order item</param>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, OrderItemDTO orderItemDto)
        {
            try
            {
                if (id != orderItemDto.OrderItemId)
                {
                    _logger.LogWarning("ID non corrispondente per l'aggiornamento. Richiesto: {Id}, Fornito: {OrderItemId}", id, orderItemDto.OrderItemId);
                    return BadRequest("ID non corrispondente.");
                }

                if (!await _orderItemRepository.ExistsAsync(id))
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato per l'aggiornamento", id);
                    return NotFound($"OrderItem con ID {id} non trovato.");
                }

                _logger.LogInformation("Aggiornamento order item con ID: {OrderItemId}", id);
                await _orderItemRepository.UpdateAsync(orderItemDto);

                _logger.LogInformation("Order item con ID {OrderItemId} aggiornato con successo", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dell'order item con ID: {OrderItemId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Elimina un order item
        /// </summary>
        /// <param name="id">ID dell'order item da eliminare</param>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (!await _orderItemRepository.ExistsAsync(id))
                {
                    _logger.LogWarning("Order item con ID {OrderItemId} non trovato per l'eliminazione", id);
                    return NotFound($"OrderItem con ID {id} non trovato.");
                }

                _logger.LogInformation("Eliminazione order item con ID: {OrderItemId}", id);
                await _orderItemRepository.DeleteAsync(id);

                _logger.LogInformation("Order item con ID {OrderItemId} eliminato con successo", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione dell'order item con ID: {OrderItemId}", id);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottiene tutti gli order items di un ordine specifico
        /// </summary>
        /// <param name="ordineId">ID dell'ordine</param>
        [HttpGet("ordine/{ordineId}")]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByOrderId(int ordineId)
        {
            try
            {
                _logger.LogInformation("Recupero order items per ordine ID: {OrdineId}", ordineId);
                var orderItems = await _orderItemRepository.GetByOrderIdAsync(ordineId);
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per ordine ID: {OrdineId}", ordineId);
                return StatusCode(500, "Errore interno del server");
            }
        }

        /// <summary>
        /// Ottiene tutti gli order items di un articolo specifico
        /// </summary>
        /// <param name="articoloId">ID dell'articolo</param>
        [HttpGet("articolo/{articoloId}")]
        public async Task<ActionResult<IEnumerable<OrderItemDTO>>> GetByArticoloId(int articoloId)
        {
            try
            {
                _logger.LogInformation("Recupero order items per articolo ID: {ArticoloId}", articoloId);
                var orderItems = await _orderItemRepository.GetByArticoloIdAsync(articoloId);
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli order items per articolo ID: {ArticoloId}", articoloId);
                return StatusCode(500, "Errore interno del server");
            }
        }
    }
}