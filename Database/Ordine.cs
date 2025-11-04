using System;
using System.Collections.Generic;

namespace Database;

public partial class Ordine
{
    public int OrdineId { get; set; }

    public int ClienteId { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public int? StatoOrdineId { get; set; }

    public int? StatoPagamentoId { get; set; }

    public decimal Totale { get; set; }

    public int Priorita { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();

    public virtual StatoOrdine? StatoOrdine { get; set; }

    public virtual StatoPagamento? StatoPagamento { get; set; }

    public virtual ICollection<StatoStoricoOrdine> StatoStoricoOrdine { get; set; } = new List<StatoStoricoOrdine>();
}
