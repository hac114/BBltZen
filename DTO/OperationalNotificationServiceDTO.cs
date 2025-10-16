using System;
using System.Collections.Generic;

namespace DTO
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string TipoNotifica { get; set; } = null!;
        public string Titolo { get; set; } = null!;
        public string Messaggio { get; set; } = null!;
        public string Priorita { get; set; } = null!;
        public bool Letta { get; set; }
        public DateTime DataCreazione { get; set; }
        public Dictionary<string, object> DatiAggiuntivi { get; set; } = new Dictionary<string, object>();
    }

    public class LowStockNotificationDTO
    {
        public int IngredienteId { get; set; }
        public string NomeIngrediente { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public int BevandeAffette { get; set; }
        public DateTime DataRilevamento { get; set; }
    }

    public class OrderStatusNotificationDTO
    {
        public int OrderId { get; set; }
        public string VecchioStato { get; set; } = null!;
        public string NuovoStato { get; set; } = null!;
        public int ClienteId { get; set; }
        public DateTime DataCambiamento { get; set; }
    }

    public class NotificationSummaryDTO
    {
        public int TotalNotifiche { get; set; }
        public int NotificheNonLette { get; set; }
        public int NotificheAltaPriorita { get; set; }
        public List<NotificationDTO> UltimeNotifiche { get; set; } = new List<NotificationDTO>();
        public DateTime DataAggiornamento { get; set; }
    }
}