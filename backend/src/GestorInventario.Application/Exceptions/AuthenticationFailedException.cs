namespace GestorInventario.Application.Exceptions;

public sealed class AuthenticationFailedException : ApplicationExceptionBase
{
    public AuthenticationFailedException()
        : base(
            "invalid_credentials",
            "El usuario o la contraseña no son correctos.")
    {
    }
}
