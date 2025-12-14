using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatoOrdineDTO
    {
        public int StatoOrdineId { get; set; }

        [Required(ErrorMessage = "Il nome dello stato è obbligatorio")]
        [StringLength(100, ErrorMessage = "Lo stato ordine non può superare 100 caratteri")]
        [RegularExpression(@"^[a-zA-Zàèéìòù\s_-]+$", ErrorMessage = "Sono ammessi solo lettere, spazi, trattini e underscore")]
        public string StatoOrdine1 { get; set; } = string.Empty; // ✅ NOME ORIGINALE del modello EF

        [Required(ErrorMessage = "Il campo terminale è obbligatorio")]
        public bool Terminale { get; set; }
    }
}