using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TavoloDTO
    {
        public int TavoloId { get; set; }
        public bool Disponibile { get; set; }

        [Range(1, 100, ErrorMessage = "Il numero tavolo deve essere tra 1 e 100")]
        public int Numero { get; set; }

        [StringLength(50, ErrorMessage = "La zona non può superare 50 caratteri")]
        public string? Zona { get; set; }
    }
}