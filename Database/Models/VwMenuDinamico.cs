using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwMenuDinamico
{
    public string Tipo { get; set; } = null!;

    public int Id { get; set; }

    public string NomeBevanda { get; set; } = null!;

    public string? Descrizione { get; set; }

    public decimal PrezzoNetto { get; set; }

    public decimal? PrezzoLordo { get; set; }

    public int TaxRateId { get; set; }

    public decimal IvaPercentuale { get; set; }

    public string? ImmagineUrl { get; set; }

    public int Priorita { get; set; }
}
