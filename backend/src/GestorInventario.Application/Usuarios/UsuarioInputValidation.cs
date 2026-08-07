using GestorInventario.Domain.Usuarios;

namespace GestorInventario.Application.Usuarios;

internal static class UsuarioInputValidation
{
    public const int PasswordMinimo = 8;
    public const int PasswordMaximo = 128;

    public static IReadOnlyDictionary<string, string[]> ValidateRegistration(
        string nombre,
        string apellidos,
        string numeroDocumento,
        string area,
        string nombreUsuario,
        string password)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        ValidateRequired(errors, "nombre", nombre, Usuario.NombreMaximo);
        ValidateRequired(errors, "apellidos", apellidos, Usuario.ApellidosMaximo);
        ValidateRequired(
            errors,
            "numeroDocumento",
            numeroDocumento,
            Usuario.NumeroDocumentoMaximo);
        ValidateRequired(errors, "area", area, Usuario.AreaMaxima);

        var usuarioNormalizado = nombreUsuario?.Trim() ?? string.Empty;

        if (usuarioNormalizado.Length is < 4 or > Usuario.NombreUsuarioMaximo)
        {
            Add(
                errors,
                "usuario",
                $"El usuario debe tener entre 4 y {Usuario.NombreUsuarioMaximo} caracteres.");
        }
        else if (usuarioNormalizado.Any(character =>
            !char.IsAsciiLetterOrDigit(character) &&
            character is not '.' and not '_' and not '-'))
        {
            Add(
                errors,
                "usuario",
                "El usuario solo puede contener letras, números, punto, guion o guion bajo.");
        }

        ValidatePassword(errors, password, requireMinimum: true);

        return errors.ToDictionary(
            item => item.Key,
            item => item.Value.ToArray(),
            StringComparer.Ordinal);
    }

    public static IReadOnlyDictionary<string, string[]> ValidateLogin(
        string nombreUsuario,
        string password)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            Add(errors, "usuario", "El usuario es obligatorio.");
        }
        else if (nombreUsuario.Trim().Length > Usuario.NombreUsuarioMaximo)
        {
            Add(
                errors,
                "usuario",
                $"El usuario no puede superar {Usuario.NombreUsuarioMaximo} caracteres.");
        }

        ValidatePassword(errors, password, requireMinimum: false);

        return errors.ToDictionary(
            item => item.Key,
            item => item.Value.ToArray(),
            StringComparer.Ordinal);
    }

    public static IReadOnlyDictionary<string, string[]> ValidateSuggestion(
        string nombre,
        string apellidos)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        ValidateRequired(errors, "nombre", nombre, Usuario.NombreMaximo);
        ValidateRequired(errors, "apellidos", apellidos, Usuario.ApellidosMaximo);

        return errors.ToDictionary(
            item => item.Key,
            item => item.Value.ToArray(),
            StringComparer.Ordinal);
    }

    private static void ValidatePassword(
        Dictionary<string, List<string>> errors,
        string password,
        bool requireMinimum)
    {
        if (string.IsNullOrEmpty(password))
        {
            Add(errors, "contrasena", "La contraseña es obligatoria.");
            return;
        }

        if (requireMinimum && password.Length < PasswordMinimo)
        {
            Add(
                errors,
                "contrasena",
                $"La contraseña debe tener al menos {PasswordMinimo} caracteres.");
        }

        if (password.Length > PasswordMaximo)
        {
            Add(
                errors,
                "contrasena",
                $"La contraseña no puede superar {PasswordMaximo} caracteres.");
        }
    }

    private static void ValidateRequired(
        Dictionary<string, List<string>> errors,
        string key,
        string value,
        int maximo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(errors, key, $"El campo {key} es obligatorio.");
            return;
        }

        if (value.Trim().Length > maximo)
        {
            Add(errors, key, $"El campo {key} no puede superar {maximo} caracteres.");
        }
    }

    private static void Add(
        Dictionary<string, List<string>> errors,
        string key,
        string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}
