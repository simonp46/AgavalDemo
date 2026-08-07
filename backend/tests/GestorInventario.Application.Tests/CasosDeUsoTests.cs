using GestorInventario.Application.Common.Interfaces;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Application.Productos.Commands.AjustarStock;
using GestorInventario.Application.Productos.Commands.CrearProducto;
using GestorInventario.Application.Productos.Queries.ListarProductos;
using GestorInventario.Application.Productos.Queries.ObtenerProductoPorId;
using GestorInventario.Domain.Categorias;
using GestorInventario.Domain.Productos;

namespace GestorInventario.Application.Tests;

public sealed class CasosDeUsoTests
{
    [Fact]
    public void CrearProductoValidator_DatosInvalidos_ReportaCamposPublicos()
    {
        var validator = new CrearProductoCommandValidator();
        var command = new CrearProductoCommand(" ", null, 0m, -1, 5, 0);

        var errors = validator.Validate(command);

        Assert.Contains("nombre", errors.Keys);
        Assert.Contains("precio", errors.Keys);
        Assert.Contains("stock", errors.Keys);
        Assert.Contains("categoriaId", errors.Keys);
    }

    [Fact]
    public void AjustarStockValidator_TipoYCantidadInvalidos_ReportaErrores()
    {
        var validator = new AjustarStockCommandValidator();

        var errors = validator.Validate(new AjustarStockCommand(0, "entrada", 0));

        Assert.Contains("id", errors.Keys);
        Assert.Contains("tipo", errors.Keys);
        Assert.Contains("cantidad", errors.Keys);
    }

    [Fact]
    public async Task CrearProducto_CategoriaInexistente_DevuelveErrorContractual()
    {
        var handler = new CrearProductoCommandHandler(
            new FakeProductoRepository(),
            new FakeCategoriaRepository(),
            new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<CategoryReferenceInvalidException>(
            () => handler.Handle(
                new CrearProductoCommand("Producto", null, 1m, 0, 5, 99),
                CancellationToken.None));

        Assert.Equal("category_reference_invalid", exception.Code);
        Assert.Contains("categoriaId", exception.Errors!.Keys);
    }

    [Fact]
    public async Task AjustarStock_SalidaSuperior_DevuelveConflictoSinModificarStock()
    {
        var producto = Producto.Crear("Producto", null, 1m, 1, 5, 1);
        var repository = new FakeProductoRepository(producto);
        var handler = new AjustarStockCommandHandler(repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(
                new AjustarStockCommand(12, "SALIDA", 2),
                CancellationToken.None));

        Assert.Equal("insufficient_stock", exception.Code);
        Assert.Equal(1, producto.Stock);
    }

    [Fact]
    public async Task ListarProductos_CombinaFiltrosYProyectaCategoria()
    {
        var producto = Producto.Crear("Producto", null, 10m, 1, 5, 2);
        var productoRepository = new FakeProductoRepository(producto);
        var categoriaRepository = new FakeCategoriaRepository(
            Categoria.Crear("Oficina"));
        categoriaRepository.ForcedCategoryId = 2;
        var handler = new ListarProductosQueryHandler(
            productoRepository,
            categoriaRepository);

        var result = await handler.Handle(
            new ListarProductosQuery(2, "bajo"),
            CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal("Oficina", item.CategoriaNombre);
        Assert.True(item.EsStockBajo);
        Assert.Equal(2, productoRepository.LastCategoriaId);
        Assert.True(productoRepository.LastStockBajo);
    }

    [Fact]
    public async Task ObtenerProductoPorId_Inexistente_DevuelveNotFoundContractual()
    {
        var handler = new ObtenerProductoPorIdQueryHandler(
            new FakeProductoRepository(),
            new FakeCategoriaRepository());

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new ObtenerProductoPorIdQuery(42),
                CancellationToken.None));

        Assert.Equal("product_not_found", exception.Code);
    }

    private sealed class FakeProductoRepository : IProductoRepository
    {
        private readonly List<Producto> _productos;

        public FakeProductoRepository(params Producto[] productos)
        {
            _productos = [.. productos];
        }

        public int? LastCategoriaId { get; private set; }

        public bool? LastStockBajo { get; private set; }

        public Task<Producto?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_productos.FirstOrDefault());
        }

        public Task<Producto?> ObtenerPorIdSoloLecturaAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_productos.FirstOrDefault());
        }

        public Task<Producto?> ObtenerParaAjusteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_productos.FirstOrDefault());
        }

        public Task<IReadOnlyList<Producto>> ListarAsync(
            int? categoriaId,
            bool? stockBajo,
            CancellationToken cancellationToken = default)
        {
            LastCategoriaId = categoriaId;
            LastStockBajo = stockBajo;
            return Task.FromResult<IReadOnlyList<Producto>>(_productos);
        }

        public Task AgregarAsync(
            Producto producto,
            CancellationToken cancellationToken = default)
        {
            _productos.Add(producto);
            return Task.CompletedTask;
        }

        public void Eliminar(Producto producto)
        {
            _productos.Remove(producto);
        }
    }

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        private readonly Categoria? _categoria;

        public FakeCategoriaRepository(Categoria? categoria = null)
        {
            _categoria = categoria;
        }

        public int? ForcedCategoryId { get; set; }

        public Task<Categoria?> ObtenerPorIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_categoria);
        }

        public Task<IReadOnlyList<Categoria>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            if (_categoria is null)
            {
                return Task.FromResult<IReadOnlyList<Categoria>>([]);
            }

            if (ForcedCategoryId.HasValue)
            {
                typeof(Categoria)
                    .GetProperty(nameof(Categoria.Id))!
                    .SetValue(_categoria, ForcedCategoryId.Value);
            }

            return Task.FromResult<IReadOnlyList<Categoria>>([_categoria]);
        }

        public Task<bool> ExisteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_categoria is not null);
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
