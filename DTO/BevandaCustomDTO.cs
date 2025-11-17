using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BevandaCustomDTO
    {
        public int BevandaCustomId { get; set; }

        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "L'ID personalizzazione custom è obbligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "L'ID personalizzazione custom deve essere maggiore di 0")]
        public int PersCustomId { get; set; }

        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Range(0.01, 100, ErrorMessage = "Il prezzo deve essere compreso tra 0.01 e 100")]
        public decimal Prezzo { get; set; }

        public DateTime DataCreazione { get; set; }

        public DateTime DataAggiornamento { get; set; }
    }
}