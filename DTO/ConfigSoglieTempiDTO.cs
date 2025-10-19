using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ConfigSoglieTempiDTO
    {
        public int SogliaId { get; set; }
        public int StatoOrdineId { get; set; }

        [Range(0, 1000, ErrorMessage = "La soglia attenzione deve essere tra 0 e 1000")]
        public int SogliaAttenzione { get; set; }

        [Range(0, 1000, ErrorMessage = "La soglia critico deve essere tra 0 e 1000")]
        public int SogliaCritico { get; set; }
        public DateTime? DataAggiornamento { get; set; }

        [StringLength(100, ErrorMessage = "L'utente aggiornamento non può superare 100 caratteri")]
        public string? UtenteAggiornamento { get; set; }
    }
}
