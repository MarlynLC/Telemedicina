namespace Telemedicina.Models.Receta
{
    public class Recetashow
    {
        public string? DocNombre { get; set; }
        public string? DocRango { get; set; }
        public string? DocCelula { get; set; }
        public string? DocEscuela { get; set; }
        public string? DocFirma { get; set; }
        public DateTime fecha { get; set; }
        public string? PacNombre { get; set; }
        public string? PacEdad { get; set; }
        public string? ModuloNombre { get; set; }
        public string? ModuloUbicacion { get; set; }
        public string? Prescripcion { get; set; }
        public List<Models.Receta.RecetaItems>? Items { get; set; }
    }
}