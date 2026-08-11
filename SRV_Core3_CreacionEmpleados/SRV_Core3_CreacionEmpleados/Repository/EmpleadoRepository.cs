using Dapper;
using MySqlConnector;
using Core3.CreacionEmpleados.Entities;

namespace Core3.CreacionEmpleados.Repository
{
    /// <summary>
    /// Resultado interno de intentar crear el empleado dentro de la transacción
    /// bloqueada. "YaEsEmpleado" indica que, bajo el bloqueo, se detectó que el
    /// oferente ya tenía un empleado asociado (evita duplicados por carrera).
    /// </summary>
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
        private const int LongitudCorrelativo = 6;

        public EmpleadoRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>Datos básicos del oferente a partir de su identificación, o null si no existe.</summary>
        public async Task<OferenteBasico?> ObtenerOferentePorIdentificacionAsync(string identificacion)
        {
            const string sql = @"
SELECT id_oferente     AS IdOferente,
       identificacion  AS Identificacion,
       nombre_completo AS NombreCompleto
FROM oferente
WHERE identificacion = @Identificacion
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<OferenteBasico>(sql, new { Identificacion = identificacion });
        }

        /// <summary>Datos básicos del puesto a partir de su código, o null si no existe.</summary>
        public async Task<PuestoBasico?> ObtenerPuestoPorCodigoAsync(string codigoPuesto)
        {
            const string sql = @"
SELECT id_puesto AS IdPuesto,
       codigo    AS Codigo,
       nombre    AS Nombre
FROM puesto
WHERE codigo = @Codigo
LIMIT 1;";

            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PuestoBasico>(sql, new { Codigo = codigoPuesto });
        }

        /// <summary>
        /// Verificación rápida (sin bloqueo) de si el oferente ya es empleado.
        /// Se usa para responder con un 409 temprano; la verificación definitiva
        /// y segura ante condiciones de carrera ocurre dentro de la transacción
        /// bloqueada de <see cref="CrearEmpleadoConBloqueoAsync"/>.
        /// </summary>
        public async Task<bool> OferenteYaEsEmpleadoAsync(int idOferente)
        {
            const string sql = "SELECT 1 FROM empleado WHERE id_oferente = @IdOferente LIMIT 1;";
            using var conn = _db.CreateConnection();
            var existe = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { IdOferente = idOferente });
            return existe.HasValue;
        }

        /// <summary>
        /// Crea el empleado y su acción de personal de contratación dentro de una
        /// única transacción. Para evitar que dos solicitudes concurrentes generen
        /// el mismo número de empleado (o que un mismo oferente sea contratado dos
        /// veces por una condición de carrera), la transacción toma un bloqueo
        /// pesimista sobre la tabla "empleado" mediante "SELECT ... FOR UPDATE"
        /// antes de calcular el correlativo y antes de re-verificar la unicidad
        /// del oferente. Cualquier otra transacción que intente el mismo flujo
        /// queda en espera hasta que ésta confirme (COMMIT) o revierta (ROLLBACK).
        /// </summary>
        public async Task<CrearEmpleadoResultado> CrearEmpleadoConBloqueoAsync(
            OferenteBasico oferente, PuestoBasico puesto)
        {
            using var conn = (MySqlConnection)_db.CreateConnection();
            conn.Open();
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                // 1) Bloqueo pesimista sobre los registros existentes de empleado.
                //    La tabla es InnoDB y esta lectura FOR UPDATE mantiene los
                //    bloqueos durante toda la transacción, serializando este flujo
                //    frente a otra contratación concurrente. No se modifica el
                //    esquema de la base de datos.
                const string sqlBloqueo = "SELECT id_empleado FROM empleado ORDER BY id_empleado FOR UPDATE;";
                await conn.QueryAsync<int>(sqlBloqueo, transaction: tx);

                // 2) Re-verificación de unicidad ya bajo el bloqueo: un oferente
                //    no puede convertirse en empleado más de una vez.
                const string sqlYaEsEmpleado = "SELECT 1 FROM empleado WHERE id_oferente = @IdOferente LIMIT 1;";
                var yaEsEmpleado = await conn.QueryFirstOrDefaultAsync<int?>(
                    sqlYaEsEmpleado, new { oferente.IdOferente }, tx);

                if (yaEsEmpleado.HasValue)
                {
                    await tx.RollbackAsync();
                    return new CrearEmpleadoResultado { YaEsEmpleado = true, Empleado = null };
                }

                // 3) Generación del número de empleado a partir del último
                //    correlativo registrado (protegida por el bloqueo anterior).
                const string sqlUltimoNumero = @"
SELECT numero_empleado
FROM empleado
ORDER BY id_empleado DESC
LIMIT 1;";

                var ultimoNumero = await conn.QueryFirstOrDefaultAsync<string?>(sqlUltimoNumero, transaction: tx);
                var numeroEmpleado = GenerarSiguienteNumero(ultimoNumero);

                // 4) Inserción del empleado.
                const string sqlInsertEmpleado = @"
INSERT INTO empleado (numero_empleado, id_oferente, id_puesto, fecha_ingreso)
VALUES (@NumeroEmpleado, @IdOferente, @IdPuesto, @FechaContratacion);
SELECT LAST_INSERT_ID();";

                var fechaContratacion = DateTime.Now;
                var idEmpleado = await conn.ExecuteScalarAsync<int>(sqlInsertEmpleado, new
                {
                    NumeroEmpleado = numeroEmpleado,
                    oferente.IdOferente,
                    puesto.IdPuesto,
                    FechaContratacion = fechaContratacion
                }, tx);

                // 5) La estructura real de MAIN exige un aprobador existente
                //    (id_aprobador NOT NULL y FK hacia empleado). Utilizamos el
                //    primer empleado existente como aprobador del movimiento, sin
                //    crear ni alterar columnas/tablas.
                const string sqlAprobador = @"
SELECT MIN(id_empleado)
FROM empleado;";

                var idAprobador = await conn.ExecuteScalarAsync<int?>(sqlAprobador, transaction: tx);
                if (!idAprobador.HasValue)
                    throw new InvalidOperationException("No existe un empleado disponible para aprobar la acción de contratación.");

                // 6) Acción de personal de tipo contratación, respetando exactamente
                //    las columnas de la BD db_personal_sitios.
                const string sqlInsertAccion = @"
INSERT INTO accion_personal (tipo_accion, fecha_accion, descripcion, id_empleado, id_aprobador)
VALUES (@TipoAccion, @FechaAccion, @Descripcion, @IdEmpleado, @IdAprobador);
SELECT LAST_INSERT_ID();";

                var descripcionAccion =
                    $"Contratación del empleado {numeroEmpleado} para el puesto '{puesto.Codigo} - {puesto.Nombre}'.";

                var idAccionPersonal = await conn.ExecuteScalarAsync<int>(sqlInsertAccion, new
                {
                    IdEmpleado = idEmpleado,
                    TipoAccion = TipoAccionContratacion,
                    FechaAccion = fechaContratacion.Date,
                    Descripcion = descripcionAccion,
                    IdAprobador = idAprobador.Value
                }, tx);

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
                        FechaContratacion = fechaContratacion,
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

        /// <summary>
        /// Calcula el siguiente número de empleado a partir del último registrado,
        /// con el formato "EMP" + correlativo numérico con relleno de ceros
        /// (p. ej. EMP000001, EMP000002, ...).
        /// </summary>
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
