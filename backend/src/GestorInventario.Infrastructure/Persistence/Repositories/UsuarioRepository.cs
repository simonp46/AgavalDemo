using System.Data;
using System.Data.Common;
using GestorInventario.Application.Dtos;
using GestorInventario.Application.Exceptions;
using GestorInventario.Application.Interfaces;
using GestorInventario.Domain.Usuarios;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestorInventario.Infrastructure.Persistence.Repositories;

internal sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly PersistenceContext _context;

    public UsuarioRepository(PersistenceContext context)
    {
        _context = context;
    }

    public async Task<UsuarioDto> RegistrarAsync(
        Usuario usuario,
        string moduloCodigo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
            await using var command = CreateCommand("dbo.usp_Usuarios_Registrar");

            AddParameter(command, "@Nombre", usuario.Nombre, DbType.String);
            AddParameter(command, "@Apellidos", usuario.Apellidos, DbType.String);
            AddParameter(
                command,
                "@NumeroDocumento",
                usuario.NumeroDocumento,
                DbType.String);
            AddParameter(command, "@Area", usuario.Area, DbType.String);
            AddParameter(
                command,
                "@NombreUsuario",
                usuario.NombreUsuario,
                DbType.String);
            AddParameter(
                command,
                "@PasswordHash",
                usuario.PasswordHash,
                DbType.String);
            AddParameter(command, "@ModuloCodigo", moduloCodigo, DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "El procedimiento de registro no devolvió el usuario creado.");
            }

            return ReadUsuario(reader);
        }
        catch (SqlException exception)
        {
            throw MapRegistrationException(exception);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<UsuarioCredencialesDto?> ObtenerCredencialesAsync(
        string nombreUsuario,
        string moduloCodigo,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = CreateCommand(
                "dbo.usp_Usuarios_ObtenerCredenciales");
            AddParameter(command, "@NombreUsuario", nombreUsuario, DbType.String);
            AddParameter(command, "@ModuloCodigo", moduloCodigo, DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new UsuarioCredencialesDto(
                ReadUsuario(reader),
                reader.GetString(reader.GetOrdinal("PasswordHash")),
                reader.GetBoolean(reader.GetOrdinal("Activo")),
                reader.GetBoolean(reader.GetOrdinal("TieneAcceso")));
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<UsuarioAccesoDto?> ObtenerPorIdAsync(
        int usuarioId,
        string moduloCodigo,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = CreateCommand("dbo.usp_Usuarios_ObtenerPorId");
            AddParameter(command, "@UsuarioId", usuarioId, DbType.Int32);
            AddParameter(command, "@ModuloCodigo", moduloCodigo, DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new UsuarioAccesoDto(
                ReadUsuario(reader),
                reader.GetBoolean(reader.GetOrdinal("Activo")),
                reader.GetBoolean(reader.GetOrdinal("TieneAcceso")));
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<string> SugerirNombreUsuarioAsync(
        string baseUsuario,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
            await using var command = CreateCommand(
                "dbo.usp_Usuarios_SugerirNombreUsuario");
            AddParameter(command, "@BaseUsuario", baseUsuario, DbType.String);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result as string ?? throw new InvalidOperationException(
                "El procedimiento de sugerencia no devolvió un usuario.");
        }
        catch (SqlException exception) when (exception.Number == 51203)
        {
            throw new ConflictException(
                "username_suggestions_exhausted",
                "No hay nombres de usuario disponibles para la combinación indicada.");
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> TieneAccesoModuloAsync(
        int usuarioId,
        string moduloCodigo,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = CreateCommand(
                "dbo.usp_Usuarios_TieneAccesoModulo");
            AddParameter(command, "@UsuarioId", usuarioId, DbType.Int32);
            AddParameter(command, "@ModuloCodigo", moduloCodigo, DbType.String);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is bool tieneAcceso && tieneAcceso;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    private DbCommand CreateCommand(string procedureName)
    {
        var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        return command;
    }

    private static void AddParameter(
        DbCommand command,
        string name,
        object value,
        DbType type)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        parameter.DbType = type;
        command.Parameters.Add(parameter);
    }

    private static UsuarioDto ReadUsuario(DbDataReader reader)
    {
        return new UsuarioDto(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("Nombre")),
            reader.GetString(reader.GetOrdinal("Apellidos")),
            reader.GetString(reader.GetOrdinal("NumeroDocumento")),
            reader.GetString(reader.GetOrdinal("Area")),
            reader.GetString(reader.GetOrdinal("NombreUsuario")),
            DateTime.SpecifyKind(
                reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                DateTimeKind.Utc));
    }

    private static Exception MapRegistrationException(SqlException exception)
    {
        if (exception.Number == 51201 ||
            IsUniqueConstraint(exception, "UQ_Usuarios_NombreUsuario"))
        {
            return new ConflictException(
                "username_already_exists",
                "El nombre de usuario ya está registrado.");
        }

        if (exception.Number == 51202 ||
            IsUniqueConstraint(exception, "UQ_Usuarios_NumeroDocumento"))
        {
            return new ConflictException(
                "document_already_exists",
                "El número de documento ya está registrado.");
        }

        return exception;
    }

    private static bool IsUniqueConstraint(SqlException exception, string constraint)
    {
        return exception.Number is 2601 or 2627 &&
            exception.Message.Contains(constraint, StringComparison.OrdinalIgnoreCase);
    }
}
