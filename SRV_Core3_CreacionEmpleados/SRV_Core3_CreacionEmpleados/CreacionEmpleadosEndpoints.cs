using Core3.CreacionEmpleados.Entities;
using Core3.CreacionEmpleados.Repository;
using Core3.CreacionEmpleados.Services;

namespace Core3.CreacionEmpleados
{
    /// <summary>
    /// Core3 - "Yo como administrador del sistema quiero un servicio que permita
    /// registrar un empleado en el sistema".
    ///
    /// Web service REST que crea un nuevo empleado a partir de un oferente
    /// existente y un puesto. Recibe en el cuerpo la información requerida para
    /// la contratación (identificación del oferente + código de puesto) y a
    /// partir de ella copia toda la información del oferente hacia las
    /// estructuras de empleado, genera el número de empleado y registra la
    /// acción de personal de contratación.
    ///
    /// Contrato REST:
    ///   POST /empleados
    ///     201 Created  -> empleado creado (incluye cabecera Location).
    ///     400 Bad Request -> faltan datos requeridos en el cuerpo.
    ///     404 Not Found   -> el oferente o el puesto no existen.
    ///     409 Conflict    -> el oferente ya había sido contratado.
    ///     500 Problem     -> error técnico (queda registrado en bitácora).
    /// </summary>
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

                    // 201 Created con cabecera Location apuntando al recurso creado.
                    // Se conserva el mismo envelope { success, statusCode, message, data }
                    // que ya consume el cliente React.
                    var ubicacion = $"/empleados/{empleado!.NumeroEmpleado}";

                    return Results.Created(ubicacion, new
                    {
                        success = true,
                        statusCode = 201,
                        message = "Empleado creado con éxito.",
                        data = empleado
                    });
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
