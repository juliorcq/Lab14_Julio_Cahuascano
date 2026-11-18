using System;
using System.Collections.Generic;

namespace Lab06_Julio_Cahuascano.Models.Scaffolded;

public partial class Category
{
    public int IdCategory { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
