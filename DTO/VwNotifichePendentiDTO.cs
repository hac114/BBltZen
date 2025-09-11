using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class VwNotifichePendentiDTO
    {
        public int NotificaId { get; set; }
        public DateTime DataCreazione { get; set; }
        public string OrdiniCoinvolti { get; set; } = null!;
        public string Messaggio { get; set; } = null!;
        public int? Priorita { get; set; }
        public int? MinutiDaCreazione { get; set; }
    }
}
