using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwOrdiniAnnullati
{
    public int OrdineId { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAnnullamento { get; set; }

    public int ClienteId { get; set; }

    public decimal Totale { get; set; }

    public int? MinutiPrimaAnnullamento { get; set; }
}
