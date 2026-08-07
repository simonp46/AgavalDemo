using GestorInventario.Domain.Categorias;

namespace GestorInventario.Application.Interfaces;

public interface ICategoriaRepository
{
    Task<Categoria?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Categoria>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
