using Dapper;

namespace Core2.OferentesAptos.Repository
{
    /// <summary>
    /// Manejo de bitácoras (HU "Manejo de bitácoras"). Cada consulta y cada error
    /// técnico se registra en la tabla `bitacora` de la misma base de datos, para
    /// que el microservicio sea totalmente independiente (no depende de un servicio
    /// externo de bitácora). Un fallo al escribir la bitácora nunca interrumpe el
    /// flujo principal.
    /// </summary>
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
