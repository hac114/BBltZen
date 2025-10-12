using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PersonalizzazioneCustomDTO
    {
        public int PersCustomId { get; set; }

        public string? Nome { get; set; }

        public byte GradoDolcezza { get; set; }

        public int DimensioneBicchiereId { get; set; }

        public DateTime DataCreazione { get; set; }

        public DateTime DataAggiornamento { get; set; }       
    }
}
