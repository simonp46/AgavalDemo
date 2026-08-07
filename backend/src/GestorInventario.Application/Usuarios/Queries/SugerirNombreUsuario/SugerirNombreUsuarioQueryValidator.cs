using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Usuarios.Queries.SugerirNombreUsuario;

public sealed class SugerirNombreUsuarioQueryValidator
    : IRequestValidator<SugerirNombreUsuarioQuery>
{
    public IReadOnlyDictionary<string, string[]> Validate(
        SugerirNombreUsuarioQuery request)
    {
        return UsuarioInputValidation.ValidateSuggestion(
            request.Nombre,
            request.Apellidos);
    }
}
