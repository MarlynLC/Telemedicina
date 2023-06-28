using FluentValidation;

namespace Telemedicina.Models.LoginFormulario
{
    public class loginViewModel : AbstractValidator<loginViewModel>
    {
        //Declaramos la reglas a seguir sobre los datos
        public loginViewModel()
        {
            RuleFor(x => x.usuario).NotEmpty().WithMessage("Incompleto");

            RuleFor(x => x.password).NotEmpty().WithMessage("Incompleto");
        }

        public string? usuario { get; set; }
        public string? password { get; set; }
    }
}
