using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class BevandaCustom
{
    public int ArticoloId { get; set; }

    public int PersCustomId { get; set; }

    public decimal Prezzo { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual Articolo Articolo { get; set; } = null!;

    public virtual PersonalizzazioneCustom PersCustom { get; set; } = null!;
}
