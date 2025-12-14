using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class VwDashboardSintesi
{
    public string Stato { get; set; } = null!;

    public int? Quantita { get; set; }

    public int? MaxMinuti { get; set; }

    public int? InRitardoCritico { get; set; }
}
