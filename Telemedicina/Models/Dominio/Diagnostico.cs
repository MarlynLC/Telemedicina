namespace Telemedicina.Models.Dominio
{
    public class Diagnostico
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public string? DiagnosticoTexto { get; set; }
        public DateTime fecha { get; set; }
        public string? temp { get; set; }
        public string? presion { get; set;}
        public string? peso { get; set;}
        public string? ritmo { get; set; }

    }
}
