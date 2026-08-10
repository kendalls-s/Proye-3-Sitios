namespace Auth.Api.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public int IntentosLogin { get; set; }

        public DateTime? FechaUltimoLogin { get; set; }

        public DateTime? FechaBloqueo { get; set; }
    }
}