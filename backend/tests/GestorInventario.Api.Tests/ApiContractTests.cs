using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Categorias;
using GestorInventario.Domain.Productos;
using GestorInventario.Domain.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GestorInventario.Api.Tests;

public sealed class ApiContractTests : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client;

    public ApiContractTests(InventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Swagger_DocumentaTodosLosEndpointsObligatorios()
    {
        using var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        AssertOperation(paths, "/api/productos", "get");
        AssertOperation(paths, "/api/productos", "post");
        AssertOperation(paths, "/api/productos/{id}", "get");
        AssertOperation(paths, "/api/productos/{id}", "put");
        AssertOperation(paths, "/api/productos/{id}", "delete");
        AssertOperation(paths, "/api/productos/stock-bajo", "get");
        AssertOperation(paths, "/api/productos/{id}/stock", "patch");
        AssertOperation(paths, "/api/categorias", "get");
        AssertOperation(paths, "/api/auth/login", "post");
        AssertOperation(paths, "/api/auth/registro", "post");
        AssertOperation(paths, "/api/auth/sugerencia-usuario", "get");
        AssertOperation(paths, "/api/auth/sesion", "get");
        AssertOperation(paths, "/api/auth/logout", "post");

        AssertResponseContentType(
            paths,
            "/api/productos",
            "post",
            "201",
            "application/json");
        AssertResponseContentType(
            paths,
            "/api/productos",
            "post",
            "400",
            "application/problem+json");
        AssertResponseContentType(
            paths,
            "/api/productos/{id}/stock",
            "patch",
            "409",
            "application/problem+json");
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_CreaCookieHttpOnly()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { usuario = "usuario01", contrasena = "ClaveCorrecta" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.Contains("gestor-inventario.session", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistrarUsuario_ConDatosValidos_DevuelveCreated()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/registro",
            new
            {
                nombre = "Ana",
                apellidos = "Pérez",
                numeroDocumento = "123456",
                area = "Compras",
                usuario = "anaperez01",
                contrasena = "ClaveCorrecta"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Productos_SinSesion_DevuelveAuthenticationProblemDetails()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "authentication_required",
            problem.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ObtenerProducto_Inexistente_DevuelveProblemDetailsContractual()
    {
        using var response = await _client.GetAsync("/api/productos/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "product_not_found",
            problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "urn:gestor-inventario:problem:product-not-found",
            problem.RootElement.GetProperty("type").GetString());
        Assert.True(problem.RootElement.TryGetProperty("detail", out _));
        Assert.True(problem.RootElement.TryGetProperty("instance", out _));
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task RutaInexistente_DevuelveProblemDetailsCompleto()
    {
        using var response = await _client.GetAsync("/api/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.True(problem.RootElement.TryGetProperty("type", out _));
        Assert.True(problem.RootElement.TryGetProperty("title", out _));
        Assert.True(problem.RootElement.TryGetProperty("status", out _));
        Assert.True(problem.RootElement.TryGetProperty("detail", out _));
        Assert.True(problem.RootElement.TryGetProperty("instance", out _));
        Assert.True(problem.RootElement.TryGetProperty("code", out _));
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task CrearProducto_PayloadIncompleto_DevuelveErroresPorCampo()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync("/api/productos", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "validation_error",
            problem.RootElement.GetProperty("code").GetString());
        var errors = problem.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("nombre", out _));
        Assert.True(errors.TryGetProperty("precio", out _));
        Assert.True(errors.TryGetProperty("stock", out _));
        Assert.True(errors.TryGetProperty("stockMinimo", out _));
        Assert.True(errors.TryGetProperty("categoriaId", out _));
    }

    [Fact]
    public async Task CrearProducto_CategoriaInexistente_DevuelveErrorContractual()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/productos",
            new
            {
                nombre = "Producto",
                descripcion = (string?)null,
                precio = 1.50m,
                stock = 0,
                stockMinimo = 5,
                categoriaId = 99
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "category_reference_invalid",
            problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "La categoria indicada no es valida.",
            problem.RootElement.GetProperty("title").GetString());
        Assert.True(
            problem.RootElement
                .GetProperty("errors")
                .TryGetProperty("categoriaId", out _));
    }

    [Fact]
    public async Task AjustarStock_SalidaSuperior_DevuelveConflictContractual()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            "/api/productos/12/stock")
        {
            Content = JsonContent.Create(new
            {
                tipo = "SALIDA",
                cantidad = 2
            })
        };

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "insufficient_stock",
            problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(
            "No es posible completar el ajuste de stock.",
            problem.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Cors_PermiteElOrigenAngularConfigurado()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/api/productos");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:4200",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    private static void AssertOperation(
        JsonElement paths,
        string path,
        string operation)
    {
        Assert.True(paths.TryGetProperty(path, out var pathItem), $"Falta {path}.");
        Assert.True(
            pathItem.TryGetProperty(operation, out _),
            $"Falta {operation.ToUpperInvariant()} {path}.");
    }

    private static void AssertResponseContentType(
        JsonElement paths,
        string path,
        string operation,
        string status,
        string contentType)
    {
        var content = paths
            .GetProperty(path)
            .GetProperty(operation)
            .GetProperty("responses")
            .GetProperty(status)
            .GetProperty("content");

        Assert.True(
            content.TryGetProperty(contentType, out _),
            $"{operation.ToUpperInvariant()} {path} {status} no documenta {contentType}.");
    }
}

public sealed class InventoryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IProductoRepository>();
            services.RemoveAll<ICategoriaRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<IUsuarioRepository>();
            services.RemoveAll<IPasswordHasher>();

            services.AddSingleton<IProductoRepository, FakeProductoRepository>();
            services.AddSingleton<ICategoriaRepository, FakeCategoriaRepository>();
            services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
            services.AddSingleton<IUsuarioRepository, FakeUsuarioRepository>();
            services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
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

        public Task<UsuarioDto> RegistrarAsync(
            Usuario usuario,
            string moduloCodigo,
            CancellationToken cancellationToken = default)
        {
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
                    ? new UsuarioCredencialesDto(_usuario, "HASH", true, true)
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

    private sealed class FakeProductoRepository : IProductoRepository
    {
        private readonly Producto _producto =
            Producto.Crear("Producto", null, 10m, 1, 5, 1);

        public Task<Producto?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Producto?>(id == 12 ? _producto : null);
        }

        public Task<Producto?> ObtenerPorIdSoloLecturaAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Producto?>(id == 12 ? _producto : null);
        }

        public Task<Producto?> ObtenerParaAjusteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Producto?>(id == 12 ? _producto : null);
        }

        public Task<IReadOnlyList<Producto>> ListarAsync(
            int? categoriaId,
            bool? stockBajo,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Producto> result =
                stockBajo is false ? [] : [_producto];
            return Task.FromResult(result);
        }

        public Task AgregarAsync(
            Producto producto,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Eliminar(Producto producto)
        {
        }
    }

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        private readonly Categoria _categoria = CreateCategoria();

        public Task<Categoria?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Categoria?>(id == 1 ? _categoria : null);
        }

        public Task<IReadOnlyList<Categoria>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Categoria>>([_categoria]);
        }

        public Task<bool> ExisteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(id == 1);
        }

        private static Categoria CreateCategoria()
        {
            var categoria = Categoria.Crear("Oficina");
            typeof(Categoria)
                .GetProperty(nameof(Categoria.Id))!
                .SetValue(categoria, 1);
            return categoria;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }
}

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "usuario01"),
            new("modulo", ModuloAcceso.Productos)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
