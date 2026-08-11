namespace Core3.CreacionEmpleados.Entities
{
    /// <summary>
    /// Información devuelta por Core3 después de crear el empleado.
    /// </summary>
    public class EmpleadoCreado
    {
        public int IdEmpleado { get; set; }
        public string NumeroEmpleado { get; set; } = null!;
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public int IdPuesto { get; set; }
        public string CodigoPuesto { get; set; } = null!;
        public string NombrePuesto { get; set; } = null!;
        public DateTime FechaIngreso { get; set; }
        public int IdAccionPersonal { get; set; }
        public string TipoAccion { get; set; } = null!;
    }

    /// <summary>
    /// Body de POST /empleados.
    /// Contiene toda la información necesaria para insertar el registro de empleado
    /// que no es generada automáticamente por la base de datos/servicio.
    /// </summary>
    public class CrearEmpleadoRequest
    {
        public int IdOferente { get; set; }
        public int IdPuesto { get; set; }
        public DateTime FechaIngreso { get; set; }
    }

    public class OferenteBasico
    {
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
    }

    public class PuestoBasico
    {
        public int IdPuesto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public bool Disponible { get; set; }
    }
}
