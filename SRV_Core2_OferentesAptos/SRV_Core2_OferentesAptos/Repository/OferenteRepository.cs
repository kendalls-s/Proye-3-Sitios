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
        /// A partir de la vista vw_oferentes_aptos_puesto retorna los oferentes que
        /// se postularon al puesto y cumplen el 100% de sus requisitos.
        /// </summary>
        public async Task<IEnumerable<OferenteApto>> ObtenerAptosPorPuestoAsync(string codigoPuesto)
        {
            const string sql = @"
SELECT id_oferente     AS IdOferente,
       identificacion  AS Identificacion,
       nombre_completo AS NombreCompleto
FROM vw_oferentes_aptos_puesto
WHERE codigo_puesto = @Codigo
ORDER BY nombre_completo;";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<OferenteApto>(sql, new { Codigo = codigoPuesto });
        }
    }
}
