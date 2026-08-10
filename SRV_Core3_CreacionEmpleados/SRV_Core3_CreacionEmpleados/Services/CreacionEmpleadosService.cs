using Core3.CreacionEmpleados.Entities;
using Core3.CreacionEmpleados.Repository;

namespace Core3.CreacionEmpleados.Services
{
    /// <summary>
    /// Core3 - Servicio de creación de empleados. Registra un nuevo empleado a
    /// partir de un oferente existente y un puesto, generando el número de
    /// empleado y la acción de personal de contratación correspondiente.
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

        public async Task<(bool oferenteExiste, bool puestoExiste, bool yaEsEmpleado, EmpleadoCreado? empleado)>
            ContratarAsync(string identificacion, string codigoPuesto)
        {
            var ident = (identificacion ?? string.Empty).Trim();
            var codigo = (codigoPuesto ?? string.Empty).Trim();

            var oferente = await _empleados.ObtenerOferentePorIdentificacionAsync(ident);
            if (oferente is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación de empleado rechazada: no existe oferente con identificación '{ident}'.");
                return (false, false, false, null);
            }

            var puesto = await _empleados.ObtenerPuestoPorCodigoAsync(codigo);
            if (puesto is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación de empleado rechazada: no existe puesto con código '{codigo}'.");
                return (true, false, false, null);
            }

            // Verificación temprana (no autoritativa) para responder rápido en el
            // caso común; la verificación definitiva ocurre bajo el bloqueo de la
            // tabla "empleado" dentro de la transacción, evitando condiciones de
            // carrera cuando dos solicitudes llegan al mismo tiempo.
            if (await _empleados.OferenteYaEsEmpleadoAsync(oferente.IdOferente))
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación de empleado rechazada: el oferente '{ident}' ya fue contratado anteriormente.");
                return (true, true, true, null);
            }

            var resultado = await _empleados.CrearEmpleadoConBloqueoAsync(oferente, puesto);

            if (resultado.YaEsEmpleado || resultado.Empleado is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "empleado",
                    $"Creación de empleado rechazada bajo bloqueo: el oferente '{ident}' ya fue contratado anteriormente.");
                return (true, true, true, null);
            }

            await _bitacora.RegistrarAsync("INSERT", "empleado",
                $"Se crea el empleado {resultado.Empleado.NumeroEmpleado} a partir del oferente '{ident}' " +
                $"para el puesto '{codigo}', junto con su acción de personal de contratación.");

            return (true, true, false, resultado.Empleado);
        }
    }
}
