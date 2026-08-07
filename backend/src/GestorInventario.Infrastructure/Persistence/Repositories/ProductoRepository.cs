using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Productos;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Infrastructure.Persistence.Repositories;

internal sealed class ProductoRepository : IProductoRepository
{
    private readonly PersistenceContext _context;

    public ProductoRepository(PersistenceContext context)
    {
        _context = context;
    }

    public Task<Producto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Productos.SingleOrDefaultAsync(
            producto => producto.Id == id,
            cancellationToken);
    }

    public Task<Producto?> ObtenerPorIdSoloLecturaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Productos
            .AsNoTracking()
            .SingleOrDefaultAsync(producto => producto.Id == id, cancellationToken);
    }

    public Task<Producto?> ObtenerParaAjusteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Productos
            .FromSqlInterpolated($"""
                SELECT [Id], [Nombre], [Descripcion], [Precio], [Stock],
                       [StockMinimo], [CategoriaId], [FechaCreacion]
                FROM [Productos] WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                WHERE [Id] = {id}
                """)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ListarAsync(
        int? categoriaId,
        bool? stockBajo,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Producto> query = _context.Productos.AsNoTracking();

        if (categoriaId.HasValue)
        {
            query = query.Where(producto => producto.CategoriaId == categoriaId.Value);
        }

        if (stockBajo.HasValue)
        {
            query = stockBajo.Value
                ? query.Where(producto => producto.Stock < producto.StockMinimo)
                : query.Where(producto => producto.Stock >= producto.StockMinimo);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        Producto producto,
        CancellationToken cancellationToken = default)
    {
        await _context.Productos.AddAsync(producto, cancellationToken);
    }

    public void Eliminar(Producto producto)
    {
        _context.Productos.Remove(producto);
    }
}
