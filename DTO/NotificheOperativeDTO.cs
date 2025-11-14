using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class NotificheOperativeDTO
    {
        public int NotificaId { get; set; }

        public DateTime DataCreazione { get; set; }

        [Required(ErrorMessage = "Gli ordini coinvolti sono obbligatori")]
        [StringLength(1000, ErrorMessage = "Gli ordini coinvolti non possono superare 1000 caratteri")]
        public string OrdiniCoinvolti { get; set; } = null!;

        [Required(ErrorMessage = "Il messaggio è obbligatorio")]
        [StringLength(500, ErrorMessage = "Il messaggio non può superare 500 caratteri")]
        public string Messaggio { get; set; } = null!;

        [Required(ErrorMessage = "Lo stato è obbligatorio")]
        [StringLength(20, ErrorMessage = "Lo stato non può superare 20 caratteri")]
        [RegularExpression("^(Pendente|In Lavorazione|Gestita|Annullata)$",
            ErrorMessage = "Stato non valido. Valori consentiti: Pendente, In Lavorazione, Gestita, Annullata")]
        public string Stato { get; set; } = "Pendente";

        public DateTime? DataGestione { get; set; }

        [StringLength(100, ErrorMessage = "L'utente gestione non può superare 100 caratteri")]
        public string? UtenteGestione { get; set; }

        [Range(1, 10, ErrorMessage = "La priorità deve essere tra 1 e 10")]
        public int Priorita { get; set; } = 1;
        public string? TipoNotifica { get; set; } = "sistema";
    }
}