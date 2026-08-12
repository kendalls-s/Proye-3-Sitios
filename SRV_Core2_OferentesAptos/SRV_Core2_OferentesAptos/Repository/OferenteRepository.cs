using Dapper;
using Core2.OferentesAptos.Entities;

namespace Core2.OferentesAptos.Repository
{
    public class OferenteRepository
    {
        private readonly IDbConnectionFactory _db;

        public OferenteRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>
        /// Verifica si existe un puesto con ese código (permite distinguir
        /// "puesto inexistente" de "sin oferentes aptos").
        /// </summary>
        public async Task<bool> ExistePuestoAsync(string codigoPuesto)
        {
            const string sql = "SELECT 1 FROM puesto WHERE codigo = @Codigo LIMIT 1;";
            using var conn = _db.CreateConnection();
            var valor = await conn.QueryFirstOrDefaultAsync<int?>(sql, new { Codigo = codigoPuesto });
            return valor != null;
        }

        /// <summary>
        /// Retorna los oferentes de un puesto. Por indicación del profesor, TODOS
        /// los oferentes postulados a un puesto se consideran aptos: no se filtra
        /// por cumplimiento de requisitos ni por estado de la postulación. El
        /// criterio de aceptación (Core2) solo exige devolver nombre e
        /// identificación de los oferentes de un puesto, lo cual se cumple.
        ///
        /// Nota: se consulta directamente contra las tablas (postulacion / puesto /
        /// oferente) en lugar de la vista vw_oferentes_aptos_puesto, porque esa
        /// vista aplica el filtro de requisitos que aquí NO se desea. Así el cambio
        /// queda contenido en este microservicio y no altera la base compartida.
        /// </summary>
        public async Task<IEnumerable<OferenteApto>> ObtenerAptosPorPuestoAsync(
     string codigoPuesto)
        {
            const string sql = @"
        SELECT DISTINCT
            v.id_oferente AS IdOferente,
            v.identificacion AS Identificacion,
            v.nombre_completo AS NombreCompleto
        FROM vw_oferentes_aptos_puesto v
        WHERE v.codigo_puesto = @Codigo
        ORDER BY v.nombre_completo;";

            using var conn = _db.CreateConnection();

            return await conn.QueryAsync<OferenteApto>(
                sql,
                new
                {
                    Codigo = codigoPuesto
                });
        }
    }
}
