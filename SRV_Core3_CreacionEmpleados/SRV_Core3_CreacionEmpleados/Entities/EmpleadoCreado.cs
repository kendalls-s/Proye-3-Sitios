namespace Core3.CreacionEmpleados.Entities
{
    /// <summary>
    /// Core3 - "Yo como administrador del sistema quiero un servicio que registre
    /// un nuevo empleado a partir de un oferente existente para generar su número
    /// de empleado y la acción de personal de contratación".
    /// Payload devuelto tras crear el empleado.
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
        public DateTime FechaContratacion { get; set; }
        public int IdAccionPersonal { get; set; }
        public string TipoAccion { get; set; } = null!;
    }

    /// <summary>Body esperado por POST /empleados.</summary>
    public class CrearEmpleadoRequest
    {
        public string Identificacion { get; set; } = null!;
        public string CodigoPuesto { get; set; } = null!;
    }

    /// <summary>Datos mínimos del oferente que necesita el flujo de contratación.</summary>
    public class OferenteBasico
    {
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
    }

    /// <summary>Datos mínimos del puesto que necesita el flujo de contratación.</summary>
    public class PuestoBasico
    {
        public int IdPuesto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
    }
}
