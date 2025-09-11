using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DimensioneBicchiereDTO
    {
        public int DimensioneBicchiereId { get; set; }
        public string Sigla { get; set; }
        public string Descrizione { get; set; }
        public decimal Capienza { get; set; }
        public int UnitaMisuraId { get; set; }
        public decimal PrezzoBase { get; set; }
        public decimal Moltiplicatore { get; set; }
    }
}
