using System;
using System.Collections.Generic;

namespace Database;

public partial class Personalizzazione
{
    public int PersonalizzazioneId { get; set; }

    public string Nome { get; set; } = null!;

    public DateTime DtCreazione { get; set; }

    public DateTime DtUpdate { get; set; }

    public string Descrizione { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<BevandaStandard> BevandaStandard { get; set; } = new List<BevandaStandard>();

    public virtual ICollection<PersonalizzazioneIngrediente> PersonalizzazioneIngrediente { get; set; } = new List<PersonalizzazioneIngrediente>();
}
