using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class IngredienteDTO
    {
        public int IngredienteId { get; set; }
        public required string Nome { get; set; }
        public int CategoriaId { get; set; }
        public decimal PrezzoAggiunto { get; set; }
        public bool Disponibile { get; set; }
        public DateTime DataInserimento { get; set; }
        public DateTime DataAggiornamento { get; set; }
    }
}
