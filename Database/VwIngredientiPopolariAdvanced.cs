using System;
using System.Collections.Generic;

namespace Database;

public partial class VwIngredientiPopolariAdvanced
{
    public int IngredienteId { get; set; }

    public string NomeIngrediente { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public int? NumeroSelezioni { get; set; }

    public int? NumeroOrdiniContenenti { get; set; }

    public decimal? PercentualeTotale { get; set; }

    public int? GiorniConSelezioni { get; set; }

    public decimal? PercentualeSuOrdiniTotali { get; set; }
}
