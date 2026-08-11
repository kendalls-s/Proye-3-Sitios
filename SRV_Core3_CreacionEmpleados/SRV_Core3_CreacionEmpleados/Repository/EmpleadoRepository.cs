using Dapper;
using MySqlConnector;
using Core3.CreacionEmpleados.Entities;

namespace Core3.CreacionEmpleados.Repository
{
    public class CrearEmpleadoResultado
    {
        public bool YaEsEmpleado { get; set; }
        public EmpleadoCreado? Empleado { get; set; }
    }

    public class EmpleadoRepository
    {
        private readonly IDbConnectionFactory _db;

        private const string TipoAccionContratacion = "CONTRATACION";
        private const string PrefijoNumeroEmpleado = "EMP-";
        private const int LongitudCorrelativo = 4;

        public EmpleadoRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<OferenteBasico?> ObtenerOferenteAsync(int idOferente)
        {
            const string sql = @"
SELECT id_oferente AS IdOferente,
       identificacion AS Identificacion,
       nombre_completo AS NombreCompleto
FROM oferente
WHERE id_oferente = @IdOferente
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<OferenteBasico>(sql, new { IdOferente = idOferente });
        }

        public async Task<PuestoBasico?> ObtenerPuestoAsync(int idPuesto)
        {
            const string sql = @"
SELECT id_puesto AS IdPuesto,
       codigo AS Codigo,
       nombre AS Nombre,
       disponible AS Disponible
FROM puesto
WHERE id_puesto = @IdPuesto
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PuestoBasico>(sql, new { IdPuesto = idPuesto });
        }

        public async Task<bool> OferenteYaEsEmpleadoAsync(int idOferente)
        {
            const string sql = "SELECT 1 FROM empleado WHERE id_oferente = @IdOferente LIMIT 1;";
            using var conn = _db.CreateConnection();
            var existe = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { IdOferente = idOferente });
            return existe.HasValue;
        }

        public async Task<OferenteBasico?> ObtenerOferentePorIdentificacionAsync(string identificacion)
        {
            const string sql = @"
SELECT id_oferente AS IdOferente,
       identificacion AS Identificacion,
       nombre_completo AS NombreCompleto
FROM oferente
WHERE identificacion = @Identificacion
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<OferenteBasico>(sql, new { Identificacion = identificacion });
        }

        public async Task<PuestoBasico?> ObtenerPuestoPorCodigoAsync(string codigoPuesto)
        {
            const string sql = @"
SELECT id_puesto AS IdPuesto,
       codigo AS Codigo,
       nombre AS Nombre,
       disponible AS Disponible
FROM puesto
WHERE codigo = @CodigoPuesto
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PuestoBasico>(sql, new { CodigoPuesto = codigoPuesto });
        }

