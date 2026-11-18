namespace Lab06_Julio_Cahuascano.DTOs;

public class ProductResponseDto
{
    public int IdProduct { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int IdCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? IdSupplier { get; set; }
    public string? SupplierName { get; set; }
}