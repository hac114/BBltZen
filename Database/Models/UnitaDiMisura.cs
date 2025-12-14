using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class UnitaDiMisura
{
    public int UnitaMisuraId { get; set; }

    public string Sigla { get; set; } = null!;

    public string Descrizione { get; set; } = null!;

    public virtual ICollection<DimensioneBicchiere> DimensioneBicchiere { get; set; } = new List<DimensioneBicchiere>();

    public virtual ICollection<PersonalizzazioneIngrediente> PersonalizzazioneIngrediente { get; set; } = new List<PersonalizzazioneIngrediente>();
}
