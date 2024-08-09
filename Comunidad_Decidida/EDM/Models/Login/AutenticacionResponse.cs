namespace Comunidad_Decidida.EDM.Models.Login
{
    /// <summary>
    /// Representa la respuesta de una solicitud de autenticación.
    /// </summary>
    public class AutenticacionResponse
    {
        /// <summary>
        /// Indica si la autenticación fue exitosa.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Razón del resultado de la autenticación, especialmente útil en caso de fallo.
        /// </summary>
        public string Razon { get; set; }

        /// <summary>
        /// Detalles de la excepción ocurrida durante el proceso de autenticación, si la hay.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Token de autenticación generado si la autenticación es exitosa.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Marca de tiempo asociada con la respuesta.
        /// </summary>
        public int TimeStamp { get; set; }

    }
}
