using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class IngredienteDTO
    {
        public int IngredienteId { get; set; }

        [StringLength(50, ErrorMessage = "Il nome non può superare 50 caratteri")]
        public required string Nome { get; set; }

        public int CategoriaId { get; set; }

        [Range(0, 5, ErrorMessage = "Il prezzo aggiunto deve essere tra 0 e 5")]
        public decimal PrezzoAggiunto { get; set; }

        public bool Disponibile { get; set; }

        public DateTime DataInserimento { get; set; }

        public DateTime DataAggiornamento { get; set; }
    }
}