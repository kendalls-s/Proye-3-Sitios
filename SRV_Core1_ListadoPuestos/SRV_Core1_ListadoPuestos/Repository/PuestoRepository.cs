using Dapper;
using Core1.ListadoPuestos.Entities;

namespace Core1.ListadoPuestos.Repository
{
    public class PuestoRepository
    {
        private readonly IDbConnectionFactory _db;

        public PuestoRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>
        /// Retorna código y nombre de todos los puestos disponibles (activos).
        /// </summary>
        public async Task<IEnumerable<PuestoActivo>> ObtenerActivosAsync()
        {
            const string sql = @"
SELECT codigo AS Codigo,
       nombre AS Nombre
FROM puesto
WHERE disponible = 1
ORDER BY nombre;";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<PuestoActivo>(sql);
        }
    }
}
