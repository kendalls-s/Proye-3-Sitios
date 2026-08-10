using Core2.OferentesAptos.Entities;

namespace Core2.OferentesAptos.Services
{
    public interface IOferentesAptosService
    {
        Task<(bool puestoExiste, IEnumerable<OferenteApto> oferentes)> ObtenerAptosPorPuestoAsync(
            string codigoPuesto);
    }
}
