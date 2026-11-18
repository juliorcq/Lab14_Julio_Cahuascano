using System;
using System.Collections.Generic;

namespace Lab06_Julio_Cahuascano.Models.Scaffolded;

public partial class StockMovement
{
    public int IdMovement { get; set; }

    public int IdProduct { get; set; }

    public int IdUser { get; set; }

    public string MovementType { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime? MovementDate { get; set; }

    public string? Description { get; set; }

    public virtual Product IdProductNavigation { get; set; } = null!;
}
