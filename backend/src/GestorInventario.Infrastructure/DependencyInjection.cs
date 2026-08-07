using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Interfaces;
using GestorInventario.Infrastructure.Persistence;
using GestorInventario.Infrastructure.Persistence.Repositories;
using GestorInventario.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestorInventario.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName = "GestorInventarioDb";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"No se encontro la connection string '{ConnectionStringName}'.");
        }

        services.AddDbContext<PersistenceContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.EnableRetryOnFailure()));

        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<PersistenceContext>());

        return services;
    }
}
