using Core7.ListadoOferentes.Entities;

namespace Core7.ListadoOferentes.Services
{
    public interface IListadoOferentesService
    {
        Task<ListadoOferentesPuesto?> ObtenerListadoPorPuestoAsync(string codigoPuesto);
    }
}
