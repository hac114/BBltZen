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
    public class OrdineRepository : IOrdineRepository
    {
        private readonly BubbleTeaContext _context;

        public OrdineRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdineDTO>> GetAllAsync()
        {
            return (IEnumerable<OrdineDTO>)await _context.Ordine.ToListAsync();
        }

        public async Task<OrdineDTO?> GetByIdAsync(int id)
        {
            var ordine = await _context.Ordine
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrdineId == id);

            if (ordine == null) return null;
            return new OrdineDTO
            {
                OrdineId = ordine.OrdineId,
                ClienteId = ordine.ClienteId,
                DataCreazione = ordine.DataCreazione,
                DataAggiornamento = ordine.DataAggiornamento,
                StatoOrdineId = ordine.StatoOrdineId,
                StatoPagamentoId = ordine.StatoPagamentoId,
                Totale = ordine.Totale,
                Priorita = ordine.Priorita
            };

        }

        public async Task<OrdineDTO> AddAsync(OrdineDTO entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            var ordineEntity = new Ordine
            {
                ClienteId = entity.ClienteId,
                DataCreazione = entity.DataCreazione,
                DataAggiornamento = entity.DataAggiornamento,
                StatoOrdineId = entity.StatoOrdineId,
                StatoPagamentoId = entity.StatoPagamentoId,
                Totale = entity.Totale,
                Priorita = entity.Priorita
            };
            await _context.Ordine.AddAsync(ordineEntity);
            await _context.SaveChangesAsync();
            entity.OrdineId = ordineEntity.OrdineId;
            entity.DataCreazione = ordineEntity.DataCreazione;
            entity.DataAggiornamento = ordineEntity.DataAggiornamento;
            return entity;
        }

        public async Task UpdateAsync(OrdineDTO entity)
        {
            if (entity == null || entity.OrdineId == 0)
                throw new ArgumentException("Invalid entity or entity ID.");

            var existingOrdine = await _context.Ordine
                .FirstOrDefaultAsync(o => o.OrdineId == entity.OrdineId);
            //.FindAsync(entity.OrdineId);
            if (existingOrdine == null)
                throw new InvalidOperationException("Entity not found in the database.");

            existingOrdine.ClienteId = entity.ClienteId;
            existingOrdine.StatoOrdineId = entity.StatoOrdineId;
            existingOrdine.StatoPagamentoId = entity.StatoPagamentoId;
            existingOrdine.Totale = entity.Totale;
            existingOrdine.Priorita = entity.Priorita;
            existingOrdine.DataAggiornamento = DateTime.Now;
            _context.Ordine.Update(existingOrdine);
            await _context.SaveChangesAsync();

        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Ordine.FindAsync(id);
            if (entity != null)
            {
                _context.Ordine.Remove(entity);
                await _context.SaveChangesAsync();

            }
        }
    }

}
