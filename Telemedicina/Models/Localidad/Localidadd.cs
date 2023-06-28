using FluentValidation;

namespace Telemedicina.Models.Localidad
{
    public class Localidadd : AbstractValidator<Localidadd>
    {
        //Clase para el agregar localidades
        public Localidadd() 
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("Incompleto");
        }

        public string? Nombre { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
    }
}
