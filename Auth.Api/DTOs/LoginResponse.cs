namespace Auth.Api.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}