using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class UtentiDTO
    {
        public int UtenteId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string TipoUtente { get; set; } = null!;
        public DateTime? DataCreazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public DateTime? UltimoAccesso { get; set; }
        public bool? Attivo { get; set; }
    }
}
