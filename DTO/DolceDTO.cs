using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class DolceDTO
    {
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri")]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, 99.99, ErrorMessage = "Il prezzo deve essere tra 0.01 e 99.99")]
        public decimal Prezzo { get; set; }

        [StringLength(255, ErrorMessage = "La descrizione non può superare 255 caratteri")]
        public string? Descrizione { get; set; }

        [StringLength(500, ErrorMessage = "L'URL immagine non può superare 500 caratteri")]
        [Url(ErrorMessage = "L'URL immagine non è valido")]
        public string? ImmagineUrl { get; set; }

        public bool Disponibile { get; set; }

        [Required(ErrorMessage = "La priorità è obbligatoria")]
        [Range(1, 10, ErrorMessage = "La priorità deve essere tra 1 e 10")]
        public int Priorita { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}