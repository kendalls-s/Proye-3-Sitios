using Auth.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(
            Usuario usuario,
            List<string> roles)
        {
            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Key.");

            string issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Issuer.");

            string audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "No se encontró Jwt:Audience.");

            int expirationMinutes =
                int.TryParse(
                    _configuration["Jwt:ExpirationMinutes"],
                    out int minutes)
                ? minutes
                : 60;

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.NombreUsuario),

                new Claim(
                    "nombreCompleto",
                    usuario.NombreCompleto),

                new Claim(
                    ClaimTypes.Email,
                    usuario.Correo)
            };

            foreach (var rol in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        rol));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow
                    .AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}