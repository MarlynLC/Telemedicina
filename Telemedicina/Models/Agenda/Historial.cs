namespace Telemedicina.Models.Agenda
{
    public class Historial
    {
        public DateTime fecha { get; set; }
        public string? diagnostico { get; set; }
        public List<Models.Receta.RecetaH>? Receta { get; set; }
    }
}
