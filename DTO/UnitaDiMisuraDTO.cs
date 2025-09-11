using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class UnitaDiMisuraDTO
    {
        public int UnitaMisuraId { get; set; }
        public string Sigla { get; set; } = null!;
        public string Descrizione { get; set; } = null!;
    }
}
