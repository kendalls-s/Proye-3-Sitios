using Core3.CreacionEmpleados.Entities;
using Core3.CreacionEmpleados.Repository;
using Core3.CreacionEmpleados.Services;

namespace Core3.CreacionEmpleados
{
    public static class CreacionEmpleadosEndpoints
    {
        public static void MapCreacionEmpleadosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/empleados")
                .WithTags("Core3 - Creación de Empleados")
                .RequireCors("ClientDev");

            // POST /empleados
            group.MapPost("", async (
                CrearEmpleadoRequest request,
                ICreacionEmpleadosService service,
                IBitacoraRepository bitacora) =>
            {
                if (request is null || string.IsNullOrWhiteSpace(request.Identificacion))
                    return Results.BadRequest(new { message = "La identificación del oferente es requerida." });

                if (string.IsNullOrWhiteSpace(request.CodigoPuesto))
                    return Results.BadRequest(new { message = "El código de puesto es requerido." });

                try
                {
                    var (oferenteExiste, puestoExiste, yaEsEmpleado, empleado) =
                        await service.ContratarAsync(request.Identificacion, request.CodigoPuesto);

                    if (!oferenteExiste)
                        return Results.NotFound(new
                        {
                            message = $"No existe un oferente con identificación '{request.Identificacion}'."
                        });

                    if (!puestoExiste)
                        return Results.NotFound(new
                        {
                            message = $"No existe un puesto con código '{request.CodigoPuesto}'."
                        });

                    if (yaEsEmpleado)
                        return Results.Conflict(new
                        {
                            message = $"El oferente '{request.Identificacion}' ya fue registrado como empleado anteriormente."
                        });

                    return Results.Json(new
                    {
                        success = true,
                        statusCode = 201,
                        message = "Empleado creado con éxito.",
                        data = empleado
                    }, statusCode: StatusCodes.Status201Created);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "empleado",
                        $"Error técnico al crear el empleado a partir del oferente '{request?.Identificacion}': {ex.Message}");
                    return Results.Problem(statusCode: 500,
                        title: "Error al crear el empleado", detail: ex.Message);
                }
            })
            .WithName("CrearEmpleado");
        }
    }
}
