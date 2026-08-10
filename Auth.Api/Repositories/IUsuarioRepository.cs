using Auth.Api.Models;

namespace Auth.Api.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorUsuarioAsync(string nombreUsuario);

        Task ActualizarIntentosAsync(int idUsuario, int intentos);

        Task BloquearAsync(int idUsuario, int intentos);

        Task RegistrarLoginExitosoAsync(int idUsuario);

        Task<List<string>> ObtenerRolesAsync(int idUsuario);
    }
}