using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Service
{
    public class NotificheOperativeRepository : INotificheOperativeRepository
    {
        private readonly BubbleTeaContext _context;
        public NotificheOperativeRepository(BubbleTeaContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<NotificheOperativeDTO>> GetAllAsync()
        {
            return await _context.NotificheOperative
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataGestione = n.DataGestione,
                    Messaggio = n.Messaggio,
                    Priorita = n.Priorita,
                    Stato = n.Stato,
                    DataCreazione = n.DataCreazione,
                    UtenteGestione = n.UtenteGestione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,

                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<NotificheOperativeDTO> GetByIdAsync(int notificaId)
        {
            var notifica = await _context.NotificheOperative.FindAsync(notificaId);
            if (notifica == null) return null;

            return new NotificheOperativeDTO
            {
                NotificaId = notifica.NotificaId,
                DataGestione = notifica.DataGestione,
                Messaggio = notifica.Messaggio,
                Priorita = notifica.Priorita,
                Stato = notifica.Stato,
                DataCreazione = notifica.DataCreazione,
                UtenteGestione = notifica.UtenteGestione,
                OrdiniCoinvolti = notifica.OrdiniCoinvolti,
                // Map other properties as needed
            };
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByStatoAsync(string stato)
        {
            return await _context.NotificheOperative
                .Where(n => n.Stato == stato)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataGestione = n.DataGestione,
                    Messaggio = n.Messaggio,
                    Priorita = n.Priorita,
                    Stato = n.Stato,
                    DataCreazione = n.DataCreazione,
                    UtenteGestione = n.UtenteGestione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,

                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPrioritaAsync(int priorita)
        {
            return await _context.NotificheOperative
                .Where(n => n.Priorita == priorita)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataGestione = n.DataGestione,
                    Messaggio = n.Messaggio,
                    Priorita = n.Priorita,
                    Stato = n.Stato,
                    DataCreazione = n.DataCreazione,
                    UtenteGestione = n.UtenteGestione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,

                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetPendentiAsync()
        {
            return await _context.NotificheOperative
                .Where(n => n.Stato == "Pendente")
                .OrderByDescending(n => n.Priorita)
                .ThenByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataGestione = n.DataGestione,
                    Messaggio = n.Messaggio,
                    Priorita = n.Priorita,
                    Stato = n.Stato,
                    DataCreazione = n.DataCreazione,
                    UtenteGestione = n.UtenteGestione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,


                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.NotificheOperative
                .Where(n => n.DataCreazione >= dataInizio && n.DataCreazione <= dataFine)
                .OrderBy(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataGestione = n.DataGestione,
                    Messaggio = n.Messaggio,
                    Priorita = n.Priorita,
                    Stato = n.Stato,
                    DataCreazione = n.DataCreazione,
                    UtenteGestione = n.UtenteGestione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,


                    // Map other properties as needed
                })
                .ToListAsync();
        }

        public async Task AddAsync(NotificheOperativeDTO notificaDto)
        {
            var notifica = new NotificheOperative
            {
                Messaggio = notificaDto.Messaggio,
                Priorita = notificaDto.Priorita,
                Stato = notificaDto.Stato ?? "Pendente", // Default value
                DataCreazione = DateTime.Now,
                DataGestione = notificaDto.DataGestione,
                UtenteGestione = notificaDto.UtenteGestione,
                NotificaId = notificaDto.NotificaId,
                OrdiniCoinvolti = notificaDto.OrdiniCoinvolti,

                // Map other properties as needed
            };

            await _context.NotificheOperative.AddAsync(notifica);
            await _context.SaveChangesAsync();

            // Return the generated ID to DTO
            notificaDto.NotificaId = notifica.NotificaId;
            notificaDto.DataCreazione = notifica.DataCreazione;
        }

        public async Task UpdateAsync(NotificheOperativeDTO notificaDto)
        {
            var notifica = await _context.NotificheOperative.FindAsync(notificaDto.NotificaId);
            if (notifica == null)
                throw new ArgumentException("Notifica not found");

            // notifica.DataCreazione = notificaDto.DataCreazione; // Usually not updated
            notifica.Messaggio = notificaDto.Messaggio;
            notifica.Priorita = notificaDto.Priorita;
            notifica.Stato = notificaDto.Stato;
            notifica.DataGestione = DateTime.Now; // Always update timestamp
            // Update other properties as needed

            _context.NotificheOperative.Update(notifica);
            await _context.SaveChangesAsync();

            // Update DTO with latest timestamp
            notificaDto.DataGestione = notifica.DataGestione;
        }

        public async Task DeleteAsync(int notificaId)
        {
            var notifica = await _context.NotificheOperative.FindAsync(notificaId);
            if (notifica != null)
            {
                _context.NotificheOperative.Remove(notifica);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int notificaId)
        {
            return await _context.NotificheOperative.AnyAsync(n => n.NotificaId == notificaId);
        }
    }
}
