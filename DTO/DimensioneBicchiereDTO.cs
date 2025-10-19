using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DimensioneBicchiereDTO
    {
        public int DimensioneBicchiereId { get; set; }

        [StringLength(3, ErrorMessage = "La sigla non può superare 3 caratteri")]
        public required string Sigla { get; set; }

        [StringLength(50, ErrorMessage = "La descrizione non può superare 50 caratteri")]
        public required string Descrizione { get; set; }

        [Range(250, 1000, ErrorMessage = "La capienza deve essere tra 250 e 1000")]
        public decimal Capienza { get; set; }

        public int UnitaMisuraId { get; set; }

        [Range(0.01, 100, ErrorMessage = "Il prezzo base deve essere tra 0.01 e 100")]
        public decimal PrezzoBase { get; set; }

        [Range(0.1, 3, ErrorMessage = "Il moltiplicatore deve essere tra 0.1 e 3")]
        public decimal Moltiplicatore { get; set; }
    }
}