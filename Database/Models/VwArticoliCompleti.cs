using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class VwArticoliCompleti
{
    public int ArticoloId { get; set; }

    public string TipoArticolo { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime? DataAggiornamento { get; set; }

    public string? NomeArticolo { get; set; }

    public decimal? PrezzoBase { get; set; }

    public decimal? AliquotaIva { get; set; }

    public int? Disponibile { get; set; }

    public string Categoria { get; set; } = null!;
}
