using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class StatoPagamento
{
    public int StatoPagamentoId { get; set; }

    public string StatoPagamento1 { get; set; } = null!;

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();
}
