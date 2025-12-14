using Database;
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
    public class OrdineRepository : IOrdineRepository
    {
        private readonly BubbleTeaContext _context;

        public OrdineRepository(BubbleTeaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrdineDTO>> GetAllAsync()
        {
            return await _context.Ordine
                .AsNoTracking()
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId // ✅ AGGIUNTO
                })
                .ToListAsync();
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
                Priorita = ordine.Priorita,
                SessioneId = ordine.SessioneId // ✅ AGGIUNTO
            };
        }

        public async Task<OrdineDTO> AddAsync(OrdineDTO entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            var ordineEntity = new Ordine
            {
                ClienteId = entity.ClienteId,
                DataCreazione = DateTime.Now,
                DataAggiornamento = DateTime.Now,
                StatoOrdineId = entity.StatoOrdineId,
                StatoPagamentoId = entity.StatoPagamentoId,
                Totale = entity.Totale,
                Priorita = entity.Priorita,
                SessioneId = entity.SessioneId // ✅ AGGIUNTO
            };

            await _context.Ordine.AddAsync(ordineEntity);
            await _context.SaveChangesAsync();

            // Aggiorna il DTO con i valori del database
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

            if (existingOrdine == null)
                throw new InvalidOperationException($"Ordine con ID {entity.OrdineId} non trovato.");

            existingOrdine.ClienteId = entity.ClienteId;
            existingOrdine.StatoOrdineId = entity.StatoOrdineId;
            existingOrdine.StatoPagamentoId = entity.StatoPagamentoId;
            existingOrdine.Totale = entity.Totale;
            existingOrdine.Priorita = entity.Priorita;
            existingOrdine.SessioneId = entity.SessioneId; // ✅ AGGIUNTO
            existingOrdine.DataAggiornamento = DateTime.Now;

            await _context.SaveChangesAsync();

            // Aggiorna il DTO con la data di aggiornamento
            entity.DataAggiornamento = existingOrdine.DataAggiornamento;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Ordine
                .FirstOrDefaultAsync(o => o.OrdineId == id);

            if (entity != null)
            {
                _context.Ordine.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Ordine
                .AnyAsync(o => o.OrdineId == id);
        }

        public async Task<IEnumerable<OrdineDTO>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.ClienteId == clienteId)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId // ✅ AGGIUNTO
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdineDTO>> GetByStatoOrdineIdAsync(int statoOrdineId)
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.StatoOrdineId == statoOrdineId)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId // ✅ AGGIUNTO
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OrdineDTO>> GetByStatoPagamentoIdAsync(int statoPagamentoId)
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.StatoPagamentoId == statoPagamentoId)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId // ✅ AGGIUNTO
                })
                .ToListAsync();
        }

        // ✅ NUOVO METODO: GetBySessioneIdAsync
        public async Task<IEnumerable<OrdineDTO>> GetBySessioneIdAsync(Guid sessioneId)
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.SessioneId == sessioneId)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId
                })
                .ToListAsync();
        }

        // ✅ NUOVO METODO: GetOrdiniConSessioneAsync
        public async Task<IEnumerable<OrdineDTO>> GetOrdiniConSessioneAsync()
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.SessioneId != null)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId
                })
                .ToListAsync();
        }

        // ✅ NUOVO METODO: GetOrdiniSenzaSessioneAsync
        public async Task<IEnumerable<OrdineDTO>> GetOrdiniSenzaSessioneAsync()
        {
            return await _context.Ordine
                .AsNoTracking()
                .Where(o => o.SessioneId == null)
                .Select(o => new OrdineDTO
                {
                    OrdineId = o.OrdineId,
                    ClienteId = o.ClienteId,
                    DataCreazione = o.DataCreazione,
                    DataAggiornamento = o.DataAggiornamento,
                    StatoOrdineId = o.StatoOrdineId,
                    StatoPagamentoId = o.StatoPagamentoId,
                    Totale = o.Totale,
                    Priorita = o.Priorita,
                    SessioneId = o.SessioneId
                })
                .ToListAsync();
        }
    }
}