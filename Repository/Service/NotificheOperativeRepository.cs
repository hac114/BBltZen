using Database;
using DTO;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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
                .AsNoTracking()
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataCreazione = n.DataCreazione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,
                    Messaggio = n.Messaggio,
                    Stato = n.Stato,
                    DataGestione = n.DataGestione,
                    UtenteGestione = n.UtenteGestione,
                    Priorita = n.Priorita
                })
                .ToListAsync();
        }

        public async Task<NotificheOperativeDTO?> GetByIdAsync(int notificaId)
        {
            var notifica = await _context.NotificheOperative
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NotificaId == notificaId);

            if (notifica == null) return null;

            return new NotificheOperativeDTO
            {
                NotificaId = notifica.NotificaId,
                DataCreazione = notifica.DataCreazione,
                OrdiniCoinvolti = notifica.OrdiniCoinvolti,
                Messaggio = notifica.Messaggio,
                Stato = notifica.Stato,
                DataGestione = notifica.DataGestione,
                UtenteGestione = notifica.UtenteGestione,
                Priorita = notifica.Priorita
            };
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByStatoAsync(string stato)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Stato == stato)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataCreazione = n.DataCreazione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,
                    Messaggio = n.Messaggio,
                    Stato = n.Stato,
                    DataGestione = n.DataGestione,
                    UtenteGestione = n.UtenteGestione,
                    Priorita = n.Priorita
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPrioritaAsync(int priorita)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Priorita == priorita)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataCreazione = n.DataCreazione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,
                    Messaggio = n.Messaggio,
                    Stato = n.Stato,
                    DataGestione = n.DataGestione,
                    UtenteGestione = n.UtenteGestione,
                    Priorita = n.Priorita
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetPendentiAsync()
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Stato == "Pendente")
                .OrderBy(n => n.Priorita) // CAMBIATO: OrderBy invece di OrderByDescending
                .ThenByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataCreazione = n.DataCreazione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,
                    Messaggio = n.Messaggio,
                    Stato = n.Stato,
                    DataGestione = n.DataGestione,
                    UtenteGestione = n.UtenteGestione,
                    Priorita = n.Priorita
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.DataCreazione >= dataInizio && n.DataCreazione <= dataFine)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => new NotificheOperativeDTO
                {
                    NotificaId = n.NotificaId,
                    DataCreazione = n.DataCreazione,
                    OrdiniCoinvolti = n.OrdiniCoinvolti,
                    Messaggio = n.Messaggio,
                    Stato = n.Stato,
                    DataGestione = n.DataGestione,
                    UtenteGestione = n.UtenteGestione,
                    Priorita = n.Priorita
                })
                .ToListAsync();
        }

        public async Task AddAsync(NotificheOperativeDTO notificaDto)
        {
            var notifica = new NotificheOperative
            {
                DataCreazione = DateTime.Now,
                OrdiniCoinvolti = notificaDto.OrdiniCoinvolti,
                Messaggio = notificaDto.Messaggio,
                Stato = notificaDto.Stato ?? "Pendente",
                DataGestione = notificaDto.DataGestione,
                UtenteGestione = notificaDto.UtenteGestione,
                Priorita = notificaDto.Priorita
            };

            _context.NotificheOperative.Add(notifica);
            await _context.SaveChangesAsync();

            notificaDto.NotificaId = notifica.NotificaId;
            notificaDto.DataCreazione = notifica.DataCreazione;
        }

        public async Task UpdateAsync(NotificheOperativeDTO notificaDto)
        {
            var notifica = await _context.NotificheOperative
                .FirstOrDefaultAsync(n => n.NotificaId == notificaDto.NotificaId);

            if (notifica == null)
                throw new ArgumentException($"Notifica con ID {notificaDto.NotificaId} non trovata");

            notifica.OrdiniCoinvolti = notificaDto.OrdiniCoinvolti;
            notifica.Messaggio = notificaDto.Messaggio;
            notifica.Stato = notificaDto.Stato;
            notifica.Priorita = notificaDto.Priorita;
            notifica.UtenteGestione = notificaDto.UtenteGestione;

            // Aggiorna DataGestione solo se lo stato cambia in "Gestita"
            if (notificaDto.Stato == "Gestita" && notifica.DataGestione == null)
            {
                notifica.DataGestione = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            notificaDto.DataGestione = notifica.DataGestione;
        }

        public async Task DeleteAsync(int notificaId)
        {
            var notifica = await _context.NotificheOperative
                .FirstOrDefaultAsync(n => n.NotificaId == notificaId);

            if (notifica != null)
            {
                _context.NotificheOperative.Remove(notifica);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int notificaId)
        {
            return await _context.NotificheOperative
                .AnyAsync(n => n.NotificaId == notificaId);
        }

        public async Task<int> GetNumeroNotifichePendentiAsync()
        {
            return await _context.NotificheOperative
                .CountAsync(n => n.Stato == "Pendente");
        }
    }
}