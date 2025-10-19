using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ArticoloDTO
    {
        public int ArticoloId { get; set; }

        [Required(ErrorMessage = "Il tipo articolo è obbligatorio")]
        [StringLength(2, ErrorMessage = "Il tipo non può superare 2 caratteri")]
        public string Tipo { get; set; } = null!;

        public DateTime? DataCreazione { get; set; }

        public DateTime? DataAggiornamento { get; set; }
    }
}