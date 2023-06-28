using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicina.Models.PacienteFormularios
{
    public class agregarPacienteViewModel : AbstractValidator<agregarPacienteViewModel>
    {
        //Declaramos la reglas a seguir sobre los datos
        public agregarPacienteViewModel()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Nacimiento).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Curp).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.Curp).MaximumLength(18).WithMessage("Formato No valido");
            RuleFor(x => x.Curp).Matches("^[A-Z]{4}[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])[HM]{1}(AS|BC|BS|CC|CS|CH|CL|CM|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)[B-DF-HJ-NP-TV-Z]{3}[0-9A-Z]{2}").WithMessage("Formato No valido");

            RuleFor(x => x.CodigoPostal).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.CodigoPostal).Matches("^[0-9]{5}").WithMessage("Formato No valido");
            RuleFor(x => x.CodigoPostal).MaximumLength(5).WithMessage("Formato No valido");

            RuleFor(x => x.Domicilio).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Localidad).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Telefono).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.Telefono).MaximumLength(18).WithMessage("Formato No valido");
            RuleFor(x => x.Telefono).Matches("^[0-9]{10}").WithMessage("Formato No valido");

            RuleFor(x => x.Correo).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Familiares).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Hereditarios).NotEmpty().WithMessage("Incompleto");

        }
        public List<Telemedicina.Models.Dominio.Localidad>? Localidades { get; set; }

        //Modelo de datos para el agregar de un paciente, para asi separarlo y tener modelos.
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Sexo { get; set; }
        public string? Sangre { get; set; }
        public DateTime Nacimiento { get; set; }
        public string? Curp { get; set; }
        public string? Domicilio { get; set; }
        public string? Localidad { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Escolaridad { get; set; }
        public string? Familiares { get; set; }
        public string? Ocupacion { get; set; }
        public string? Hereditarios { get; set; }
        public string? Imagen { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }
    }
}
