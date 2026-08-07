using GestorInventario.Domain.Productos;

namespace GestorInventario.Application.Interfaces;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Producto?> ObtenerPorIdSoloLecturaAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Producto?> ObtenerParaAjusteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Producto>> ListarAsync(
        int? categoriaId,
        bool? stockBajo,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Producto producto,
        CancellationToken cancellationToken = default);

    void Eliminar(Producto producto);
}
