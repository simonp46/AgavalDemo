using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Usuarios.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioCommandValidator
    : IRequestValidator<RegistrarUsuarioCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(
        RegistrarUsuarioCommand request)
    {
        return UsuarioInputValidation.ValidateRegistration(
            request.Nombre,
            request.Apellidos,
            request.NumeroDocumento,
            request.Area,
            request.Usuario,
            request.Contrasena);
    }
}
