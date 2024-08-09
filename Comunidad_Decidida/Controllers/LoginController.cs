using Comunidad_Decidida.Interfaces;
using Comunidad_Decidida.EDM.Models.Login;
using Microsoft.AspNetCore.Mvc;

namespace Comunidad_Decidida.Controllers
{
    /// <summary>
    /// Controlador para manejar las acciones de autenticación y gestión de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        // ServicesLoginManager es inyectado a través del constructor:
        private readonly IConfiguration _configuration;
        private readonly IServicesLoginManager _services;

        /// <summary>
        /// Constructor para inyectar dependencias en el controlador.
        /// </summary>
        /// <param name="configuration">Configuración del sistema.</param>
        /// <param name="services">Servicio para gestionar la autenticación y otros procesos relacionados con usuarios.</param>
        public LoginController(IConfiguration configuration, IServicesLoginManager services)
        {
            _configuration = configuration;
            _services = services;
        }

        /// <summary>
        /// Autentica a un usuario y devuelve los detalles relevantes si la autenticación es exitosa.
        /// </summary>
        /// <param name="user">Datos del usuario para autenticación.</param>
        /// <returns>Resultado de la autenticación.</returns>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="400">Datos de usuario no proporcionados.</response>
        [HttpPost("authenticate")]
        public async Task<IActionResult> LoginUser([FromBody] UsuarioAppRequest user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var result = await _services.LoginAndApp(user).ConfigureAwait(false);
            return Ok(result);
        }
    }

}