using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [StringLength(20, ErrorMessage = "Il tipo accesso non può superare 20 caratteri")]
        public string TipoAccesso { get; set; } = null!;

        [StringLength(20, ErrorMessage = "L'esito non può superare 20 caratteri")]
        public string Esito { get; set; } = null!;

        [StringLength(45, ErrorMessage = "L'IP address non può superare 45 caratteri")]
        public string? IpAddress { get; set; }

        [StringLength(500, ErrorMessage = "Lo user agent non può superare 500 caratteri")]
        public string? UserAgent { get; set; }

        public DateTime? DataCreazione { get; set; }

        [StringLength(1000, ErrorMessage = "I dettagli non possono superare 1000 caratteri")]
        public string? Dettagli { get; set; }
    }
}