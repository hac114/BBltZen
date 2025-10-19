using System;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    public class PersonalizzazioneDTO
    {
        public int PersonalizzazioneId { get; set; }

        [StringLength(50, ErrorMessage = "Il nome non può superare 50 caratteri")]
        public string Nome { get; set; } = null!;

        [StringLength(500, ErrorMessage = "La descrizione non può superare 500 caratteri")]
        public string Descrizione { get; set; } = null!;

        public DateTime DtCreazione { get; set; }
        public DateTime DtUpdate { get; set; }
        public bool IsDeleted { get; set; }
    }
}