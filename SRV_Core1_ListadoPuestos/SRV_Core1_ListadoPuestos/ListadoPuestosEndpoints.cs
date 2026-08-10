using Core1.ListadoPuestos.Repository;
using Core1.ListadoPuestos.Services;

namespace Core1.ListadoPuestos
{
    public static class ListadoPuestosEndpoints
    {
        public static void MapListadoPuestosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/puestos-activos")
                .WithTags("Core1 - Listado de Puestos")
                .RequireCors("ClientDev");

            // GET /puestos-activos
            group.MapGet("/", async (
                IListadoPuestosService service,
                IBitacoraRepository bitacora) =>
            {
                try
                {
                    var puestos = await service.ObtenerActivosAsync();
                    return Results.Ok(new
                    {
                        success = true,
                        statusCode = 200,
                        message = "Puestos obtenidos correctamente.",
                        data = puestos
                    });
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "puesto",
                        $"Error técnico al consultar el listado de puestos activos: {ex.Message}");
                    return Results.Problem(statusCode: 500,
                        title: "Error al consultar el listado de puestos activos", detail: ex.Message);
                }
            })
            .WithName("ObtenerPuestosActivos");
        }
    }
}
