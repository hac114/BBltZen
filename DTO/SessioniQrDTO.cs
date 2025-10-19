using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SessioniQrDTO
    {
        public Guid SessioneId { get; set; }
        public int TavoloId { get; set; }
        public int? ClienteId { get; set; }

        [StringLength(100, ErrorMessage = "Il codice sessione non può superare 100 caratteri")]
        public string CodiceSessione { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Lo stato non può superare 20 caratteri")]
        public string Stato { get; set; } = "Attiva";

        [StringLength(500, ErrorMessage = "Il QR code non può superare 500 caratteri")]
        public string QrCode { get; set; } = null!;

        public DateTime? DataCreazione { get; set; }
        public DateTime DataScadenza { get; set; }
        public bool? Utilizzato { get; set; }
        public DateTime? DataUtilizzo { get; set; }
    }
}