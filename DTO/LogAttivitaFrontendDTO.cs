using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class LogAttivitaFrontendDTO
    {
        public int LogId { get; set; }

        [Required(ErrorMessage = "Il tipo attività è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il tipo attività non può superare 50 caratteri")]
        public string TipoAttivita { get; set; } = null!;

        [Required(ErrorMessage = "La descrizione è obbligatoria")]
        [StringLength(500, ErrorMessage = "La descrizione non può superare 500 caratteri")]
        public string Descrizione { get; set; } = null!;

        public DateTime DataEsecuzione { get; set; }

        [StringLength(2000, ErrorMessage = "I dettagli non possono superare 2000 caratteri")]
        public string? Dettagli { get; set; }

        public int? UtenteId { get; set; }

        // ✅ NUOVO CAMPO: TipoUtente dall'entità Utenti
        public string? TipoUtente { get; set; }

        // ✅ CAMPI CALCOLATI AGGIORNATI
        public string DataFormattata => DataEsecuzione.ToString("dd/MM/yyyy HH:mm");
        public string UtenteDisplay => !string.IsNullOrEmpty(TipoUtente) ? TipoUtente : "Sistema";
        public string TipoAttivitaFormattato => char.ToUpper(TipoAttivita[0]) + TipoAttivita[1..].ToLower();
    }
}