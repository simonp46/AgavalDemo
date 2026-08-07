using GestorInventario.Domain.Categorias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorInventario.Infrastructure.Persistence.Configurations;

internal sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(categoria => categoria.Id)
            .HasName("PK_Categorias");

        builder.Property(categoria => categoria.Id)
            .ValueGeneratedOnAdd();

        builder.Property(categoria => categoria.Nombre)
            .HasMaxLength(Categoria.NombreMaximo)
            .IsRequired();

        builder.Property(categoria => categoria.Activo)
            .HasDefaultValue(true)
            .IsRequired();
    }
}
