using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class SessioniQr
{
    public Guid SessioneId { get; set; }

    public int? ClienteId { get; set; }

    public string QrCode { get; set; } = null!;

    public DateTime? DataCreazione { get; set; }

    public DateTime DataScadenza { get; set; }

    public bool? Utilizzato { get; set; }

    public DateTime? DataUtilizzo { get; set; }

    public int TavoloId { get; set; }

    public string CodiceSessione { get; set; } = null!;

    public string Stato { get; set; } = null!;

    public virtual ICollection<Ordine> Ordine { get; set; } = new List<Ordine>();

    public virtual Tavolo Tavolo { get; set; } = null!;
}
