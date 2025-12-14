using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class PersonalizzazioneCustom
{
    public int PersCustomId { get; set; }

    public string Nome { get; set; } = null!;

    public byte GradoDolcezza { get; set; }

    public int DimensioneBicchiereId { get; set; }

    public DateTime DataCreazione { get; set; }

    public DateTime DataAggiornamento { get; set; }

    public virtual ICollection<BevandaCustom> BevandaCustom { get; set; } = new List<BevandaCustom>();

    public virtual DimensioneBicchiere DimensioneBicchiere { get; set; } = null!;

    public virtual ICollection<IngredientiPersonalizzazione> IngredientiPersonalizzazione { get; set; } = new List<IngredientiPersonalizzazione>();
}
