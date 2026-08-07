using GestorInventario.Application.Common.Validation;

namespace GestorInventario.Application.Usuarios.Queries.ObtenerSesion;

public sealed class ObtenerSesionQueryValidator : IRequestValidator<ObtenerSesionQuery>
{
    public IReadOnlyDictionary<string, string[]> Validate(ObtenerSesionQuery request)
    {
        return request.UsuarioId > 0
            ? new Dictionary<string, string[]>()
            : new Dictionary<string, string[]>
            {
                ["usuarioId"] = ["El identificador del usuario no es válido."]
            };
    }
}
