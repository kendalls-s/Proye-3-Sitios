using Core7.ListadoOferentes.Repository;
using Core7.ListadoOferentes.Services;

namespace Core7.ListadoOferentes
{
    public static class ListadoOferentesEndpoints
    {
        public static void MapListadoOferentesEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/listado-oferentes")
                .WithTags("Core7 - Listado de Oferentes")
                .RequireCors("ClientDev");

            // GET /listado-oferentes/{codigoPuesto}
            group.MapGet("/{codigoPuesto}", async (
                string codigoPuesto,
                IListadoOferentesService service,
                IBitacoraRepository bitacora) =>
            {
                if (string.IsNullOrWhiteSpace(codigoPuesto))
                    return Results.BadRequest(new { message = "El código de puesto es requerido." });

                try
                {
                    var listado = await service.ObtenerListadoPorPuestoAsync(codigoPuesto);

                    if (listado is null)
                        return Results.NotFound(new { message = $"No existe un puesto con código '{codigoPuesto}'." });

                    return Results.Ok(new
                    {
                        success = true,
                        statusCode = 200,
                        message = "Listado de oferentes obtenido correctamente.",
                        data = listado
                    });
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "oferente",
                        $"Error técnico al consultar el listado de oferentes para el puesto '{codigoPuesto}': {ex.Message}");
                    return Results.Problem(statusCode: 500,
                        title: "Error al consultar el listado de oferentes", detail: ex.Message);
                }
            })
            .WithName("ObtenerListadoOferentes");
        }
    }
}
