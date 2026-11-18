
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lab06_Julio_Cahuascano.Data;
using Lab06_Julio_Cahuascano.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Lab06_Julio_Cahuascano.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DbConnection _db;

    private const string JwtIssuer = "TuIssuer";
    private const string JwtAudience = "TuAudience";
    private const string JwtKey = "TuClaveSecretaSuperSegura123456789!";

    public AuthController(DbConnection db)
    {
        _db = db;
    }

    // REGISTRO DE USUARIOS
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            return BadRequest(new
            {
                message = "El nombre de usuario es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BadRequest(new
            {
                message = "La contraseña es obligatoria."
            });
        }

        if (user.PasswordHash.Length < 8)
        {
            return BadRequest(new
            {
                message = "La contraseña debe tener al menos 8 caracteres."
            });
        }

        using var connection = _db.CreateConnection();
        await connection.OpenAsync();

        var username = user.Username.Trim();

        // Comprobar si el usuario ya existe
        var checkSql = """
            SELECT EXISTS(
                SELECT 1
                FROM users
                WHERE username = @username
            );
            """;

        using var checkCommand = new NpgsqlCommand(checkSql, connection);
        checkCommand.Parameters.AddWithValue("@username", username);

        var exists = (bool)(await checkCommand.ExecuteScalarAsync() ?? false);

        if (exists)
        {
            return Conflict(new
            {
                message = "El nombre de usuario ya está registrado."
            });
        }

        // Encriptar contraseña
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        // Todo registro público tendrá el rol user
        var sql = """
            INSERT INTO users (username, password_hash, role)
            VALUES (@username, @password_hash, @role)
            RETURNING id_user;
            """;

        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password_hash", passwordHash);
        command.Parameters.AddWithValue("@role", "user");

        var id = await command.ExecuteScalarAsync();

        return Ok(new
        {
            message = "Usuario registrado correctamente.",
            id
        });
    }

    // INICIO DE SESIÓN
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) ||
            string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BadRequest(new
            {
                message = "Debes ingresar usuario y contraseña."
            });
        }

        using var connection = _db.CreateConnection();
        await connection.OpenAsync();

        var sql = """
            SELECT id_user, username, password_hash, role
            FROM users
            WHERE username = @username;
            """;

        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@username",
            user.Username.Trim()
        );

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return Unauthorized(new
            {
                message = "Usuario o contraseña incorrectos."
            });
        }

        var id = reader.GetInt32(0);
        var username = reader.GetString(1);
        var passwordHash = reader.GetString(2);
        var role = reader.GetString(3);

        if (!BCrypt.Net.BCrypt.Verify(user.PasswordHash, passwordHash))
        {
            return Unauthorized(new
            {
                message = "Usuario o contraseña incorrectos."
            });
        }

        var token = GenerateToken(id, username, role);

        return Ok(new
        {
            message = "Login correcto.",
            token,
            usuario = username,
            rol = role
        });
    }

    // GENERAR TOKEN JWT
    private string GenerateToken(int id, string username, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(JwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ENDPOINT SOLO PARA ADMINISTRADORES
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult GetAdminData()
    {
        return Ok(new
        {
            message = "Datos solo para administradores.",
            usuario = User.Identity?.Name,
            rol = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    // ENDPOINT PARA USUARIOS AUTENTICADOS
    [Authorize]
    [HttpGet("usuario")]
    public IActionResult GetUserData()
    {
        return Ok(new
        {
            message = "Datos para usuarios autenticados.",
            usuario = User.Identity?.Name,
            rol = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    // PERFIL DEL USUARIO AUTENTICADO
    [Authorize]
    [HttpGet("perfil")]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            usuario = User.Identity?.Name,
            id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            rol = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}
