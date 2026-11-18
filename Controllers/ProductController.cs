
using Lab06_Julio_Cahuascano.Data;
using Lab06_Julio_Cahuascano.DTOs;
using Lab06_Julio_Cahuascano.Models.Scaffolded;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Lab06_Julio_Cahuascano.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly InventarioContext _context;

    public ProductController(InventarioContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Select(p => new ProductResponseDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IdCategory = p.IdCategory,
                CategoryName = p.IdCategoryNavigation.Name,
                IdSupplier = p.IdSupplier,
                SupplierName = p.IdSupplierNavigation != null
                    ? p.IdSupplierNavigation.Name
                    : null
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.IdProduct == id)
            .Select(p => new ProductResponseDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IdCategory = p.IdCategory,
                CategoryName = p.IdCategoryNavigation.Name,
                IdSupplier = p.IdSupplier,
                SupplierName = p.IdSupplierNavigation != null
                    ? p.IdSupplierNavigation.Name
                    : null
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound(new { message = "Producto no encontrado." });

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(
        [FromBody] CreateProductDto dto)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.IdCategory == dto.IdCategory);

        if (!categoryExists)
            return BadRequest(new { message = "La categoría indicada no existe." });

        if (dto.IdSupplier.HasValue)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.IdSupplier == dto.IdSupplier.Value);

            if (!supplierExists)
                return BadRequest(new { message = "El proveedor indicado no existe." });
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            Stock = 0,
            IdCategory = dto.IdCategory,
            IdSupplier = dto.IdSupplier
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = await GetProductResponse(product.IdProduct);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.IdProduct },
            response
        );
    }

    // PUT: api/products/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound(new { message = "Producto no encontrado." });

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.IdCategory == dto.IdCategory);

        if (!categoryExists)
            return BadRequest(new { message = "La categoría indicada no existe." });

        if (dto.IdSupplier.HasValue)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.IdSupplier == dto.IdSupplier.Value);

            if (!supplierExists)
                return BadRequest(new { message = "El proveedor indicado no existe." });
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.IdCategory = dto.IdCategory;
        product.IdSupplier = dto.IdSupplier;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Producto actualizado correctamente.",
            product = await GetProductResponse(id)
        });
    }

    // DELETE: api/products/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.StockMovements)
            .FirstOrDefaultAsync(p => p.IdProduct == id);

        if (product == null)
            return NotFound(new { message = "Producto no encontrado." });

        if (product.StockMovements.Any())
        {
            return Conflict(new
            {
                message = "No se puede eliminar un producto que tiene movimientos registrados."
            });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Producto eliminado correctamente." });
    }

    private async Task<ProductResponseDto?> GetProductResponse(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IdProduct == id)
            .Select(p => new ProductResponseDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IdCategory = p.IdCategory,
                CategoryName = p.IdCategoryNavigation.Name,
                IdSupplier = p.IdSupplier,
                SupplierName = p.IdSupplierNavigation != null
                    ? p.IdSupplierNavigation.Name
                    : null
            })
            .FirstOrDefaultAsync();
    }
}
