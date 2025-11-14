using System;
using System.Collections.Generic;

namespace Database;

public partial class StatisticheCache
{
    public int Id { get; set; }

    public string TipoStatistica { get; set; } = null!;

    public string Periodo { get; set; } = null!;

    public string Metriche { get; set; } = null!;

    public DateTime DataAggiornamento { get; set; }
}
