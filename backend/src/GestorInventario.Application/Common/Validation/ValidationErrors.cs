namespace GestorInventario.Application.Common.Validation;

public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors =
        new(StringComparer.Ordinal);

    public void Add(string propertyName, string message)
    {
        if (!_errors.TryGetValue(propertyName, out var messages))
        {
            messages = [];
            _errors[propertyName] = messages;
        }

        messages.Add(message);
    }

    public IReadOnlyDictionary<string, string[]> ToDictionary()
    {
        return _errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Distinct(StringComparer.Ordinal).ToArray(),
            StringComparer.Ordinal);
    }
}
