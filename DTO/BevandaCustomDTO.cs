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
        public int PersCustomId { get; set; }
        [Range(0.01, 100, ErrorMessage = "Il prezzo deve essere tra 0.01 e 100")]
        public decimal Prezzo { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}
