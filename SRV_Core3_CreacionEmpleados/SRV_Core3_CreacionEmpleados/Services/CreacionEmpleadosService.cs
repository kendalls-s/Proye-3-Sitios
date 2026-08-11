using Core3.CreacionEmpleados.Entities;
using Core3.CreacionEmpleados.Repository;

namespace Core3.CreacionEmpleados.Services
{
    /// <summary>
    /// Core3 - crea un empleado a partir de un oferente y puesto existentes.
    /// La numeración y la acción de personal se generan dentro de la misma
    /// transacción utilizada para insertar el empleado.
    /// </summary>
    public class CreacionEmpleadosService : ICreacionEmpleadosService
    {
        private readonly EmpleadoRepository _empleados;
        private readonly IBitacoraRepository _bitacora;

        public CreacionEmpleadosService(EmpleadoRepository empleados, IBitacoraRepository bitacora)
        {
            _empleados = empleados;
            _bitacora = bitacora;
        }

        public async Task<(bool oferenteExiste, bool puestoExiste, bool puestoDisponible, bool yaEsEmpleado, EmpleadoCreado? empleado)>
            CrearEmpleadoAsync(CrearEmpleadoRequest request)
        {
            OferenteBasico? oferente = null;

            if (request.IdOferente.HasValue && request.IdOferente.Value > 0)
            {
                oferente = await _empleados.ObtenerOferenteAsync(request.IdOferente.Value);
            }
            else if (!string.IsNullOrWhiteSpace(request.Identificacion))
            {
                oferente = await _empleados.ObtenerOferentePorIdentificacionAsync(request.Identificacion!.Trim());
            }

            if (oferente is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación rechazada: no existe el oferente '{request.Identificacion ?? request.IdOferente?.ToString()}'.");
                return (false, false, false, false, null);
            }

            PuestoBasico? puesto = null;

            if (request.IdPuesto.HasValue && request.IdPuesto.Value > 0)
            {
                puesto = await _empleados.ObtenerPuestoAsync(request.IdPuesto.Value);
            }
            else if (!string.IsNullOrWhiteSpace(request.CodigoPuesto))
            {
                puesto = await _empleados.ObtenerPuestoPorCodigoAsync(request.CodigoPuesto!.Trim());
            }

            if (puesto is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación rechazada: no existe el puesto '{request.CodigoPuesto ?? request.IdPuesto?.ToString()}'.");
                return (true, false, false, false, null);
            }

            if (!puesto.Disponible)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación rechazada: el puesto '{puesto.Codigo}' no está disponible.");
                return (true, true, false, false, null);
            }

            if (await _empleados.OferenteYaEsEmpleadoAsync(oferente.IdOferente))
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación rechazada: el oferente '{oferente.Identificacion}' ya es empleado.");
                return (true, true, true, true, null);
            }

            var resultado = await _empleados.CrearEmpleadoConBloqueoAsync(oferente, puesto, request.FechaIngreso!.Value.Date);

            if (resultado.YaEsEmpleado || resultado.Empleado is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación rechazada bajo bloqueo: el oferente '{oferente.Identificacion}' ya es empleado.");
                return (true, true, true, true, null);
            }

            await _bitacora.RegistrarAsync("INSERT", "empleado",
                $"Se crea el empleado {resultado.Empleado.NumeroEmpleado} para el oferente '{oferente.Identificacion}' en el puesto '{puesto.Codigo}'.");

            return (true, true, true, false, resultado.Empleado);
        }
    }
}
