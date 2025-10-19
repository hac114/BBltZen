using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DolceDTO
    {
        public int ArticoloId { get; set; }

        [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri")]
        public string Nome { get; set; } = null!;

        [Range(0.01, 50, ErrorMessage = "Il prezzo deve essere tra 0.01 e 50")]
        public decimal Prezzo { get; set; }

        [StringLength(255, ErrorMessage = "La descrizione non può superare 255 caratteri")]
        public string? Descrizione { get; set; }

        [StringLength(500, ErrorMessage = "L'URL immagine non può superare 500 caratteri")]
        public string? ImmagineUrl { get; set; }

        public bool Disponibile { get; set; }

        [Range(0, 10, ErrorMessage = "La priorità deve essere tra 0 e 10")]
        public int Priorita { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}