        /// <summary>
        /// Inserta empleado + acción de personal en una única transacción.
        /// SELECT ... FOR UPDATE sobre empleado serializa la generación del número
        /// y la comprobación de duplicados entre solicitudes concurrentes.
        /// No modifica el esquema de la base de datos.
        /// </summary>
        public async Task<CrearEmpleadoResultado> CrearEmpleadoConBloqueoAsync(
            OferenteBasico oferente,
            PuestoBasico puesto,
            DateTime fechaIngreso)
        {
            using var conn = (MySqlConnection)_db.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                // La tabla empleado de la BD es InnoDB. El bloqueo mantiene la
                // lectura y la siguiente inserción serializadas.
                const string sqlBloqueo = @"
SELECT id_empleado
FROM empleado
ORDER BY id_empleado
FOR UPDATE;";

                await conn.QueryAsync<int>(sqlBloqueo, transaction: tx);

                const string sqlYaEsEmpleado = @"
SELECT 1
FROM empleado
WHERE id_oferente = @IdOferente
LIMIT 1;";

                var yaEsEmpleado = await conn.QueryFirstOrDefaultAsync<int?>(
                    sqlYaEsEmpleado,
                    new { IdOferente = oferente.IdOferente },
                    tx);

                if (yaEsEmpleado.HasValue)
                {
                    await tx.RollbackAsync();
                    return new CrearEmpleadoResultado { YaEsEmpleado = true };
                }

                const string sqlUltimoNumero = @"
SELECT numero_empleado
FROM empleado
ORDER BY id_empleado DESC
LIMIT 1;";

                var ultimoNumero = await conn.QueryFirstOrDefaultAsync<string?>(
                    sqlUltimoNumero,
                    transaction: tx);

                var numeroEmpleado = GenerarSiguienteNumero(ultimoNumero);

                const string sqlInsertEmpleado = @"
INSERT INTO empleado
    (numero_empleado, id_oferente, id_puesto, fecha_ingreso)
VALUES
    (@NumeroEmpleado, @IdOferente, @IdPuesto, @FechaIngreso);
SELECT LAST_INSERT_ID();";

                var idEmpleado = await conn.ExecuteScalarAsync<int>(
                    sqlInsertEmpleado,
                    new
                    {
                        NumeroEmpleado = numeroEmpleado,
                        IdOferente = oferente.IdOferente,
                        IdPuesto = puesto.IdPuesto,
                        FechaIngreso = fechaIngreso.Date
                    },
                    tx);

                // La BD exige id_aprobador NOT NULL y FK hacia empleado.
                // MAIN ya contiene empleados existentes; usamos el primero como
                // aprobador sin alterar la estructura de la BD.
                const string sqlAprobador = "SELECT MIN(id_empleado) FROM empleado;";
                var idAprobador = await conn.ExecuteScalarAsync<int?>(sqlAprobador, transaction: tx);

                if (!idAprobador.HasValue)
                    throw new InvalidOperationException("No existe un empleado disponible para aprobar la acción de contratación.");

                const string sqlInsertAccion = @"
INSERT INTO accion_personal
    (tipo_accion, fecha_accion, descripcion, id_empleado, id_aprobador)
VALUES
    (@TipoAccion, @FechaAccion, @Descripcion, @IdEmpleado, @IdAprobador);
SELECT LAST_INSERT_ID();";

                var idAccionPersonal = await conn.ExecuteScalarAsync<int>(
                    sqlInsertAccion,
                    new
                    {
                        TipoAccion = TipoAccionContratacion,
                        FechaAccion = fechaIngreso.Date,
                        Descripcion = $"Contratación como {puesto.Nombre}.",
                        IdEmpleado = idEmpleado,
                        IdAprobador = idAprobador.Value
                    },
                    tx);

                await tx.CommitAsync();

                return new CrearEmpleadoResultado
                {
                    YaEsEmpleado = false,
                    Empleado = new EmpleadoCreado
                    {
                        IdEmpleado = idEmpleado,
                        NumeroEmpleado = numeroEmpleado,
                        IdOferente = oferente.IdOferente,
                        Identificacion = oferente.Identificacion,
                        NombreCompleto = oferente.NombreCompleto,
                        IdPuesto = puesto.IdPuesto,
                        CodigoPuesto = puesto.Codigo,
                        NombrePuesto = puesto.Nombre,
                        FechaIngreso = fechaIngreso.Date,
                        IdAccionPersonal = idAccionPersonal,
                        TipoAccion = TipoAccionContratacion
                    }
                };
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static string GenerarSiguienteNumero(string? ultimoNumero)
        {
            var siguiente = 1;

            if (!string.IsNullOrWhiteSpace(ultimoNumero))
            {
                var soloDigitos = new string(ultimoNumero.Where(char.IsDigit).ToArray());
                if (int.TryParse(soloDigitos, out var actual))
                    siguiente = actual + 1;
            }

            return PrefijoNumeroEmpleado + siguiente.ToString().PadLeft(LongitudCorrelativo, '0');
        }
    }
}
