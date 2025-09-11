using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IEnumerable<OrderItemDTO>> GetAllAsync()
        {
            return await _context.OrderItem
                .Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    OrdineId = oi.OrdineId,
                    ArticoloId = oi.ArticoloId,
                    Quantita = oi.Quantita,
                    PrezzoUnitario = oi.PrezzoUnitario,
                    ScontoApplicato = oi.ScontoApplicato,
                    Imponibile = oi.Imponibile,
                    DataCreazione = oi.DataCreazione,
                    DataAggiornamento = oi.DataAggiornamento,
                    TipoArticolo = oi.TipoArticolo,
                    TotaleIvato = oi.TotaleIvato,
                    TaxRateId = oi.TaxRateId
                })
                .ToListAsync();
        }

        public async Task<OrderItemDTO> GetByIdAsync(int id)
        {
            var orderItem = await _context.OrderItem
                .FindAsync(id);
            if (orderItem == null) return null;
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

        public async Task AddAsync(OrderItemDTO orderItem)
        {
            var orderIteme = new OrderItem
            {
                OrdineId = orderItem.OrdineId,
                ArticoloId = orderItem.ArticoloId,
                Quantita = orderItem.Quantita,
                PrezzoUnitario = orderItem.PrezzoUnitario,
                ScontoApplicato = orderItem.ScontoApplicato,
                Imponibile = orderItem.Imponibile,
            };
            await _context.OrderItem.AddAsync(orderIteme);
            await _context.SaveChangesAsync();
            orderItem.OrderItemId = orderIteme.OrderItemId;
        }

        public async Task UpdateAsync(OrderItemDTO orderItems)
        {
            var orderItem = await _context.OrderItem.FindAsync(orderItems.OrderItemId);
            if (orderItem == null)
                throw new ArgumentException("Invalid entity or entity ID.");
            orderItem.OrdineId = orderItems.OrdineId;
            orderItem.ArticoloId = orderItems.ArticoloId;
            orderItem.Quantita = orderItems.Quantita;
            orderItem.PrezzoUnitario = orderItems.PrezzoUnitario;
            orderItem.ScontoApplicato = orderItems.ScontoApplicato;
            orderItem.Imponibile = orderItems.Imponibile;
            orderItem.DataAggiornamento = DateTime.UtcNow;

            _context.OrderItem.Update(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var orderItems = await _context.OrderItem.FindAsync(id);
            if (orderItems != null)
            {
                _context.OrderItem.Remove(orderItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderItem.AnyAsync(oi => oi.OrderItemId == id);
        }

        public async Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int ordineId)
        {
            return await _context.OrderItem
                .Where(oi => oi.OrdineId == ordineId)
                .Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    OrdineId = oi.OrdineId,
                    ArticoloId = oi.ArticoloId,
                    Quantita = oi.Quantita,
                    PrezzoUnitario = oi.PrezzoUnitario,
                    ScontoApplicato = oi.ScontoApplicato,
                    Imponibile = oi.Imponibile,
                    DataCreazione = oi.DataCreazione,
                    DataAggiornamento = oi.DataAggiornamento,
                    TipoArticolo = oi.TipoArticolo,
                    TotaleIvato = oi.TotaleIvato,
                    TaxRateId = oi.TaxRateId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItemDTO>> GetByArticoloIdAsync(int articoloId)
        {
            return await _context.OrderItem
                .Where(oi => oi.ArticoloId == articoloId)
                .Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    OrdineId = oi.OrdineId,
                    ArticoloId = oi.ArticoloId,
                    Quantita = oi.Quantita,
                    PrezzoUnitario = oi.PrezzoUnitario,
                    ScontoApplicato = oi.ScontoApplicato,
                    Imponibile = oi.Imponibile,
                    DataCreazione = oi.DataCreazione,
                    DataAggiornamento = oi.DataAggiornamento,
                    TipoArticolo = oi.TipoArticolo,
                    TotaleIvato = oi.TotaleIvato,
                    TaxRateId = oi.TaxRateId
                })
                .ToListAsync();
        }
    }
}
