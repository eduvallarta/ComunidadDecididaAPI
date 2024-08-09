using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comunidad_Decidida.EDM.Models.Login
{
    /// <summary>
    /// Clase que contiene los datos de la autenticación.
    /// </summary>
    /// <created>Eduardo Alfonso Vallarta Zarate</created>
    public class UsuarioAppRequest
    {
        /// <summary>
        /// Usuario
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Contraseña
        /// </summary>
        public string Password { get; set; }

    }
}
