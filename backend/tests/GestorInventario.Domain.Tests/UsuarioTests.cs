using GestorInventario.Domain.Common;
using GestorInventario.Domain.Usuarios;

namespace GestorInventario.Domain.Tests;

public sealed class UsuarioTests
{
    private const string ValidHash =
        "PBKDF2-SHA512$210000$c2FsdC1kZS1wcnVlYmE=$aGFzaC1kZS1wcnVlYmEtMzItYnl0ZXM=";

    [Fact]
    public void Crear_ConDatosValidos_NormalizaUsuario()
    {
        var usuario = Usuario.Crear(
            "  Ana  ",
            "  Pérez  ",
            "  12345  ",
            "  Compras  ",
            "  Ana.Perez01  ",
            ValidHash);

        Assert.Equal("Ana", usuario.Nombre);
        Assert.Equal("Pérez", usuario.Apellidos);
        Assert.Equal("ana.perez01", usuario.NombreUsuario);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void CrearBaseSugerencia_EliminaAcentosYUneNombreApellido()
    {
        var suggestionBase = Usuario.CrearBaseSugerencia(
            "María Fernanda",
            "Gómez Ruiz");

        Assert.Equal("mariagomez", suggestionBase);
    }

    [Fact]
    public void Crear_ConUsuarioInvalido_RechazaRegistro()
    {
        var exception = Assert.Throws<DomainException>(() => Usuario.Crear(
            "Ana",
            "Pérez",
            "12345",
            "Compras",
            "ana pérez",
            ValidHash));

        Assert.Contains("solo puede contener", exception.Message);
    }
}
