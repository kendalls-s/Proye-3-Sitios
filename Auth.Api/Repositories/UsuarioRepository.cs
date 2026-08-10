using Auth.Api.Models;
using Dapper;
using MySqlConnector;

namespace Auth.Api.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión.");
        }

        private MySqlConnection CrearConexion()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<Usuario?> ObtenerPorUsuarioAsync(
            string nombreUsuario)
        {
            const string sql = """
                SELECT
                    id_usuario AS IdUsuario,
                    usuario AS NombreUsuario,
                    nombre_completo AS NombreCompleto,
                    correo AS Correo,
                    password_hash AS PasswordHash,
                    COALESCE(estado, 'ACTIVO') AS Estado,
                    COALESCE(intentos_login, 0) AS IntentosLogin,
                    fecha_ultimo_login AS FechaUltimoLogin,
                    fecha_bloqueo AS FechaBloqueo
                FROM usuario
                WHERE usuario = @NombreUsuario
                LIMIT 1;
                """;

            await using var conexion = CrearConexion();

            return await conexion.QueryFirstOrDefaultAsync<Usuario>(
                sql,
                new
                {
                    NombreUsuario = nombreUsuario
                });
        }

        public async Task ActualizarIntentosAsync(
            int idUsuario,
            int intentos)
        {
            const string sql = """
                UPDATE usuario
                SET intentos_login = @Intentos
                WHERE id_usuario = @IdUsuario;
                """;

            await using var conexion = CrearConexion();

            await conexion.ExecuteAsync(sql, new
            {
                Intentos = intentos,
                IdUsuario = idUsuario
            });
        }

        public async Task BloquearAsync(
            int idUsuario,
            int intentos)
        {
            const string sql = """
                UPDATE usuario
                SET
                    intentos_login = @Intentos,
                    estado = 'BLOQUEADO',
                    fecha_bloqueo = @FechaBloqueo
                WHERE id_usuario = @IdUsuario;
                """;

            await using var conexion = CrearConexion();

            await conexion.ExecuteAsync(sql, new
            {
                Intentos = intentos,
                FechaBloqueo = DateTime.Now,
                IdUsuario = idUsuario
            });
        }

        public async Task RegistrarLoginExitosoAsync(
            int idUsuario)
        {
            const string sql = """
                UPDATE usuario
                SET
                    intentos_login = 0,
                    fecha_ultimo_login = @FechaUltimoLogin
                WHERE id_usuario = @IdUsuario;
                """;

            await using var conexion = CrearConexion();

            await conexion.ExecuteAsync(sql, new
            {
                FechaUltimoLogin = DateTime.Now,
                IdUsuario = idUsuario
            });
        }

        public async Task<List<string>> ObtenerRolesAsync(
            int idUsuario)
        {
            const string sql = """
                SELECT r.nombre
                FROM rol r
                INNER JOIN usuario_rol ur
                    ON ur.id_rol = r.id_rol
                WHERE ur.id_usuario = @IdUsuario;
                """;

            await using var conexion = CrearConexion();

            var roles = await conexion.QueryAsync<string>(
                sql,
                new
                {
                    IdUsuario = idUsuario
                });

            return roles.ToList();
        }
    }
}