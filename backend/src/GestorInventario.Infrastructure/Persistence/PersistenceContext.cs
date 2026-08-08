using System.Data;
using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Exceptions;
using GestorInventario.Domain.Categorias;
using GestorInventario.Domain.Productos;
using GestorInventario.Domain.Usuarios;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Infrastructure.Persistence;

public sealed class PersistenceContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
{
    public PersistenceContext(DbContextOptions<PersistenceContext> options)
        : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Modulo> Modulos => Set<Modulo>();

    public DbSet<UsuarioModulo> UsuarioModulos => Set<UsuarioModulo>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.Entries.Any(entry =>
                entry.Entity is Producto &&
                entry.State == EntityState.Deleted))
        {
            throw new ConflictException(
                "delete_conflict",
                "Una restriccion de integridad impide eliminar el producto.");
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            ChangeTracker.Clear();

            await using var transaction = await Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseNamedDefaultConstraints();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersistenceContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
