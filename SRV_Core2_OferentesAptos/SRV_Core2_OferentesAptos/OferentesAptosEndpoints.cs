using Core2.OferentesAptos.Repository;
using Core2.OferentesAptos.Services;

namespace Core2.OferentesAptos
{
    public static class OferentesAptosEndpoints
    {
        public static void MapOferentesAptosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/oferentes-aptos")
                .WithTags("Core2 - Oferentes Aptos")
                .RequireCors("ClientDev");

            // GET /oferentes-aptos/{codigoPuesto}
            group.MapGet("/{codigoPuesto}", async (
                string codigoPuesto,
                IOferentesAptosService service,
                IBitacoraRepository bitacora) =>
            {
                if (string.IsNullOrWhiteSpace(codigoPuesto))
                    return Results.BadRequest(new { message = "El código de puesto es requerido." });

                try
                {
                    var (puestoExiste, oferentes) =
                        await service.ObtenerAptosPorPuestoAsync(codigoPuesto);

                    if (!puestoExiste)
                        return Results.NotFound(new { message = $"No existe un puesto con código '{codigoPuesto}'." });

                    return Results.Ok(oferentes);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "oferente",
                        $"Error técnico al consultar oferentes aptos para el puesto '{codigoPuesto}': {ex.Message}");
                    return Results.Problem(statusCode: 500,
                        title: "Error al consultar los oferentes aptos", detail: ex.Message);
                }
            })
            .WithName("ObtenerOferentesAptos");
        }
    }
}
