using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class NotificheOperativeDTO
    {
        public int NotificaId { get; set; }
        public DateTime DataCreazione { get; set; }
        public string OrdiniCoinvolti { get; set; } = null!;
        public string Messaggio { get; set; } = null!;
        public string Stato { get; set; } = null!;
        public DateTime? DataGestione { get; set; }
        public string? UtenteGestione { get; set; }
        public int? Priorita { get; set; }
    }
}
