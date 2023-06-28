using FluentValidation;

namespace Telemedicina.Models.Agenda
{
    public class CitaTemp : AbstractValidator<CitaTemp>
    {
        public CitaTemp() 
        {
            RuleFor(x => x.Comentarios).NotEmpty().WithMessage("Incompleto");
        }
        //Clase para manipular y luego poder hacer el registro de citas temporales
        public int Id { get; set; }
        public int AsistenteRemotoId { get; set; }
        public List<Telemedicina.Models.Dominio.Paciente>? Pacientes { get; set; }
        public int PacienteId { get; set; }
        public string? NombrePaciente { get; set; }
        public int ComplejoId { get; set; }
        public string? NombreComplejo { get; set; }
        public DateTime Fecha { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Estado { get; set; }
    }
}
