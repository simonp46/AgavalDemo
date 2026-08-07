namespace GestorInventario.Domain.Usuarios;

public sealed class UsuarioModulo
{
    private UsuarioModulo()
    {
    }

    private UsuarioModulo(int usuarioId, int moduloId)
    {
        UsuarioId = usuarioId;
        ModuloId = moduloId;
        PuedeAcceder = true;
    }

    public int UsuarioId { get; private set; }

    public int ModuloId { get; private set; }

    public bool PuedeAcceder { get; private set; }

    public DateTime FechaAsignacion { get; private set; }

    public static UsuarioModulo Crear(int usuarioId, int moduloId)
    {
        return new UsuarioModulo(usuarioId, moduloId);
    }

    public void CambiarAcceso(bool puedeAcceder)
    {
        PuedeAcceder = puedeAcceder;
    }
}
