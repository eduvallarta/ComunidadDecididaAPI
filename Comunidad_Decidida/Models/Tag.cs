namespace Comunidad_Decidida.Models
{
    public class Tag
    {
        public int IDTags { get; set; }
        public int IDSAE { get; set; }
        public string Identificador { get; set; }
        public string Etiqueta { get; set; }
        public int Activa { get; set; }
        public string CancelacionWA { get; set; }
        public string DocCancelacion { get; set; }

        // Relaciones
        public Asociado Asociado { get; set; }
    }
}
