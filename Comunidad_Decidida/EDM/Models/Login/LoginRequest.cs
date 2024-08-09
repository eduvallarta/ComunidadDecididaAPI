namespace Comunidad_Decidida.EDM.Models.Login
{
    /// <summary>
    /// Representa una solicitud de inicio de sesión con credenciales de usuario.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Nombre de usuario para la autenticación.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Contraseña del usuario para la autenticación.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Tipo de aplicativo que solicita la autenticación.
        /// </summary>
        /// <remarks>
        /// Este campo puede ser utilizado para diferenciar entre distintos sistemas o aplicaciones que utilizan el mismo servicio de autenticación.
        /// </remarks>
        public int TipoAplicativo { get; set; }
    }
}
