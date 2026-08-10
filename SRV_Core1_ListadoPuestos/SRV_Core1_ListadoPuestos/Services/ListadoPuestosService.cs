using Core1.ListadoPuestos.Entities;
using Core1.ListadoPuestos.Repository;

namespace Core1.ListadoPuestos.Services
{
    /// <summary>
    /// Core1 - "Yo como administrador del sistema quiero un microservicio de
    /// listado de puestos para obtener los puestos activos".
    /// </summary>
    public class ListadoPuestosService : IListadoPuestosService
    {
        private readonly PuestoRepository _puestos;
        private readonly IBitacoraRepository _bitacora;

        public ListadoPuestosService(PuestoRepository puestos, IBitacoraRepository bitacora)
        {
            _puestos = puestos;
            _bitacora = bitacora;
        }

        public async Task<IEnumerable<PuestoActivo>> ObtenerActivosAsync()
        {
            var lista = (await _puestos.ObtenerActivosAsync()).ToList();

            await _bitacora.RegistrarAsync("SELECT", "puesto",
                "Se consulta el listado de puestos activos.");

            return lista;
        }
    }
}
