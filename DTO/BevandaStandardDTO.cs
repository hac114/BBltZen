using System;
using System.Collections.Generic;
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
        public decimal Prezzo { get; set; }
        public string ImmagineUrl { get; set; }
        public bool Disponibile { get; set; }
        public bool SempreDisponibile { get; set; }
        public int Priorita { get; set; }
        public DimensioneBicchiereDTO DimensioneBicchiere { get; set; }
    }
}
