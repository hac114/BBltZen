using System;
using System.Collections.Generic;

namespace Database;

public partial class SessioniQr
{
    public Guid SessioneId { get; set; }

    public int ClienteId { get; set; }

    public string QrCode { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime DataScadenza { get; set; }

    public bool? Utilizzato { get; set; }

    public DateTime? DataUtilizzo { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
