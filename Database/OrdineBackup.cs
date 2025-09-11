using System;
using System.Collections.Generic;

namespace Database;

public partial class OrdineBackup
{
    public int OrdineId { get; set; }

    public int ClienteId { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public int? StatoOrdineId { get; set; }

    public int? StatoPagamentoId { get; set; }

    public decimal Totale { get; set; }

    public int? Priorita { get; set; }
}
