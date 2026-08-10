using Core8.DetalleOferente.Entities;

namespace Core8.DetalleOferente.Services
{
    public interface IDetalleOferenteService
    {
        Task<OferenteDetalle?> ObtenerDetalleAsync(string identificacion);
    }
}
