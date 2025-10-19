using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class BevandaStandardDTO
    {
        public int ArticoloId { get; set; }
        public int PersonalizzazioneId { get; set; }
        public int DimensioneBicchiereId { get; set; }

        [Range(0.01, 100, ErrorMessage = "Il prezzo deve essere tra 0.01 e 100")]
        public decimal Prezzo { get; set; }

        [StringLength(500, ErrorMessage = "L'URL immagine non può superare 500 caratteri")]
        public string? ImmagineUrl { get; set; }
        public bool Disponibile { get; set; }
        public bool SempreDisponibile { get; set; }

        [Range(0, 10, ErrorMessage = "La priorità deve essere tra 0 e 10")]
        public int Priorita { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
        public DimensioneBicchiereDTO? DimensioneBicchiere { get; set; }
    }
}
