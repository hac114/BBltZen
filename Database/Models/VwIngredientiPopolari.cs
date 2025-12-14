using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwIngredientiPopolari
{
    public int IngredienteId { get; set; }

    public string NomeIngrediente { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public int? NumeroSelezioni { get; set; }

    public int? NumeroOrdiniContenenti { get; set; }

    public decimal? PercentualeTotale { get; set; }
}
