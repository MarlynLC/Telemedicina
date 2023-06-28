namespace Telemedicina.Models.Dominio
{
    public class MDoctor
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string? TipoModificacion { get; set; }
        public int UsuarioId { get; set; }
        public string? UsuarioTipo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
