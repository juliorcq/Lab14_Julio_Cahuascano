
using Lab06_Julio_Cahuascano.Data;
using Lab06_Julio_Cahuascano.Models.Scaffolded;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Lab06_Julio_Cahuascano.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly InventarioContext _context;

    public CategoryController(InventarioContext context)
    {
        _context = context;
    }

    // GET: api/Category
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/Category/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategory == id);

        if (category == null)
            return NotFound(new { message = "Categoría no encontrada." });

        return Ok(category);
    }

    // POST: api/Category
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Category>> CreateCategory(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest(new { message = "El nombre es obligatorio." });

        category.IdCategory = 0;
        category.Name = category.Name.Trim();
        category.Description = category.Description?.Trim();

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.IdCategory },
            category
        );
    }

    // PUT: api/Category/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, Category category)
    {
        if (id != category.IdCategory)
            return BadRequest(new { message = "Los identificadores no coinciden." });

        if (string.IsNullOrWhiteSpace(category.Name))
            return BadRequest(new { message = "El nombre es obligatorio." });

        var existingCategory = await _context.Categories.FindAsync(id);

        if (existingCategory == null)
            return NotFound(new { message = "Categoría no encontrada." });

        existingCategory.Name = category.Name.Trim();
        existingCategory.Description = category.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Categoría actualizada correctamente.",
            category = existingCategory
        });
    }

    // DELETE: api/Category/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.IdCategory == id);

        if (category == null)
            return NotFound(new { message = "Categoría no encontrada." });

        if (category.Products.Any())
            return Conflict(new
            {
                message = "No se puede eliminar una categoría que tiene productos asociados."
            });

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Categoría eliminada correctamente." });
    }
}
