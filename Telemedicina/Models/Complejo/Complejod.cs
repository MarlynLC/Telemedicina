using FluentValidation;
using Telemedicina.Models.Dominio;
using Telemedicina.Models.Localidad;

namespace Telemedicina.Models.Complejo
{
    public class Complejod : AbstractValidator<Complejod>
    {
        public Complejod() 
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("Incompleto");
            RuleFor(x => x.Ubicacion).NotEmpty().WithMessage("Incompleto");
        }
        public List<Telemedicina.Models.Dominio.Localidad>? Localidades { get; set; }
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Ubicacion { get; set; }
        public int Localidadid { get; set; }
    }
}
