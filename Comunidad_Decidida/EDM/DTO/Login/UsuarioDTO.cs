using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comunidad_Decidida.DTO.Login
{
    public class UsuarioDTO
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public int IdUser { get; set; }

        /// <summary>
        /// Nombre Completo usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Usuario
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// Identificador de Role
        /// </summary>
        public int IdRole { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Estatus
        /// </summary>
        public int Estatus { get; set; }

    }
}
