namespace Core8.DetalleOferente.Entities
{
    /// <summary>
    /// Core8 - "Yo como usuario del sistema quiero un servicio de detalle de
    /// oferente para obtener el detalle de la información de un oferente".
    /// Retorna toda la información registrada del oferente. Recibe como parámetro
    /// la identificación (la tabla oferente no tiene columna "codigo").
    /// </summary>
    public class OferenteDetalle
    {
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string TipoIdentificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? NombreDistrito { get; set; }
        public string? NombreCanton { get; set; }
        public string? NombreProvincia { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<string> Correos { get; set; } = new();
        public List<string> Telefonos { get; set; } = new();
        public List<PreparacionAcademicaOferente> PreparacionAcademica { get; set; } = new();
        public List<ExperienciaLaboralOferente> ExperienciaLaboral { get; set; } = new();
        public List<CurriculumOferente> Curriculums { get; set; } = new();
        public List<PostulacionOferente> Postulaciones { get; set; } = new();
    }

    public class PreparacionAcademicaOferente
    {
        public string Institucion { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class ExperienciaLaboralOferente
    {
        public string Empresa { get; set; } = null!;
        public string Puesto { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class CurriculumOferente
    {
        public string NombreArchivo { get; set; } = null!;
        public string RutaArchivo { get; set; } = null!;
        public string? TipoArchivo { get; set; }
        public int? TamanoBytes { get; set; }
        public DateTime FechaCarga { get; set; }
    }

    public class PostulacionOferente
    {
        public int IdPuesto { get; set; }
        public string CodigoPuesto { get; set; } = null!;
        public string NombrePuesto { get; set; } = null!;
        public DateTime FechaPostulacion { get; set; }
        public string Estado { get; set; } = null!;
        public string? Observacion { get; set; }
    }
}
