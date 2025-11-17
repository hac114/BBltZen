using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class UtentiDTO
    {
        public int UtenteId { get; set; }

        public int? ClienteId { get; set; }

        [EmailAddress(ErrorMessage = "Formato email non valido")]
        [StringLength(255, ErrorMessage = "L'email non può superare 255 caratteri")]
        public string? Email { get; set; }

        [StringLength(512, ErrorMessage = "Il password hash non può superare 512 caratteri")]
        public string? PasswordHash { get; set; }

        [Required(ErrorMessage = "Il tipo utente è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il tipo utente non può superare 50 caratteri")]
        public string TipoUtente { get; set; } = "cliente"; // ✅ DEFAULT

        public DateTime? DataCreazione { get; set; }
        public DateTime? DataAggiornamento { get; set; }
        public DateTime? UltimoAccesso { get; set; }

        public bool? Attivo { get; set; } = true; // ✅ DEFAULT

        [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri")]
        public string? Nome { get; set; }

        [StringLength(100, ErrorMessage = "Il cognome non può superare 100 caratteri")]
        public string? Cognome { get; set; }

        [StringLength(20, ErrorMessage = "Il telefono non può superare 20 caratteri")]
        [Phone(ErrorMessage = "Formato telefono non valido")]
        public string? Telefono { get; set; }

        public Guid? SessioneGuest { get; set; }
    }
}