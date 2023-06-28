namespace Telemedicina.Models.Dominio
{
    public class CitaFinal
    {
        public int Id { get; set; }
        public int AsistenteRemotoId { get; set; }
        public int AsistenteLocalId { get; set; }
        public int PacienteId { get; set; }
        public int DoctorId { get; set; }
        public int ComplejoId { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Hora { get; set; }
        public string? Comentarios { get; set; }
        public string? Estado { get; set; }
        public string? roomid { get; set; }
    }
}
