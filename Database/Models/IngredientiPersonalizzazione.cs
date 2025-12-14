using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class IngredientiPersonalizzazione
{
    public int IngredientePersId { get; set; }

    public int PersCustomId { get; set; }

    public int IngredienteId { get; set; }

    public DateTime DataCreazione { get; set; }

    public virtual Ingrediente Ingrediente { get; set; } = null!;

    public virtual PersonalizzazioneCustom PersCustom { get; set; } = null!;
}
