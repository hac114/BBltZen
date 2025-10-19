using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }

        [StringLength(50)]
        public string TipoNotifica { get; set; } = null!;

        [StringLength(200)]
        public string Titolo { get; set; } = null!;

        [StringLength(1000)]
        public string Messaggio { get; set; } = null!;

        [StringLength(10)]
        public string Priorita { get; set; } = null!;

        public bool Letta { get; set; }
        public DateTime DataCreazione { get; set; }
        public Dictionary<string, object> DatiAggiuntivi { get; set; } = new Dictionary<string, object>();
    }

    public class LowStockNotificationDTO
    {
        public int IngredienteId { get; set; }

        [StringLength(100)]
        public string NomeIngrediente { get; set; } = null!;

        [StringLength(50)]
        public string Categoria { get; set; } = null!;

        [Range(0, 1000)]
        public int BevandeAffette { get; set; }

        public DateTime DataRilevamento { get; set; }
    }

    public class OrderStatusNotificationDTO
    {
        public int OrderId { get; set; }

        [StringLength(50)]
        public string VecchioStato { get; set; } = null!;

        [StringLength(50)]
        public string NuovoStato { get; set; } = null!;

        public int ClienteId { get; set; }

        public DateTime DataCambiamento { get; set; }
    }

    public class NotificationSummaryDTO
    {
        [Range(0, int.MaxValue)]
        public int TotalNotifiche { get; set; }

        [Range(0, int.MaxValue)]
        public int NotificheNonLette { get; set; }

        [Range(0, int.MaxValue)]
        public int NotificheAltaPriorita { get; set; }

        public List<NotificationDTO> UltimeNotifiche { get; set; } = new List<NotificationDTO>();

        public DateTime DataAggiornamento { get; set; }
    }
}