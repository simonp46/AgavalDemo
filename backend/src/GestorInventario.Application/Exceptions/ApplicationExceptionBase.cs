namespace GestorInventario.Application.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
