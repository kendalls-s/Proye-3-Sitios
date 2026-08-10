using Core8.DetalleOferente.Repository;
using Core8.DetalleOferente.Services;

namespace Core8.DetalleOferente
{
    public static class DetalleOferenteEndpoints
    {
        public static void MapDetalleOferenteEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/detalle-oferente")
                .WithTags("Core8 - Detalle de Oferente")
                .RequireCors("ClientDev");

            // GET /detalle-oferente/{identificacion}
            group.MapGet("/{identificacion}", async (
                string identificacion,
                IDetalleOferenteService service,
                IBitacoraRepository bitacora) =>
            {
                if (string.IsNullOrWhiteSpace(identificacion))
                    return Results.BadRequest(new { message = "La identificación es requerida." });

                try
                {
                    var detalle = await service.ObtenerDetalleAsync(identificacion);

                    if (detalle is null)
                        return Results.NotFound(new { message = $"No se encontró un oferente con identificación '{identificacion}'." });

                    return Results.Ok(detalle);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "oferente",
                        $"Error técnico al consultar el detalle del oferente '{identificacion}': {ex.Message}");
                    return Results.Problem(statusCode: 500,
                        title: "Error al consultar el detalle del oferente", detail: ex.Message);
                }
            })
            .WithName("ObtenerDetalleOferente");
        }
    }
}
