using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SessioniQrDTO
    {
        public Guid SessioneId { get; set; }
        public int TavoloId { get; set; }           // ✅ CAMBIATO: ClienteId → TavoloId
        public int? ClienteId { get; set; }         // ✅ MODIFICATO: ora nullable (opzionale)
        public string CodiceSessione { get; set; } = null!;  // ✅ NUOVO: per URL sessione
        public string Stato { get; set; } = "Attiva";        // ✅ NUOVO: "Attiva", "Scaduta", "Utilizzata"
        public string QrCode { get; set; } = null!;
        public DateTime? DataCreazione { get; set; }
        public DateTime DataScadenza { get; set; }
        public bool? Utilizzato { get; set; }
        public DateTime? DataUtilizzo { get; set; }
    }
}