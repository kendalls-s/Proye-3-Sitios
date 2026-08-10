using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class TokenPruebaController : ControllerBase
    {
        [Authorize]
        [HttpGet("protegido")]
        public IActionResult Protegido()
        {
            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = User.FindFirstValue(ClaimTypes.Name);
            var correo = User.FindFirstValue(ClaimTypes.Email);

            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Token válido.",
                data = new
                {
                    idUsuario,
                    usuario,
                    correo
                }
            });
        }
    }
}