using Comunidad_Decidida.Interfaces;
using Comunidad_Decidida.Manager;
using Comunidad_Decidida.EDM.Models.Login;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlackList.Services
{
    /// <summary>
    /// Clase de servicio para la gestión del inicio de sesión y autenticación.
    /// </summary>
    public class ServicesLoginManager : IServicesLoginManager
    {
        #region " Members "

        private string token = string.Empty;
        private string urlBase;
        private HttpClient httpClient;
        private readonly IConfiguration _configuration;
        private readonly AutenticacionManager _autenticacionManager;
        #endregion

        #region " Constructor "

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ServicesLoginManager"/>.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="autenticacionManager">El gestor de autenticación.</param>

        public ServicesLoginManager(IConfiguration configuration, AutenticacionManager autenticacionManager)
        {
            _configuration = configuration;
            _autenticacionManager = autenticacionManager;
        }
        #endregion

        #region " Functions "

        /// <summary>
        /// Realiza la autenticación del usuario.
        /// </summary>
        /// <param name="login">Datos de inicio de sesión del usuario.</param>
        /// <returns>True si la autenticación es exitosa, de lo contrario false.</returns>
		// <created>Eduardo Vallarta Zarate</created>
        public async Task<bool> Authentication(LoginRequest login)
        {
            string token = await _autenticacionManager.Authentication(login).ConfigureAwait(false);
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Realiza el inicio de sesión y devuelve información adicional específica de la aplicación.
        /// </summary>
        /// <param name="usuario">Datos del usuario para el inicio de sesión.</param>
        /// <returns>Respuesta de autenticación con información del usuario y un token JWT.</returns>
        // <created>Eduardo Vallarta Zarate</created>
        public async Task<AutenticacionResponse> LoginAndApp(UsuarioAppRequest usuario)
        {
            var aux = await _autenticacionManager.LoginAndApp(usuario).ConfigureAwait(false);

            if (aux.Success)
                aux.Token = GenerateJwtToken(usuario.User);

            return aux;
        }

        /// <summary>
        /// Genera un token JWT para un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario para el cual se genera el token.</param>
        /// <returns>Token JWT generado.</returns>
        // <created>Eduardo Vallarta Zarate</created>
        private string GenerateJwtToken(string username)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];
                var expireTime = _configuration["Jwt:Expire_Minutes"];

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) });

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claimsIdentity,
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = signingCredentials
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                // Aquí deberías manejar el error, por ejemplo, registrándolo o devolviendo un mensaje específico.
                // Por ejemplo, podrías escribir el mensaje de error en el log:
                // LogError(ex, "Error generating JWT token");

                // Y luego podrías decidir si lanzar la excepción o manejarla de otra manera:
                throw new InvalidOperationException("An error occurred while generating JWT token", ex);

                // O podrías devolver null o un string vacío para indicar que la generación del token falló,
                // dependiendo de cómo quieras manejar esta situación en tu aplicación.
                // return null; // o return string.Empty;
            }
        }

    }
    #endregion

}
