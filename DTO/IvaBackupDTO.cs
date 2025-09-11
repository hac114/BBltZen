using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class IvaBackupDTO
    {
        public int IvaId { get; set; }
        public decimal Aliquota { get; set; }
        public string Descrizione { get; set; } = null!;
        public string? CodiceCategoria { get; set; }
        public DateOnly? DataInizioValidita { get; set; }
        public DateOnly? DataFineValidita { get; set; }
    }
}
