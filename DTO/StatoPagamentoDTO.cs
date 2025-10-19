using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class StatoPagamentoDTO
    {
        public int StatoPagamentoId { get; set; }

        [StringLength(50, ErrorMessage = "Lo stato pagamento non può superare 50 caratteri")]
        public string StatoPagamento1 { get; set; } = null!;
    }
}