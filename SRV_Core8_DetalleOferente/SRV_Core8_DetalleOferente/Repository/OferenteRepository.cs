using Dapper;
using Core8.DetalleOferente.Entities;

namespace Core8.DetalleOferente.Repository
{
    public class OferenteRepository
    {
        private readonly IDbConnectionFactory _db;

        public OferenteRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        /// <summary>
        /// Retorna todos los datos registrados del oferente cuya identificación
        /// coincide exactamente, o null si no existe.
        /// </summary>
        public async Task<OferenteDetalle?> ObtenerDetallePorIdentificacionAsync(string identificacion)
        {
            const string sqlOferente = @"
SELECT o.id_oferente        AS IdOferente,
       o.identificacion     AS Identificacion,
       o.tipo_identificacion AS TipoIdentificacion,
       o.nombre_completo    AS NombreCompleto,
       o.fecha_nacimiento   AS FechaNacimiento,
       o.direccion          AS Direccion,
       o.fecha_registro     AS FechaRegistro,
       d.nombre             AS NombreDistrito,
       c.nombre             AS NombreCanton,
       p.nombre             AS NombreProvincia
FROM oferente o
LEFT JOIN distrito d  ON d.id_distrito = o.id_distrito
LEFT JOIN canton c    ON c.id_canton = d.id_canton
LEFT JOIN provincia p ON p.id_provincia = c.id_provincia
WHERE o.identificacion = @Identificacion;";

            const string sqlCorreos = "SELECT correo FROM oferente_correo WHERE id_oferente = @IdOferente;";
            const string sqlTelefonos = "SELECT telefono FROM oferente_telefono WHERE id_oferente = @IdOferente;";

            const string sqlPreparacion = @"
SELECT ie.nombre       AS Institucion,
       pa.titulo       AS Titulo,
       pa.fecha_inicio AS FechaInicio,
       pa.fecha_fin    AS FechaFin
FROM preparacion_academica pa
JOIN institucion_educativa ie ON ie.id_institucion = pa.id_institucion
WHERE pa.id_oferente = @IdOferente;";

            const string sqlExperiencia = @"
SELECT empresa AS Empresa, puesto AS Puesto, fecha_inicio AS FechaInicio, fecha_fin AS FechaFin
FROM experiencia_laboral
WHERE id_oferente = @IdOferente;";

            const string sqlCurriculums = @"
SELECT nombre_archivo AS NombreArchivo,
       ruta_archivo   AS RutaArchivo,
       tipo_archivo   AS TipoArchivo,
       tamano_bytes   AS TamanoBytes,
       fecha_carga    AS FechaCarga
FROM curriculum_oferente
WHERE id_oferente = @IdOferente;";

            const string sqlPostulaciones = @"
SELECT po.id_puesto       AS IdPuesto,
       pu.codigo           AS CodigoPuesto,
       pu.nombre           AS NombrePuesto,
       po.fecha_postulacion AS FechaPostulacion,
       po.estado           AS Estado,
       po.observacion      AS Observacion
FROM postulacion po
JOIN puesto pu ON pu.id_puesto = po.id_puesto
WHERE po.id_oferente = @IdOferente;";

            using var conn = _db.CreateConnection();

            var detalle = await conn.QueryFirstOrDefaultAsync<OferenteDetalle>(
                sqlOferente, new { Identificacion = identificacion });

            if (detalle is null)
                return null;

            var parametros = new { detalle.IdOferente };

            detalle.Correos = (await conn.QueryAsync<string>(sqlCorreos, parametros)).ToList();
            detalle.Telefonos = (await conn.QueryAsync<string>(sqlTelefonos, parametros)).ToList();
            detalle.PreparacionAcademica = (await conn.QueryAsync<PreparacionAcademicaOferente>(sqlPreparacion, parametros)).ToList();
            detalle.ExperienciaLaboral = (await conn.QueryAsync<ExperienciaLaboralOferente>(sqlExperiencia, parametros)).ToList();
            detalle.Curriculums = (await conn.QueryAsync<CurriculumOferente>(sqlCurriculums, parametros)).ToList();
            detalle.Postulaciones = (await conn.QueryAsync<PostulacionOferente>(sqlPostulaciones, parametros)).ToList();

            return detalle;
        }
    }
}
