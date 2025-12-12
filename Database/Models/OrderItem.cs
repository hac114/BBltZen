using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrdineId { get; set; }

    public int ArticoloId { get; set; }

    public int Quantita { get; set; }

    public decimal PrezzoUnitario { get; set; }

    public decimal ScontoApplicato { get; set; }

    public decimal Imponibile { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public string TipoArticolo { get; set; } = null!;

    public decimal? TotaleIvato { get; set; }

    public int TaxRateId { get; set; }

    public virtual Articolo Articolo { get; set; } = null!;

    public virtual Ordine Ordine { get; set; } = null!;

    public virtual TaxRates TaxRate { get; set; } = null!;
}
