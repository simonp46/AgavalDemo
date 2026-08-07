namespace GestorInventario.Application.Exceptions;

public sealed class ValidationException : ApplicationExceptionBase
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(
            "validation_error",
            "La solicitud contiene datos invalidos.",
            errors)
    {
    }
}
