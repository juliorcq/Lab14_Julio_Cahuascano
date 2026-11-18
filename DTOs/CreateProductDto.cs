using System.ComponentModel.DataAnnotations;

namespace Lab06_Julio_Cahuascano.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor que cero.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int IdCategory { get; set; }

    public int? IdSupplier { get; set; }
}