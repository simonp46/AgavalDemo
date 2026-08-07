using GestorInventario.Domain.Categorias;
using GestorInventario.Domain.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorInventario.Infrastructure.Persistence.Configurations;

internal sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable(
            "Productos",
            table =>
            {
                table.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
                table.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
            });

        builder.HasKey(producto => producto.Id)
            .HasName("PK_Productos");

        builder.Property(producto => producto.Id)
            .ValueGeneratedOnAdd();

        builder.Property(producto => producto.Nombre)
            .HasMaxLength(Producto.NombreMaximo)
            .IsRequired();

        builder.Property(producto => producto.Descripcion)
            .HasMaxLength(Producto.DescripcionMaxima)
            .IsRequired(false);

        builder.Property(producto => producto.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(producto => producto.Stock)
            .HasDefaultValue(0)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(producto => producto.StockMinimo)
            .HasDefaultValue(5)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(producto => producto.CategoriaId)
            .IsRequired();

        builder.Property(producto => producto.FechaCreacion)
            .HasDefaultValueSql("GETDATE()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Ignore(producto => producto.EsStockBajo);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(producto => producto.CategoriaId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Productos_Categorias");
    }
}
