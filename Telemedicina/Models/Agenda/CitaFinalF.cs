using FluentValidation;

namespace Telemedicina.Models.Agenda
{
    public class CitaFinalF : AbstractValidator<CitaFinalF>
    {
        public CitaFinalF()
        {
            RuleFor(x => x.Comentarios).NotEmpty().WithMessage("Incompleto");
        }
        //Clase para manipular y luego poder hacer el registro de citas temporales
        public int Id { get; set; }
        public int AsistenteRemotoId { get; set; }
        public string? NombreAsistenteRemoto { get; set; }
        public int PacienteId { get; set; }
        public string? NombrePaciente { get; set; }
        public int ComplejoId { get; set; }
        public int DoctorId { get; set; }
        public string? NombreDoctor { get; set; }
        public string? NombreComplejo { get; set; }
        public string? Comentarios { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Hora { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Estado { get; set; }
        public List<Telemedicina.Models.Dominio.Doctor>? Doctores { get; set; }
        public List<Telemedicina.Models.Agenda.AgendaDoctor>? AgendaDoctores { get; set; }
        public string? roomid { get; set; }
    }
}
