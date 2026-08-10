using Core2.OferentesAptos.Entities;
using Core2.OferentesAptos.Repository;

namespace Core2.OferentesAptos.Services
{
    /// <summary>
    /// Core2 - "Yo como administrador del sistema quiero un servicio que a partir
    /// de un código de puesto regrese los oferentes que cumplen los requisitos de
    /// este para obtener la lista de oferentes".
    /// </summary>
    public class OferentesAptosService : IOferentesAptosService
    {
        private readonly OferenteRepository _oferentes;
        private readonly IBitacoraRepository _bitacora;

        public OferentesAptosService(OferenteRepository oferentes, IBitacoraRepository bitacora)
        {
            _oferentes = oferentes;
            _bitacora = bitacora;
        }

        public async Task<(bool puestoExiste, IEnumerable<OferenteApto> oferentes)> ObtenerAptosPorPuestoAsync(
            string codigoPuesto)
        {
            var codigo = (codigoPuesto ?? string.Empty).Trim();

            if (!await _oferentes.ExistePuestoAsync(codigo))
            {
                await _bitacora.RegistrarAsync("ERROR", "oferente",
                    $"Consulta de oferentes aptos rechazada: el puesto '{codigo}' no existe.");
                return (false, Enumerable.Empty<OferenteApto>());
            }

            var lista = (await _oferentes.ObtenerAptosPorPuestoAsync(codigo)).ToList();

            await _bitacora.RegistrarAsync("SELECT", "oferente",
                $"Se consultan los oferentes aptos para el puesto '{codigo}'.");

            return (true, lista);
        }
    }
}
