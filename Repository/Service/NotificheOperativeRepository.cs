using Database.Models;
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

        private NotificheOperativeDTO MapToDTO(NotificheOperative notifica)
        {
            return new NotificheOperativeDTO
            {
                NotificaId = notifica.NotificaId,
                DataCreazione = notifica.DataCreazione,
                OrdiniCoinvolti = notifica.OrdiniCoinvolti,
                Messaggio = notifica.Messaggio,
                Stato = notifica.Stato,
                DataGestione = notifica.DataGestione,
                UtenteGestione = notifica.UtenteGestione,
                Priorita = notifica.Priorita,
                TipoNotifica = notifica.TipoNotifica
            };
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetAllAsync()
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<NotificheOperativeDTO?> GetByIdAsync(int notificaId)
        {
            var notifica = await _context.NotificheOperative
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NotificaId == notificaId);

            return notifica == null ? null : MapToDTO(notifica);
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByStatoAsync(string stato)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Stato == stato)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPrioritaAsync(int priorita)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Priorita == priorita)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetPendentiAsync()
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.Stato == "Pendente")
                .OrderBy(n => n.Priorita)
                .ThenByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByPeriodoAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.DataCreazione >= dataInizio && n.DataCreazione <= dataFine)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<NotificheOperativeDTO> AddAsync(NotificheOperativeDTO notificaDto)
        {
            if (notificaDto == null)
                throw new ArgumentNullException(nameof(notificaDto));

            var notifica = new NotificheOperative
            {
                DataCreazione = DateTime.Now, // ✅ SEMPRE DateTime.Now
                OrdiniCoinvolti = notificaDto.OrdiniCoinvolti,
                Messaggio = notificaDto.Messaggio,
                Stato = notificaDto.Stato ?? "Pendente",
                DataGestione = notificaDto.DataGestione,
                UtenteGestione = notificaDto.UtenteGestione,
                Priorita = notificaDto.Priorita,
                TipoNotifica = notificaDto.TipoNotifica ?? "sistema"
            };

            _context.NotificheOperative.Add(notifica);
            await _context.SaveChangesAsync();

            // ✅ AGGIORNA DTO CON ID GENERATO
            notificaDto.NotificaId = notifica.NotificaId;
            notificaDto.DataCreazione = notifica.DataCreazione;

            return notificaDto; // ✅ IMPORTANTE: ritorna DTO
        }

        public async Task UpdateAsync(NotificheOperativeDTO notificaDto)
        {
            var notifica = await _context.NotificheOperative
                .FirstOrDefaultAsync(n => n.NotificaId == notificaDto.NotificaId);

            if (notifica == null)
                return; // ✅ SILENT FAIL - Non lanciare eccezione

            // ✅ AGGIORNA SOLO SE ESISTE
            notifica.OrdiniCoinvolti = notificaDto.OrdiniCoinvolti;
            notifica.Messaggio = notificaDto.Messaggio;
            notifica.Stato = notificaDto.Stato;
            notifica.Priorita = notificaDto.Priorita;
            notifica.UtenteGestione = notificaDto.UtenteGestione;
            notifica.TipoNotifica = notificaDto.TipoNotifica;

            // ✅ Aggiorna DataGestione solo se lo stato cambia in "Gestita"
            if (notificaDto.Stato == "Gestita" && notifica.DataGestione == null)
            {
                notifica.DataGestione = DateTime.Now;
                notificaDto.DataGestione = notifica.DataGestione;
            }

            await _context.SaveChangesAsync();
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

        public async Task<IEnumerable<NotificheOperativeDTO>> GetByTipoNotificaAsync(string tipoNotifica)
        {
            return await _context.NotificheOperative
                .AsNoTracking()
                .Where(n => n.TipoNotifica == tipoNotifica)
                .OrderByDescending(n => n.DataCreazione)
                .Select(n => MapToDTO(n))
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetStatisticheNotificheAsync()
        {
            var statistiche = await _context.NotificheOperative
                .GroupBy(n => n.Stato)
                .Select(g => new { Stato = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Stato, x => x.Count);

            return statistiche;
        }

        public async Task<int> GetNumeroNotificheByStatoAsync(string stato)
        {
            return await _context.NotificheOperative
                .CountAsync(n => n.Stato == stato);
        }
    }
}