using FluentValidation;

namespace Telemedicina.Models.Localidad
{
    public class Localidadm : AbstractValidator<Localidadm>
    {
        //Clase para la modificacion de Localidades
        public Localidadm()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("Incompleto");
        }

        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
    }
}
