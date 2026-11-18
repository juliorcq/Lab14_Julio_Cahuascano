using System;
using System.Collections.Generic;

namespace Lab06_Julio_Cahuascano.Models.Scaffolded;

public partial class Product
{
    public int IdProduct { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int IdCategory { get; set; }

    public int? IdSupplier { get; set; }

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual Supplier? IdSupplierNavigation { get; set; }

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
