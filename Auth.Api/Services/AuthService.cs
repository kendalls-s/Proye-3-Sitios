using Auth.Api.DTOs;
using Auth.Api.Repositories;

namespace Auth.Api.Services
{
    public class AuthService
    {
        private const int MaxIntentos = 3;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly CriptografiaService _criptografiaService;
        private readonly JwtService _jwtService;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            CriptografiaService criptografiaService,
            JwtService jwtService)
        {
            _usuarioRepository = usuarioRepository;
            _criptografiaService = criptografiaService;
            _jwtService = jwtService;
        }

        public async Task<(bool Exito, int StatusCode, string Mensaje, LoginResponse? Data)>
            LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Usuario) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return (
                    false,
                    400,
                    "Usuario y/o contraseña incorrectos.",
                    null
                );
            }

            var usuario = await _usuarioRepository
                .ObtenerPorUsuarioAsync(request.Usuario.Trim());

            if (usuario == null)
            {
                return (
                    false,
                    401,
                    "Usuario y/o contraseña incorrectos.",
                    null
                );
            }

            if (usuario.Estado.Equals(
                    "BLOQUEADO",
                    StringComparison.OrdinalIgnoreCase))
            {
                return (
                    false,
                    403,
                    "El usuario se encuentra bloqueado.",
                    null
                );
            }

            if (usuario.Estado.Equals(
                    "INACTIVO",
                    StringComparison.OrdinalIgnoreCase))
            {
                return (
                    false,
                    403,
                    "El usuario se encuentra inactivo.",
                    null
                );
            }

            bool passwordCorrecto =
                _criptografiaService.Verificar(
                    request.Password,
                    usuario.PasswordHash
                );

            if (!passwordCorrecto)
            {
                int nuevosIntentos =
                    usuario.IntentosLogin + 1;

                if (nuevosIntentos >= MaxIntentos)
                {
                    await _usuarioRepository.BloquearAsync(
                        usuario.IdUsuario,
                        nuevosIntentos
                    );

                    return (
                        false,
                        403,
                        "El usuario ha sido bloqueado por 3 intentos fallidos.",
                        null
                    );
                }

                await _usuarioRepository.ActualizarIntentosAsync(
                    usuario.IdUsuario,
                    nuevosIntentos
                );

                return (
                    false,
                    401,
                    "Usuario y/o contraseña incorrectos.",
                    null
                );
            }

            await _usuarioRepository
                .RegistrarLoginExitosoAsync(usuario.IdUsuario);

            var roles =
                await _usuarioRepository
                    .ObtenerRolesAsync(usuario.IdUsuario);
                    string token =
    _jwtService.GenerarToken(
        usuario,
        roles
    );

            var response = new LoginResponse
            {
                Token = token,
                IdUsuario = usuario.IdUsuario,
                Usuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                Roles = roles
            };

            return (
                true,
                200,
                "Inicio de sesión exitoso.",
                response
            );
        }
    }
}