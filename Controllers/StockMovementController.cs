
using Lab06_Julio_Cahuascano.Data;
using Lab06_Julio_Cahuascano.Models.Scaffolded;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Lab06_Julio_Cahuascano.Controllers;

[ApiController]
[Route("api/stock-movements")]
[Authorize]
public class StockMovementController : ControllerBase
{
    private readonly InventarioContext _context;

    public StockMovementController(InventarioContext context)
    {
        _context = context;
    }

    // GET: api/stock-movements
    [HttpGet]
    public async Task<IActionResult> GetMovements()
    {
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Select(m => new
            {
                m.IdMovement,
                m.IdProduct,
                ProductName = m.IdProductNavigation.Name,
                m.IdUser,
                m.MovementType,
                m.Quantity,
                m.MovementDate,
                m.Description
            })
            .OrderByDescending(m => m.MovementDate)
            .ToListAsync();

        return Ok(movements);
    }

    // GET: api/stock-movements/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMovement(int id)
    {
        var movement = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.IdMovement == id)
            .Select(m => new
            {
                m.IdMovement,
                m.IdProduct,
                ProductName = m.IdProductNavigation.Name,
                m.IdUser,
                m.MovementType,
                m.Quantity,
                m.MovementDate,
                m.Description
            })
            .FirstOrDefaultAsync();

        if (movement == null)
            return NotFound(new { message = "Movimiento no encontrado." });

        return Ok(movement);
    }

    // POST: api/stock-movements
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMovement(
        [FromBody] StockMovement movement)
    {
        if (movement.Quantity <= 0)
            return BadRequest(new { message = "La cantidad debe ser mayor que cero." });

        if (string.IsNullOrWhiteSpace(movement.MovementType))
            return BadRequest(new { message = "El tipo de movimiento es obligatorio." });

        movement.MovementType = movement.MovementType.Trim().ToLower();

        if (movement.MovementType != "entrada" &&
            movement.MovementType != "salida")
            return BadRequest(new { message = "El tipo debe ser entrada o salida." });

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.IdProduct == movement.IdProduct);

            if (product == null)
                return NotFound(new { message = "Producto no encontrado." });

            var userExists = await _context.Users
                .AnyAsync(u => u.IdUser == movement.IdUser);

            if (!userExists)
                return BadRequest(new { message = "El usuario indicado no existe." });

            if (movement.MovementType == "salida" &&
                product.Stock < movement.Quantity)
                return BadRequest(new { message = "No hay suficiente stock disponible." });

            if (movement.MovementType == "entrada")
                product.Stock += movement.Quantity;
            else
                product.Stock -= movement.Quantity;

            movement.IdMovement = 0;
            movement.MovementDate = null;
            movement.Description = movement.Description?.Trim();

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Movimiento registrado correctamente.",
                product = product.Name,
                movementType = movement.MovementType,
                quantity = movement.Quantity,
                currentStock = product.Stock,
                movementDate = movement.MovementDate
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
