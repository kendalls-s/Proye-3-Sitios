using Core8.DetalleOferente.Entities;
using Core8.DetalleOferente.Repository;

namespace Core8.DetalleOferente.Services
{
    /// <summary>
    /// Core8 - Servicio de detalle de oferente. Devuelve toda la información
    /// registrada de un oferente a partir de su identificación.
    /// </summary>
    public class DetalleOferenteService : IDetalleOferenteService
    {
        private readonly OferenteRepository _oferentes;
        private readonly IBitacoraRepository _bitacora;

        public DetalleOferenteService(OferenteRepository oferentes, IBitacoraRepository bitacora)
        {
            _oferentes = oferentes;
            _bitacora = bitacora;
        }

        public async Task<OferenteDetalle?> ObtenerDetalleAsync(string identificacion)
        {
            var ident = (identificacion ?? string.Empty).Trim();

            var detalle = await _oferentes.ObtenerDetallePorIdentificacionAsync(ident);
            if (detalle is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "oferente",
                    $"Consulta de detalle de oferente: no se encontró oferente con identificación '{ident}'.");
                return null;
            }

            await _bitacora.RegistrarAsync("SELECT", "oferente",
                $"Se consulta el detalle del oferente con identificación '{ident}'.");

            return detalle;
        }
    }
}
