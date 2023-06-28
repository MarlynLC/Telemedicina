using Telemedicina.Models.Dominio;

namespace Telemedicina.Models.TablasClase
{
    public class MPacienteDatos
    {
        //public List<Paciente> Pacientes { get; set; }
        //public List<MPaciente> mPacientes { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public string? Usuario { get; set; }
        public string? UsuarioTipo { get; set; }
        public DateTime Fecha { get; set; }

    }
}
