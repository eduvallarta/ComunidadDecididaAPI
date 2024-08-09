namespace Comunidad_Decidida.Models
{
    public class Direccion
    {
        public int IDDireccion { get; set; }
        public int IDSAE { get; set; }
        public string Calle { get; set; }
        public string NumInt { get; set; }
        public string NumExt { get; set; }

        // Propiedades de navegación
        public Asociado Asociado { get; set; }
    }
}
