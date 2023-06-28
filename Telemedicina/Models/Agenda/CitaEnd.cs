namespace Telemedicina.Models.Agenda
{
    public class CitaEnd
    {
        public string? roomId { get; set; }
        public string? PacienteName { get; set;}
        public string? PaciEdad { get; set; }
        public string? PacienteSex { get; set;}
        public string? PaciOcupa { get; set; }
        public string? PaciHere { get; set; }
        public string? MotivoCita { get; set; }
        public List<Historial>? Historial { get; set; }
    }
}
