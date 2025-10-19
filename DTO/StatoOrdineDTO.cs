using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatoOrdineDTO
    {
        public int StatoOrdineId { get; set; }

        [StringLength(50, ErrorMessage = "Lo stato ordine non può superare 50 caratteri")]
        public string StatoOrdine1 { get; set; } = null!;

        public bool Terminale { get; set; }
    }
}