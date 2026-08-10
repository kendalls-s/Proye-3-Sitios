using Dapper;
using Core7.ListadoOferentes.Entities;

namespace Core7.ListadoOferentes.Repository
{
    public class OferenteRepository
    {
        private readonly IDbConnectionFactory _db;

        public OferenteRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>Obtiene código + nombre del puesto, o null si no existe.</summary>
        public async Task<PuestoInfo?> ObtenerPuestoAsync(string codigoPuesto)
        {
            const string sql = "SELECT codigo AS Codigo, nombre AS Nombre FROM puesto WHERE codigo = @Codigo LIMIT 1;";
            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<PuestoInfo>(sql, new { Codigo = codigoPuesto });
        }

        /// <summary>
        /// Candidatos que cumplen el 100% de los requisitos del puesto,
        /// tomados de vw_oferentes_aptos_puesto.
        /// </summary>
        public async Task<IEnumerable<OferenteCandidato>> ObtenerCandidatosPorPuestoAsync(string codigoPuesto)
        {
            const string sql = @"
SELECT id_oferente     AS IdOferente,
       identificacion  AS Identificacion,
       nombre_completo AS NombreCompleto
FROM vw_oferentes_aptos_puesto
WHERE codigo_puesto = @Codigo
ORDER BY nombre_completo;";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<OferenteCandidato>(sql, new { Codigo = codigoPuesto });
        }
    }
}
