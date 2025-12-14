using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwBevandePreferiteClienti
{
    public int PreferitoId { get; set; }

    public int ClienteId { get; set; }

    public int TavoloId { get; set; }

    public int BevandaId { get; set; }

    public string NomeBevanda { get; set; } = null!;

    public string Descrizione { get; set; } = null!;

    public decimal PrezzoNetto { get; set; }

    public decimal? PrezzoLordo { get; set; }

    public decimal IvaPercentuale { get; set; }

    public string? ImmagineUrl { get; set; }

    public bool BevandaDisponibile { get; set; }

    public DateTime? DataAggiunta { get; set; }
}
