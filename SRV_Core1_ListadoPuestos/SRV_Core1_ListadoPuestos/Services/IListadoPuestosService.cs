using Core1.ListadoPuestos.Entities;

namespace Core1.ListadoPuestos.Services
{
    public interface IListadoPuestosService
    {
        Task<IEnumerable<PuestoActivo>> ObtenerActivosAsync();
    }
}
