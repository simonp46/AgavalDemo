SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

PRINT N'Iniciando validaciones de autenticación...';

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Modulos', N'U') IS NULL
   OR OBJECT_ID(N'dbo.UsuarioModulos', N'U') IS NULL
BEGIN
    THROW 51300, 'Falta una tabla del modelo de autenticación.', 1;
END;

IF OBJECT_ID(N'dbo.PK_Usuarios', N'PK') IS NULL
   OR OBJECT_ID(N'dbo.PK_Modulos', N'PK') IS NULL
   OR OBJECT_ID(N'dbo.PK_UsuarioModulos', N'PK') IS NULL
   OR OBJECT_ID(N'dbo.FK_UsuarioModulos_Usuarios', N'F') IS NULL
   OR OBJECT_ID(N'dbo.FK_UsuarioModulos_Modulos', N'F') IS NULL
BEGIN
    THROW 51301, 'Falta una PK o FK de autenticación.', 1;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Modulos
    WHERE Codigo = N'PRODUCTOS'
      AND Activo = 1
)
BEGIN
    THROW 51302, 'Falta el módulo PRODUCTOS activo.', 1;
END;

DECLARE @Procedimientos TABLE (Nombre SYSNAME NOT NULL);

INSERT INTO @Procedimientos (Nombre)
VALUES
    (N'usp_Usuarios_Registrar'),
    (N'usp_Usuarios_ObtenerCredenciales'),
    (N'usp_Usuarios_ObtenerPorId'),
    (N'usp_Usuarios_SugerirNombreUsuario'),
    (N'usp_Usuarios_TieneAccesoModulo'),
    (N'usp_Usuarios_Listar'),
    (N'usp_Usuarios_CambiarEstado'),
    (N'usp_Usuarios_AsignarAccesoModulo'),
    (N'usp_Usuarios_RevocarAccesoModulo');

IF EXISTS
(
    SELECT 1
    FROM @Procedimientos AS esperado
    WHERE OBJECT_ID(CONCAT(N'dbo.', esperado.Nombre), N'P') IS NULL
)
BEGIN
    THROW 51303, 'Falta un procedimiento almacenado de autenticación.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC dbo.usp_Usuarios_Registrar
        @Nombre = N'Usuario',
        @Apellidos = N'Validación',
        @NumeroDocumento = N'__AUTH_VALIDATION__',
        @Area = N'Calidad',
        @NombreUsuario = N'authvalidation99',
        @PasswordHash = N'PBKDF2-SHA512$210000$validation-salt$validation-hash',
        @ModuloCodigo = N'PRODUCTOS';

    DECLARE @UsuarioTemporalId INT =
    (
        SELECT Id
        FROM dbo.Usuarios
        WHERE NombreUsuario = N'authvalidation99'
    );

    IF @UsuarioTemporalId IS NULL
    BEGIN
        THROW 51304, 'El SP de registro no creó el usuario temporal.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UsuarioModulos AS acceso
        INNER JOIN dbo.Modulos AS modulo
            ON modulo.Id = acceso.ModuloId
        WHERE acceso.UsuarioId = @UsuarioTemporalId
          AND acceso.PuedeAcceder = 1
          AND modulo.Codigo = N'PRODUCTOS'
    )
    BEGIN
        THROW 51305, 'El registro no asignó acceso a PRODUCTOS.', 1;
    END;

    EXEC dbo.usp_Usuarios_ObtenerCredenciales
        @NombreUsuario = N'authvalidation99',
        @ModuloCodigo = N'PRODUCTOS';

    EXEC dbo.usp_Usuarios_TieneAccesoModulo
        @UsuarioId = @UsuarioTemporalId,
        @ModuloCodigo = N'PRODUCTOS';

    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

IF EXISTS
(
    SELECT 1
    FROM dbo.Usuarios
    WHERE NombreUsuario = N'authvalidation99'
)
BEGIN
    THROW 51306, 'La validación dejó datos temporales.', 1;
END;

PRINT N'VALIDACIÓN COMPLETA - autenticación, permisos y SP operativos.';
GO
