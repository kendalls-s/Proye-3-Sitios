namespace Core1.ListadoPuestos.Entities
{
    /// <summary>
    /// Core1 - Un puesto disponible para postulación. El criterio de aceptación
    /// solo exige código y nombre.
    /// </summary>
    public class PuestoActivo
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
    }
}
