using Core3.CreacionEmpleados.Entities;

namespace Core3.CreacionEmpleados.Services
{
    public interface ICreacionEmpleadosService
    {
        Task<(bool oferenteExiste, bool puestoExiste, bool puestoDisponible, bool yaEsEmpleado, EmpleadoCreado? empleado)>
            CrearEmpleadoAsync(int idOferente, int idPuesto, DateTime fechaIngreso);
    }
}
