namespace Core7.ListadoOferentes.Entities
{
    /// <summary>
    /// Core7 - Candidato mostrado en el listado de oferentes que cumplen los
    /// requisitos de un puesto. La pantalla usa el nombre como enlace hacia el
    /// detalle del oferente (Core8/Core9), por eso se incluye la identificación.
    /// </summary>
    public class OferenteCandidato
    {
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
    }

    /// <summary>
    /// Core7 - "Yo como usuario del sistema quiero una pantalla de listado de
    /// oferentes para seleccionar el que será el nuevo empleado".
    /// Es el read-model que necesita esa pantalla: el contexto del puesto más la
    /// lista de candidatos aptos.
    /// </summary>
    public class ListadoOferentesPuesto
    {
        public string CodigoPuesto { get; set; } = null!;
        public string NombrePuesto { get; set; } = null!;
        public List<OferenteCandidato> Oferentes { get; set; } = new();
    }

    /// <summary>Proyección interna del puesto (código + nombre).</summary>
    public class PuestoInfo
    {
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
    }
}
