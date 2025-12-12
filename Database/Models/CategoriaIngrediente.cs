using System;
using System.Collections.Generic;

namespace Database.Models;

public partial class CategoriaIngrediente
{
    public int CategoriaId { get; set; }

    public string Categoria { get; set; } = null!;

    public virtual ICollection<Ingrediente> Ingrediente { get; set; } = new List<Ingrediente>();
}
