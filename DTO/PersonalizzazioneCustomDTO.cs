using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PersonalizzazioneCustomDTO
    {
        public int PersCustomId { get; set; }

        [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri")]
        public string? Nome { get; set; }

        [Range(1, 3, ErrorMessage = "Il grado dolcezza deve essere tra 1 e 3")]
        public byte GradoDolcezza { get; set; }

        public int DimensioneBicchiereId { get; set; }

        public DateTime DataCreazione { get; set; }

        public DateTime DataAggiornamento { get; set; }
    }
}