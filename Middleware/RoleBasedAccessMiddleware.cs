using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Lab06_Julio_Cahuascano.Middleware;

public class RoleBasedAccessMiddleware
{
    private readonly RequestDelegate _next;

    public RoleBasedAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Permitir acceso a endpoints públicos
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        // Obtener el rol del usuario autenticado
        var userRole = context.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        // Verificar permisos
        if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
        {
            context.Response.StatusCode = 403;

            await context.Response.WriteAsync(
                "Acceso denegado. El rol del usuario no tiene permisos.");

            return;
        }

        // Si es Admin, continúa
        await _next(context);
    }
}