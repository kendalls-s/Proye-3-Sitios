using Core3.CreacionEmpleados.Entities;

namespace Core3.CreacionEmpleados.Services
{
    public interface ICreacionEmpleadosService
    {
        Task<(bool oferenteExiste, bool puestoExiste, bool yaEsEmpleado, EmpleadoCreado? empleado)> ContratarAsync(
            string identificacion, string codigoPuesto);
    }
}
