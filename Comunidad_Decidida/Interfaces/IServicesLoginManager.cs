using Comunidad_Decidida.EDM.Models.Login;

namespace Comunidad_Decidida.Interfaces
{
    /// <summary>
    /// Interfaz para la gestión de la autenticación y acciones relacionadas con los usuarios.
    /// </summary>
    public interface IServicesLoginManager
    {
        /// <summary>
        /// Realiza la autenticación basada en las credenciales proporcionadas.
        /// </summary>
        /// <param name="login">Datos de inicio de sesión del usuario.</param>
        /// <returns>Verdadero si la autenticación es exitosa, de lo contrario, falso.</returns>
        Task<bool> Authentication(LoginRequest login);

        /// <summary>
        /// Procesa el inicio de sesión y realiza acciones adicionales específicas de la aplicación.
        /// </summary>
        /// <param name="usuario">Información del usuario para el inicio de sesión.</param>
        /// <returns>Respuesta con los detalles de la autenticación y datos específicos de la aplicación.</returns>
        Task<AutenticacionResponse> LoginAndApp(UsuarioAppRequest usuario);
    }
}
