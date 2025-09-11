using System;
using System.Collections.Generic;

namespace Database;

public partial class TaxRates
{
    public int TaxRateId { get; set; }

    public decimal Aliquota { get; set; }

    public string Descrizione { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime? DataAggiornamento { get; set; }

    public virtual ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();
}
