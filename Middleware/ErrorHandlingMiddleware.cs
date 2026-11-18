using Newtonsoft.Json;

namespace Lab06_Julio_Cahuascano.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continúa con el siguiente middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            // Error interno del servidor
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                message = "Ocurrió un error interno",
                details = ex.Message
            };

            await context.Response.WriteAsync(
                JsonConvert.SerializeObject(errorResponse));
        }
    }
}