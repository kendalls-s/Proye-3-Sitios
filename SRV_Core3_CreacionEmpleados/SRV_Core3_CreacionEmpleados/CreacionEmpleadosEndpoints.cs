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
            // Body: { idOferente, idPuesto, fechaIngreso }
            group.MapPost("", async (
                CrearEmpleadoRequest request,
                ICreacionEmpleadosService service,
                IBitacoraRepository bitacora) =>
            {
                if (request is null)
                    return Results.BadRequest(new { message = "El cuerpo de la solicitud es requerido." });

                if (request.IdOferente <= 0)
                    return Results.BadRequest(new { message = "idOferente es requerido y debe ser mayor que cero." });

                if (request.IdPuesto <= 0)
                    return Results.BadRequest(new { message = "idPuesto es requerido y debe ser mayor que cero." });

                if (request.FechaIngreso == default)
                    return Results.BadRequest(new { message = "fechaIngreso es requerida." });

                try
                {
                    var resultado = await service.CrearEmpleadoAsync(
                        request.IdOferente,
                        request.IdPuesto,
                        request.FechaIngreso);

                    if (!resultado.oferenteExiste)
                        return Results.NotFound(new { message = $"No existe el oferente con id '{request.IdOferente}'." });

                    if (!resultado.puestoExiste)
                        return Results.NotFound(new { message = $"No existe el puesto con id '{request.IdPuesto}'." });

                    if (!resultado.puestoDisponible)
                        return Results.Conflict(new { message = "El puesto seleccionado no está disponible." });

                    if (resultado.yaEsEmpleado)
                        return Results.Conflict(new { message = "El oferente seleccionado ya está registrado como empleado." });

                    return Results.Json(new
                    {
                        success = true,
                        statusCode = 201,
                        message = "Empleado creado con éxito.",
                        data = resultado.empleado
                    }, statusCode: StatusCodes.Status201Created);
                }
                catch (Exception ex)
                {
                    await bitacora.RegistrarAsync("ERROR", "empleado",
                        $"Error técnico al crear empleado. Oferente={request.IdOferente}, Puesto={request.IdPuesto}: {ex.Message}");

                    return Results.Problem(
                        statusCode: 500,
                        title: "Error al crear el empleado",
                        detail: ex.Message);
                }
            })
            .WithName("CrearEmpleado");
        }
    }
}
