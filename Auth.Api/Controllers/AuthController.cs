using Auth.Api.DTOs;
using Auth.Api.Responses;
using Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var resultado =
                await _authService.LoginAsync(request);

            var respuesta =
                new ApiResponse<LoginResponse?>
                {
                    Success = resultado.Exito,
                    StatusCode = resultado.StatusCode,
                    Message = resultado.Mensaje,
                    Data = resultado.Data
                };

            return StatusCode(
                resultado.StatusCode,
                respuesta
            );
        }
    }
}