using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatisticheCacheDTO
    {
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "Il tipo statistica non può superare 50 caratteri")]
        public string TipoStatistica { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Il periodo non può superare 20 caratteri")]
        public string Periodo { get; set; } = null!;

        [StringLength(4000, ErrorMessage = "Le metriche non possono superare 4000 caratteri")]
        public string Metriche { get; set; } = null!;

        public DateTime? DataAggiornamento { get; set; }
    }

    public class AggiornaCacheRequestDTO
    {
        [Required(ErrorMessage = "Il tipo statistica è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il tipo statistica non può superare 50 caratteri")]
        public required string TipoStatistica { get; set; }

        [Required(ErrorMessage = "Il periodo è obbligatorio")]
        [StringLength(20, ErrorMessage = "Il periodo non può superare 20 caratteri")]
        public required string Periodo { get; set; }

        [Required(ErrorMessage = "Le metriche sono obbligatorie")]
        [StringLength(4000, ErrorMessage = "Le metriche non possono superare 4000 caratteri")]
        public required string Metriche { get; set; }
    }
}