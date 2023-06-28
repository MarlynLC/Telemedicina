using FluentValidation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telemedicina.Models.DoctorFormularios
{
    public class UpdateDoctorViewModel : AbstractValidator<UpdateDoctorViewModel>
    {
        //Declaramos la reglas a seguir sobre los datos
        public UpdateDoctorViewModel()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Nacimiento).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Curp).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.Curp).MaximumLength(18).WithMessage("Formato No valido");
            RuleFor(x => x.Curp).Matches("^[A-Z]{4}[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])[HM]{1}(AS|BC|BS|CC|CS|CH|CL|CM|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)[B-DF-HJ-NP-TV-Z]{3}[0-9A-Z]{2}").WithMessage("Formato No valido");

            RuleFor(x => x.Telefono).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.Telefono).MaximumLength(18).WithMessage("Formato No valido");
            RuleFor(x => x.Telefono).Matches("^[0-9]{10}").WithMessage("Formato No valido");

            RuleFor(x => x.Correo).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Institucion).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Matricula).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Especialidad).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Jerarquia).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Incompleto");
        }

        //Modelo de datos para la modificacion un Doctor, para asi separarlo y tener modelos.
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public string? Sexo { get; set; }
        public string? Sangre { get; set; }
        public DateTime Nacimiento { get; set; }
        public string? Curp { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Institucion { get; set; }
        public string? Matricula { get; set; }
        public string? Especialidad { get; set; }
        public string? Jerarquia { get; set; }
        public string? Password { get; set; }
        public string? Imagen { get; set; }
        public string? Firma { get; set; }
        public string? Visilbe { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }
        [NotMapped]
        public IFormFile? FileFirma { get; set; }
    }
}