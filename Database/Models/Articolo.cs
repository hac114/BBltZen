using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class Articolo
{
    public int ArticoloId { get; set; }

    public string Tipo { get; set; } = null!;

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual BevandaCustom? BevandaCustom { get; set; }

    public virtual BevandaStandard? BevandaStandard { get; set; }

    public virtual Dolce? Dolce { get; set; }

    public virtual ICollection<OrderItem> OrderItem { get; set; } = new List<OrderItem>();
}
