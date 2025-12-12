using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class LogAttivitaDTO
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

        // ✅ CAMPI ARRICCHITI DALL'UTENTE
        public string? TipoUtente { get; set; }

        [StringLength(100, ErrorMessage = "Il nome utente non può superare 100 caratteri")]
        public string? UtenteNome { get; set; }

        [EmailAddress(ErrorMessage = "Formato email non valido")]
        [StringLength(100, ErrorMessage = "L'email non può superare 100 caratteri")]
        public string? UtenteEmail { get; set; }

        public int? ClienteId { get; set; }

        // ✅ CAMPI CALCOLATI AGGIORNATI
        public string DataFormattata => DataEsecuzione.ToString("dd/MM/yyyy HH:mm");

        public string UtenteDisplay => !string.IsNullOrEmpty(UtenteNome)
            ? $"{UtenteNome} ({TipoUtente ?? "Utente"})"
            : "Sistema";

        public string TipoAttivitaFormattato => !string.IsNullOrEmpty(TipoAttivita)
            ? char.ToUpper(TipoAttivita[0]) + TipoAttivita[1..].ToLower()
            : string.Empty;

        // ✅ NUOVI CAMPI CALCOLATI UTILI
        public bool IsSistema => UtenteId == null || UtenteId == 0;
        public bool HasDettagli => !string.IsNullOrEmpty(Dettagli);

        // ✅ PER FACILITARE LA RICERCA (solo get, non persistito)
        public string RicercaTestuale =>
            $"{TipoAttivita} {Descrizione} {Dettagli} {UtenteNome} {TipoUtente}".ToLower();
    }
}