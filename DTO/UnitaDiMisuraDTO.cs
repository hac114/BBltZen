using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class UnitaDiMisuraDTO
    {
        public int UnitaMisuraId { get; set; }

        [StringLength(2, ErrorMessage = "La sigla non può superare 2 caratteri")]
        public string Sigla { get; set; } = null!;

        [StringLength(10, ErrorMessage = "La descrizione non può superare 10 caratteri")]
        public string Descrizione { get; set; } = null!;
    }
}