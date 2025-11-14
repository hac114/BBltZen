using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PreferitiClienteDTO
    {
        public int PreferitoId { get; set; }
        public int ClienteId { get; set; }
        public int BevandaId { get; set; }
        public DateTime? DataAggiunta { get; set; }
        public string TipoArticolo { get; set; } = null!;
        public string? NomePersonalizzato { get; set; }
        public byte? GradoDolcezza { get; set; }
        public int? DimensioneBicchiereId { get; set; }
        public string? IngredientiJson { get; set; }
        public string? NotePersonali { get; set; }
    }
}
