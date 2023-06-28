namespace Telemedicina.Models.Complejo
{
    public class ComplejoUsuario
    {
        public int Id { get; set; }
        public String? UsuarioNombre { get; set; }
        public int DoctorId { get; set; }
        public int AsistenteRemotoId { get; set; }
        public String? ComplejoNombre { get; set; }
        public int ComplejoId { get; set; }
        public List<Telemedicina.Models.Dominio.Complejo>? Complejos { get; set; }
        public List<Telemedicina.Models.Dominio.Doctor>? Doctores { get; set; }
        public List<Telemedicina.Models.Dominio.AsistenteRemoto>? AsistenteRemotos { get; set; }
    }
}
