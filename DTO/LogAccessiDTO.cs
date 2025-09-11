using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class LogAccessiDTO
    {
        public int LogId { get; set; }
        public int? UtenteId { get; set; }
        public int? ClienteId { get; set; }
        public string TipoAccesso { get; set; } = null!;
        public string Esito { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime? DataCreazione { get; set; }
        public string? Dettagli { get; set; }
    }
}
