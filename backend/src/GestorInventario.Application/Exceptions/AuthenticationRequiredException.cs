namespace GestorInventario.Application.Exceptions;

public sealed class AuthenticationRequiredException : ApplicationExceptionBase
{
    public AuthenticationRequiredException()
        : base(
            "authentication_required",
            "La sesión no es válida o ha expirado.")
    {
    }
}
