using BBltZen;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly BubbleTeaContext _context;

        public OrderItemRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        // ✅ METODO HELPER PER MAPPING - DA AGGIUNGERE
        private static OrderItemDTO MapToDTO(OrderItem orderItem)
        {
            return new OrderItemDTO
            {
                OrderItemId = orderItem.OrderItemId,
                OrdineId = orderItem.OrdineId,
                ArticoloId = orderItem.ArticoloId,
                Quantita = orderItem.Quantita,
                PrezzoUnitario = orderItem.PrezzoUnitario,
                ScontoApplicato = orderItem.ScontoApplicato,
                Imponibile = orderItem.Imponibile,
                DataCreazione = orderItem.DataCreazione,
                DataAggiornamento = orderItem.DataAggiornamento,
                TipoArticolo = orderItem.TipoArticolo,
                TotaleIvato = orderItem.TotaleIvato,
                TaxRateId = orderItem.TaxRateId
            };
        }

        public async Task<IEnumerable<OrderItemDTO>> GetAllAsync()
        {
            return await _context.OrderItem
                .AsNoTracking()
                .Select(oi => MapToDTO(oi))
                .ToListAsync();
        }

        // ✅ METODO GetByIdAsync CORRETTO CON MapToDTO
        public async Task<OrderItemDTO?> GetByIdAsync(int id)
        {
            var orderItem = await _context.OrderItem
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.OrderItemId == id);

            return orderItem == null ? null : MapToDTO(orderItem);
        }

        public async Task<OrderItemDTO> AddAsync(OrderItemDTO orderItemDto) // ✅ CAMBIATO: ritorna DTO
        {
            if (orderItemDto == null)
                throw new ArgumentNullException(nameof(orderItemDto));

            var orderItem = new OrderItem
            {
                OrdineId = orderItemDto.OrdineId,
                ArticoloId = orderItemDto.ArticoloId,
                Quantita = orderItemDto.Quantita,
                PrezzoUnitario = orderItemDto.PrezzoUnitario,
                ScontoApplicato = orderItemDto.ScontoApplicato,
                Imponibile = orderItemDto.Imponibile,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                TipoArticolo = orderItemDto.TipoArticolo ?? "BS", // ✅ DEFAULT per Bevanda Standard
                TotaleIvato = orderItemDto.TotaleIvato,
                TaxRateId = orderItemDto.TaxRateId
            };

            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();

            // ✅ Aggiorna DTO con valori dal database
            orderItemDto.OrderItemId = orderItem.OrderItemId;
            orderItemDto.DataCreazione = orderItem.DataCreazione;
            orderItemDto.DataAggiornamento = orderItem.DataAggiornamento;
            orderItemDto.TipoArticolo = orderItem.TipoArticolo;

            return orderItemDto; // ✅ IMPORTANTE: ritorna il DTO
        }

        public async Task UpdateAsync(OrderItemDTO orderItemDto)
        {
            var orderItem = await _context.OrderItem
                .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemDto.OrderItemId);

            if (orderItem == null)
                throw new ArgumentException($"OrderItem con OrderItemId {orderItemDto.OrderItemId} non trovato");

            orderItem.OrdineId = orderItemDto.OrdineId;
            orderItem.ArticoloId = orderItemDto.ArticoloId;
            orderItem.Quantita = orderItemDto.Quantita;
            orderItem.PrezzoUnitario = orderItemDto.PrezzoUnitario;
            orderItem.ScontoApplicato = orderItemDto.ScontoApplicato;
            orderItem.Imponibile = orderItemDto.Imponibile;
            orderItem.DataAggiornamento = DateTime.Now;
            orderItem.TipoArticolo = orderItemDto.TipoArticolo;
            orderItem.TotaleIvato = orderItemDto.TotaleIvato;
            orderItem.TaxRateId = orderItemDto.TaxRateId;

            await _context.SaveChangesAsync();

            orderItemDto.DataAggiornamento = orderItem.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var orderItem = await _context.OrderItem
                .FirstOrDefaultAsync(oi => oi.OrderItemId == id);

            if (orderItem != null)
            {
                _context.OrderItem.Remove(orderItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderItem
                .AnyAsync(oi => oi.OrderItemId == id);
        }

        public async Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int ordineId)
        {
            return await _context.OrderItem
                .AsNoTracking()
                .Where(oi => oi.OrdineId == ordineId)
                .Select(oi => MapToDTO(oi))
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItemDTO>> GetByArticoloIdAsync(int articoloId)
        {
            return await _context.OrderItem
                .AsNoTracking()
                .Where(oi => oi.ArticoloId == articoloId)
                .Select(oi => MapToDTO(oi))
                .ToListAsync();
        }
    }
}