using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comunidad_Decidida.Models
{
    public class Asociado
    {
        public int IDAsociado { get; set; }
        public int IDSAE { get; set; }
        public string Nombre { get; set; }

        // Propiedades de navegación
        public ICollection<Tag> Tags { get; set; }
        public ICollection<Direccion> Direcciones { get; set; }
    }
}
