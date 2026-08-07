namespace GestorInventario.Application.Exceptions;

public sealed class ConflictException : ApplicationExceptionBase
{
    public ConflictException(string code, string message)
        : base(code, message)
    {
    }
}
