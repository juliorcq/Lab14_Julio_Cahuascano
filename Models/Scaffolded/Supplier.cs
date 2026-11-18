using System;
using System.Collections.Generic;

namespace Lab06_Julio_Cahuascano.Models.Scaffolded;

public partial class Supplier
{
    public int IdSupplier { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
