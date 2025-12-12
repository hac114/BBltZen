using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class TempPriceCalculations
{
    public int TempId { get; set; }

    public int? ArticoloId { get; set; }

    public int? PersCustomId { get; set; }

    public decimal? PrezzoCalcolato { get; set; }

    public DateTime? DataCalcolo { get; set; }

    public string? CalcolatoDa { get; set; }
}
