namespace GestorInventario.Application.Common.Validation;

public interface IRequestValidator<in TRequest>
    where TRequest : notnull
{
    IReadOnlyDictionary<string, string[]> Validate(TRequest request);
}
