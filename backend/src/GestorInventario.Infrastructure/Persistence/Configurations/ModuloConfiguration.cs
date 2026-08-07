using GestorInventario.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorInventario.Infrastructure.Persistence.Configurations;

internal sealed class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable(
            "Modulos",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Modulos_Codigo",
                    "LEN(LTRIM(RTRIM([Codigo]))) > 0");
                table.HasCheckConstraint(
                    "CK_Modulos_Nombre",
                    "LEN(LTRIM(RTRIM([Nombre]))) > 0");
            });

        builder.HasKey(modulo => modulo.Id)
            .HasName("PK_Modulos");

        builder.HasAlternateKey(modulo => modulo.Codigo)
            .HasName("UQ_Modulos_Codigo");

        builder.Property(modulo => modulo.Id)
            .ValueGeneratedOnAdd();

        builder.Property(modulo => modulo.Codigo)
            .HasMaxLength(Modulo.CodigoMaximo)
            .IsRequired();

        builder.Property(modulo => modulo.Nombre)
            .HasMaxLength(Modulo.NombreMaximo)
            .IsRequired();

        builder.Property(modulo => modulo.Activo)
            .HasDefaultValue(true)
            .IsRequired();
    }
}
