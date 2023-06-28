namespace Telemedicina.Models.Localidad
{
    public class LocalAsistente
    {
        public int Id { get; set; }
        public int AsistenteLID { get; set; }
        public string? AsistenteNombre { get; set; }
        public int LocalidadID { get; set; }
        public string? LocalidadNombre { get; set; }
        public List<Telemedicina.Models.Dominio.AsistenteLocal>? AsistentesLocales { get; set; }
        public List<Telemedicina.Models.Dominio.Localidad>? Localidades { get; set; }
    }
}
