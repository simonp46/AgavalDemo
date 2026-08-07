using GestorInventario.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestorInventario.Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable(
            "Usuarios",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Usuarios_Nombre",
                    "LEN(LTRIM(RTRIM([Nombre]))) > 0");
                table.HasCheckConstraint(
                    "CK_Usuarios_Apellidos",
                    "LEN(LTRIM(RTRIM([Apellidos]))) > 0");
                table.HasCheckConstraint(
                    "CK_Usuarios_NumeroDocumento",
                    "LEN(LTRIM(RTRIM([NumeroDocumento]))) > 0");
                table.HasCheckConstraint(
                    "CK_Usuarios_Area",
                    "LEN(LTRIM(RTRIM([Area]))) > 0");
                table.HasCheckConstraint(
                    "CK_Usuarios_NombreUsuario",
                    "LEN(LTRIM(RTRIM([NombreUsuario]))) >= 4");
                table.HasCheckConstraint(
                    "CK_Usuarios_PasswordHash",
                    "LEN([PasswordHash]) >= 32");
            });

        builder.HasKey(usuario => usuario.Id)
            .HasName("PK_Usuarios");

        builder.HasAlternateKey(usuario => usuario.NumeroDocumento)
            .HasName("UQ_Usuarios_NumeroDocumento");

        builder.HasAlternateKey(usuario => usuario.NombreUsuario)
            .HasName("UQ_Usuarios_NombreUsuario");

        builder.Property(usuario => usuario.Id)
            .ValueGeneratedOnAdd();

        builder.Property(usuario => usuario.Nombre)
            .HasMaxLength(Usuario.NombreMaximo)
            .IsRequired();

        builder.Property(usuario => usuario.Apellidos)
            .HasMaxLength(Usuario.ApellidosMaximo)
            .IsRequired();

        builder.Property(usuario => usuario.NumeroDocumento)
            .HasMaxLength(Usuario.NumeroDocumentoMaximo)
            .IsRequired();

        builder.Property(usuario => usuario.Area)
            .HasMaxLength(Usuario.AreaMaxima)
            .IsRequired();

        builder.Property(usuario => usuario.NombreUsuario)
            .HasMaxLength(Usuario.NombreUsuarioMaximo)
            .IsRequired();

        builder.Property(usuario => usuario.PasswordHash)
            .HasMaxLength(Usuario.PasswordHashMaximo)
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(usuario => usuario.FechaCreacion)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd()
            .IsRequired();
    }
}
