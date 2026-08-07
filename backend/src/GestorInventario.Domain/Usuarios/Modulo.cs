using GestorInventario.Domain.Common;

namespace GestorInventario.Domain.Usuarios;

public sealed class Modulo
{
    public const int CodigoMaximo = 50;
    public const int NombreMaximo = 100;

    private Modulo()
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
    }

    private Modulo(string codigo, string nombre, bool activo)
    {
        Codigo = ValidarTexto(codigo, CodigoMaximo, "código").ToUpperInvariant();
        Nombre = ValidarTexto(nombre, NombreMaximo, "nombre");
        Activo = activo;
    }

    public int Id { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo { get; private set; }

    public static Modulo Crear(string codigo, string nombre, bool activo = true)
    {
        return new Modulo(codigo, nombre, activo);
    }

    private static string ValidarTexto(string value, int maximo, string campo)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El {campo} del módulo es obligatorio.");
        }

        var normalizado = value.Trim();

        if (normalizado.Length > maximo)
        {
            throw new DomainException(
                $"El {campo} del módulo no puede superar {maximo} caracteres.");
        }

        return normalizado;
    }
}
