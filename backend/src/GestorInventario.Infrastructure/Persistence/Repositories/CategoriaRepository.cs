using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Categorias;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Infrastructure.Persistence.Repositories;

internal sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly PersistenceContext _context;

    public CategoriaRepository(PersistenceContext context)
    {
        _context = context;
    }

    public Task<Categoria?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Categorias
            .AsNoTracking()
            .SingleOrDefaultAsync(categoria => categoria.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Categoria>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Categorias
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Categorias.AnyAsync(
            categoria => categoria.Id == id,
            cancellationToken);
    }
}
