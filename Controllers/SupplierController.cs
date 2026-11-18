
using Lab06_Julio_Cahuascano.Data;
using Lab06_Julio_Cahuascano.Models.Scaffolded;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Lab06_Julio_Cahuascano.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly InventarioContext _context;

    public SupplierController(InventarioContext context)
    {
        _context = context;
    }

    // GET: api/Supplier
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .ToListAsync();

        return Ok(suppliers);
    }

    // GET: api/Supplier/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Supplier>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSupplier == id);

        if (supplier == null)
            return NotFound(new { message = "Proveedor no encontrado." });

        return Ok(supplier);
    }

    // POST: api/Supplier
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Supplier>> CreateSupplier(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest(new { message = "El nombre del proveedor es obligatorio." });

        supplier.IdSupplier = 0;
        supplier.Name = supplier.Name.Trim();
        supplier.Phone = supplier.Phone?.Trim();
        supplier.Email = supplier.Email?.Trim();
        supplier.Address = supplier.Address?.Trim();

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.IdSupplier },
            supplier
        );
    }

    // PUT: api/Supplier/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSupplier(int id, Supplier supplier)
    {
        if (id != supplier.IdSupplier)
            return BadRequest(new { message = "Los identificadores no coinciden." });

        if (string.IsNullOrWhiteSpace(supplier.Name))
            return BadRequest(new { message = "El nombre del proveedor es obligatorio." });

        var existingSupplier = await _context.Suppliers.FindAsync(id);

        if (existingSupplier == null)
            return NotFound(new { message = "Proveedor no encontrado." });

        existingSupplier.Name = supplier.Name.Trim();
        existingSupplier.Phone = supplier.Phone?.Trim();
        existingSupplier.Email = supplier.Email?.Trim();
        existingSupplier.Address = supplier.Address?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Proveedor actualizado correctamente.",
            supplier = existingSupplier
        });
    }

    // DELETE: api/Supplier/1
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.IdSupplier == id);

        if (supplier == null)
            return NotFound(new { message = "Proveedor no encontrado." });

        if (supplier.Products.Any())
            return Conflict(new
            {
                message = "No se puede eliminar un proveedor que tiene productos asociados."
            });

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Proveedor eliminado correctamente." });
    }
}
