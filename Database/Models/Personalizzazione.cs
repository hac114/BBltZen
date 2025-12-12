using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class Personalizzazione
{
    public int PersonalizzazioneId { get; set; }

    public string Nome { get; set; } = null!;

    public DateTime DtCreazione { get; set; }

    public string Descrizione { get; set; } = null!;

    public virtual ICollection<BevandaStandard> BevandaStandard { get; set; } = new List<BevandaStandard>();

    public virtual ICollection<PersonalizzazioneIngrediente> PersonalizzazioneIngrediente { get; set; } = new List<PersonalizzazioneIngrediente>();
}
