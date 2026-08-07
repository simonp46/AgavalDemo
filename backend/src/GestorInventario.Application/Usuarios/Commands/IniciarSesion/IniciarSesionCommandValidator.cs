using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Usuarios.Commands.IniciarSesion;

public sealed class IniciarSesionCommandValidator
    : IRequestValidator<IniciarSesionCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(IniciarSesionCommand request)
    {
        return UsuarioInputValidation.ValidateLogin(
            request.Usuario,
            request.Contrasena);
    }
}
