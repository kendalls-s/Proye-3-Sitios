using Dapper;

namespace Core7.ListadoOferentes.Repository
{
    public interface IBitacoraRepository
    {
        Task RegistrarAsync(
            string tipo,
            string entidad,
            string descripcion,
            int? idUsuario = null,
            string? datosAnteriores = null,
            string? datosNuevos = null);
    }

    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly IDbConnectionFactory _db;

        public BitacoraRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task RegistrarAsync(
            string tipo,
            string entidad,
            string descripcion,
            int? idUsuario = null,
            string? datosAnteriores = null,
            string? datosNuevos = null)
        {
            const string sql = @"
INSERT INTO bitacora (fecha, id_usuario, tipo, entidad, datos_anteriores, datos_nuevos, descripcion)
VALUES (@Fecha, @IdUsuario, @Tipo, @Entidad, @DatosAnteriores, @DatosNuevos, @Descripcion);";

            try
            {
                using var conn = _db.CreateConnection();
                await conn.ExecuteAsync(sql, new
                {
                    Fecha = DateTime.Now,
                    IdUsuario = idUsuario,
                    Tipo = tipo,
                    Entidad = entidad,
                    DatosAnteriores = datosAnteriores,
                    DatosNuevos = datosNuevos,
                    Descripcion = descripcion
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo registrar la bitácora ({entidad}): {ex.Message}");
            }
        }
    }
}
