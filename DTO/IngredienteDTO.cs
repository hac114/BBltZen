using System.ComponentModel.DataAnnotations;

namespace DTO
{
    
    public class IngredienteDTO
    {
        // ⚠️ PER UPDATE/CREATE: In Create, sarà 0. In Update, avrà valore
        public int IngredienteId { get; set; }

        [Required(ErrorMessage = "Il nome dell'ingrediente è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il nome non può superare 50 caratteri")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "La categoria è obbligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleziona una categoria valida")]
        public int CategoriaId { get; set; }

        // ⚠️ SOLO PER LETTURA (non per create/update)
        public string CategoriaNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0, 5, ErrorMessage = "Il prezzo aggiunto deve essere tra 0 e 5")]
        public decimal PrezzoAggiunto { get; set; }

        [Required(ErrorMessage = "Specifica la disponibilità")]
        public bool Disponibile { get; set; } = true;

        // ⚠️ SOLO PER LETTURA (non per create/update)
        public DateTime DataInserimento { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}