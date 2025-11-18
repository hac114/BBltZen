using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class StatoStoricoOrdineDTO
    {
        public int StatoStoricoOrdineId { get; set; }

        [Required(ErrorMessage = "L'ID ordine è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID ordine deve essere maggiore di 0")]
        public int OrdineId { get; set; }

        [Required(ErrorMessage = "L'ID stato ordine è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID stato ordine deve essere maggiore di 0")]
        public int StatoOrdineId { get; set; }

        [Required(ErrorMessage = "La data di inizio è obbligatoria")]
        public DateTime Inizio { get; set; } = DateTime.Now;

        public DateTime? Fine { get; set; }

        // ✅ METODO DI VALIDAZIONE CUSTOM
        public bool IsValid()
        {
            return Fine == null || Fine >= Inizio;
        }
    }
}