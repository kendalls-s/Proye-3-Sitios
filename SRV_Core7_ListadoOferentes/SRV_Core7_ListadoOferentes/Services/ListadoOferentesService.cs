using Core7.ListadoOferentes.Entities;
using Core7.ListadoOferentes.Repository;

namespace Core7.ListadoOferentes.Services
{
    /// <summary>
    /// Core7 - Arma el listado de oferentes aptos (candidatos) para un puesto,
    /// junto con el contexto del puesto, tal como lo requiere la pantalla de
    /// selección del nuevo empleado.
    /// </summary>
    public class ListadoOferentesService : IListadoOferentesService
    {
        private readonly OferenteRepository _oferentes;
        private readonly IBitacoraRepository _bitacora;

        public ListadoOferentesService(OferenteRepository oferentes, IBitacoraRepository bitacora)
        {
            _oferentes = oferentes;
            _bitacora = bitacora;
        }

        public async Task<ListadoOferentesPuesto?> ObtenerListadoPorPuestoAsync(string codigoPuesto)
        {
            var codigo = (codigoPuesto ?? string.Empty).Trim();

            var puesto = await _oferentes.ObtenerPuestoAsync(codigo);
            if (puesto is null)
            {
                await _bitacora.RegistrarAsync("ERROR", "oferente",
                    $"Listado de oferentes rechazado: el puesto '{codigo}' no existe.");
                return null;
            }

            var candidatos = (await _oferentes.ObtenerCandidatosPorPuestoAsync(codigo)).ToList();

            await _bitacora.RegistrarAsync("SELECT", "oferente",
                $"Se consulta el listado de oferentes para el puesto '{codigo}'.");

            return new ListadoOferentesPuesto
            {
                CodigoPuesto = puesto.Codigo,
                NombrePuesto = puesto.Nombre,
                Oferentes = candidatos
            };
        }
    }
}
