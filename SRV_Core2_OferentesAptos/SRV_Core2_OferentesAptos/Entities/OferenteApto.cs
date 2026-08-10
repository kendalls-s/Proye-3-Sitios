namespace Core2.OferentesAptos.Entities
{
    /// <summary>
    /// Core2 - Un oferente que cumple el 100% de los requisitos de un puesto.
    /// El criterio de aceptación solo exige nombre e identificación; se incluye
    /// IdOferente porque la pantalla que consume este servicio (Core7) lo usa
    /// para armar el enlace hacia el detalle del oferente (Core8/Core9).
    /// </summary>
    public class OferenteApto
    {
        public int IdOferente { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
    }
}
