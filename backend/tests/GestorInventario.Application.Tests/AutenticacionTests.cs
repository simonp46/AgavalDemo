using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Usuarios.Commands.IniciarSesion;
using GestorInventario.Application.Usuarios.Commands.RegistrarUsuario;
using GestorInventario.Application.Usuarios.Queries.SugerirNombreUsuario;
using GestorInventario.Domain.Usuarios;

namespace GestorInventario.Application.Tests;

public sealed class AutenticacionTests
{
    [Fact]
    public async Task IniciarSesion_ConCredencialesValidas_DevuelveUsuario()
    {
        var repository = new FakeUsuarioRepository();
        var handler = new IniciarSesionCommandHandler(
            repository,
            new FakePasswordHasher());

        var result = await handler.Handle(
            new IniciarSesionCommand("USUARIO01", "ClaveCorrecta"),
            CancellationToken.None);

        Assert.Equal("usuario01", result.NombreUsuario);
    }

    [Fact]
    public async Task IniciarSesion_ConPasswordInvalido_NoRevelaElMotivo()
    {
        var handler = new IniciarSesionCommandHandler(
            new FakeUsuarioRepository(),
            new FakePasswordHasher());

        var exception = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => handler.Handle(
                new IniciarSesionCommand("usuario01", "Incorrecta"),
                CancellationToken.None));

        Assert.Equal("invalid_credentials", exception.Code);
    }

    [Fact]
    public async Task RegistrarUsuario_HasheaPasswordYAsignaModuloProductos()
    {
        var repository = new FakeUsuarioRepository();
        var handler = new RegistrarUsuarioCommandHandler(
            repository,
            new FakePasswordHasher());

        var result = await handler.Handle(
            new RegistrarUsuarioCommand(
                "Ana",
                "Pérez",
                "123456",
                "Compras",
                "anaperez01",
                "ClaveCorrecta"),
            CancellationToken.None);

        Assert.Equal("anaperez01", result.NombreUsuario);
        Assert.Equal(ModuloAcceso.Productos, repository.LastModule);
        Assert.StartsWith("HASH-", repository.LastUser?.PasswordHash);
    }

    [Fact]
    public async Task SugerirUsuario_UsaBaseNormalizada()
    {
        var repository = new FakeUsuarioRepository();
        var handler = new SugerirNombreUsuarioQueryHandler(repository);

        var result = await handler.Handle(
            new SugerirNombreUsuarioQuery("María", "Gómez"),
            CancellationToken.None);

        Assert.Equal("mariagomez00", result);
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"HASH-{password}-contenido-suficientemente-largo";
        }

        public bool Verify(string passwordHash, string password)
        {
            return password == "ClaveCorrecta";
        }
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly UsuarioDto _usuario = new(
            1,
            "Ana",
            "Pérez",
            "123456",
            "Compras",
            "usuario01",
            DateTime.UtcNow);

        public Usuario? LastUser { get; private set; }

        public string? LastModule { get; private set; }

        public Task<UsuarioDto> RegistrarAsync(
            Usuario usuario,
            string moduloCodigo,
            CancellationToken cancellationToken = default)
        {
            LastUser = usuario;
            LastModule = moduloCodigo;
            return Task.FromResult(_usuario with
            {
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                NumeroDocumento = usuario.NumeroDocumento,
                Area = usuario.Area,
                NombreUsuario = usuario.NombreUsuario
            });
        }

        public Task<UsuarioCredencialesDto?> ObtenerCredencialesAsync(
            string nombreUsuario,
            string moduloCodigo,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UsuarioCredencialesDto?>(
                nombreUsuario == "usuario01"
                    ? new UsuarioCredencialesDto(
                        _usuario,
                        "HASH",
                        Activo: true,
                        TieneAcceso: true)
                    : null);
        }

        public Task<UsuarioAccesoDto?> ObtenerPorIdAsync(
            int usuarioId,
            string moduloCodigo,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UsuarioAccesoDto?>(
                new UsuarioAccesoDto(_usuario, true, true));
        }

        public Task<string> SugerirNombreUsuarioAsync(
            string baseUsuario,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"{baseUsuario}00");
        }

        public Task<bool> TieneAccesoModuloAsync(
            int usuarioId,
            string moduloCodigo,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
