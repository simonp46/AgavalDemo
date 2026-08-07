using System.Globalization;
using System.Text;
using GestorInventario.Domain.Common;

namespace GestorInventario.Domain.Usuarios;

public sealed class Usuario
{
    public const int NombreMaximo = 100;
    public const int ApellidosMaximo = 100;
    public const int NumeroDocumentoMaximo = 30;
    public const int AreaMaxima = 100;
    public const int NombreUsuarioMaximo = 60;
    public const int PasswordHashMaximo = 512;

    private Usuario()
    {
        Nombre = string.Empty;
        Apellidos = string.Empty;
        NumeroDocumento = string.Empty;
        Area = string.Empty;
        NombreUsuario = string.Empty;
        PasswordHash = string.Empty;
    }

    private Usuario(
        string nombre,
        string apellidos,
        string numeroDocumento,
        string area,
        string nombreUsuario,
        string passwordHash)
    {
        Nombre = ValidarTexto(nombre, NombreMaximo, "nombre");
        Apellidos = ValidarTexto(apellidos, ApellidosMaximo, "apellidos");
        NumeroDocumento = ValidarTexto(
            numeroDocumento,
            NumeroDocumentoMaximo,
            "número de documento");
        Area = ValidarTexto(area, AreaMaxima, "área");
        NombreUsuario = ValidarNombreUsuario(nombreUsuario);
        PasswordHash = ValidarPasswordHash(passwordHash);
        Activo = true;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Apellidos { get; private set; }

    public string NumeroDocumento { get; private set; }

    public string Area { get; private set; }

    public string NombreUsuario { get; private set; }

    public string PasswordHash { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public static Usuario Crear(
        string nombre,
        string apellidos,
        string numeroDocumento,
        string area,
        string nombreUsuario,
        string passwordHash)
    {
        return new Usuario(
            nombre,
            apellidos,
            numeroDocumento,
            area,
            nombreUsuario,
            passwordHash);
    }

    public static string NormalizarNombreUsuario(string nombreUsuario)
    {
        return nombreUsuario.Trim().ToLowerInvariant();
    }

    public static string CrearBaseSugerencia(string nombre, string apellidos)
    {
        var primerNombre = PrimerTermino(nombre, "nombre");
        var primerApellido = PrimerTermino(apellidos, "apellidos");
        var baseUsuario = SoloLetrasYNumeros(primerNombre + primerApellido);

        if (baseUsuario.Length < 2)
        {
            throw new DomainException(
                "No fue posible generar una sugerencia con el nombre indicado.");
        }

        return baseUsuario[..Math.Min(baseUsuario.Length, NombreUsuarioMaximo - 2)];
    }

    private static string ValidarNombreUsuario(string nombreUsuario)
    {
        var normalizado = NormalizarNombreUsuario(nombreUsuario);

        if (normalizado.Length is < 4 or > NombreUsuarioMaximo)
        {
            throw new DomainException(
                $"El usuario debe tener entre 4 y {NombreUsuarioMaximo} caracteres.");
        }

        if (normalizado.Any(character =>
            !char.IsAsciiLetterOrDigit(character) &&
            character is not '.' and not '_' and not '-'))
        {
            throw new DomainException(
                "El usuario solo puede contener letras, números, punto, guion o guion bajo.");
        }

        return normalizado;
    }

    private static string ValidarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length < 32)
        {
            throw new DomainException("El hash de contraseña no es válido.");
        }

        if (passwordHash.Length > PasswordHashMaximo)
        {
            throw new DomainException(
                $"El hash de contraseña supera {PasswordHashMaximo} caracteres.");
        }

        return passwordHash;
    }

    private static string ValidarTexto(string value, int maximo, string campo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El {campo} es obligatorio.");
        }

        var normalizado = value.Trim();

        if (normalizado.Length > maximo)
        {
            throw new DomainException(
                $"El {campo} no puede superar {maximo} caracteres.");
        }

        return normalizado;
    }

    private static string PrimerTermino(string value, string campo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El {campo} es obligatorio.");
        }

        return value.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
    }

    private static string SoloLetrasYNumeros(string value)
    {
        var descompuesto = value.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(descompuesto.Length);

        foreach (var character in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) ==
                UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                result.Append(char.ToLowerInvariant(character));
            }
        }

        return result.ToString();
    }
}
