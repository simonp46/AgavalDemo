using GestorInventario.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorInventario.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioModuloConfiguration
    : IEntityTypeConfiguration<UsuarioModulo>
{
    public void Configure(EntityTypeBuilder<UsuarioModulo> builder)
    {
        builder.ToTable("UsuarioModulos");

        builder.HasKey(acceso => new { acceso.UsuarioId, acceso.ModuloId })
            .HasName("PK_UsuarioModulos");

        builder.Property(acceso => acceso.PuedeAcceder)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(acceso => acceso.FechaAsignacion)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(acceso => acceso.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UsuarioModulos_Usuarios");

        builder.HasOne<Modulo>()
            .WithMany()
            .HasForeignKey(acceso => acceso.ModuloId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UsuarioModulos_Modulos");
    }
}
