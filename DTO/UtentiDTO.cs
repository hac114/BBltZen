using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class UtentiDTO
    {
        public int UtenteId { get; set; }
        public int? ClienteId { get; set; }

        [EmailAddress(ErrorMessage = "Formato email non valido")]
        [StringLength(255, ErrorMessage = "L'email non può superare 255 caratteri")]
        public string Email { get; set; } = null!;

        [StringLength(512, ErrorMessage = "Il password hash non può superare 512 caratteri")]
        public string PasswordHash { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Il tipo utente non può superare 50 caratteri")]
        public string TipoUtente { get; set; } = null!;

        public DateTime? DataCreazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public DateTime? UltimoAccesso { get; set; }
        public bool? Attivo { get; set; }
    }
}