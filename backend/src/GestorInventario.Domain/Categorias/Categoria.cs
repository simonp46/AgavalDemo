using GestorInventario.Domain.Common;

namespace GestorInventario.Domain.Categorias;

public sealed class Categoria
{
    public const int NombreMaximo = 100;

    private Categoria()
    {
        Nombre = string.Empty;
    }

    private Categoria(string nombre, bool activo)
    {
        Nombre = ValidarNombre(nombre);
        Activo = activo;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo { get; private set; }

    public static Categoria Crear(string nombre, bool activo = true)
    {
        return new Categoria(nombre, activo);
    }

    public void ActualizarNombre(string nombre)
    {
        Nombre = ValidarNombre(nombre);
    }

    public void CambiarEstado(bool activo)
    {
        Activo = activo;
    }

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre de la categoria es obligatorio.");
        }

        var nombreNormalizado = nombre.Trim();

        if (nombreNormalizado.Length > NombreMaximo)
        {
            throw new DomainException(
                $"El nombre de la categoria no puede superar {NombreMaximo} caracteres.");
        }

        return nombreNormalizado;
    }
}
