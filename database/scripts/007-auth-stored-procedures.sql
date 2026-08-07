SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_Registrar
    @Nombre NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @NumeroDocumento NVARCHAR(30),
    @Area NVARCHAR(100),
    @NombreUsuario NVARCHAR(60),
    @PasswordHash NVARCHAR(512),
    @ModuloCodigo NVARCHAR(50) = N'PRODUCTOS'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @InicioTransaccion BIT = 0;

    IF @@TRANCOUNT = 0
    BEGIN
        SET @InicioTransaccion = 1;
        BEGIN TRANSACTION;
    END;

    BEGIN TRY
        DECLARE @ModuloId INT =
        (
            SELECT Id
            FROM dbo.Modulos WITH (UPDLOCK, HOLDLOCK)
            WHERE Codigo = @ModuloCodigo
              AND Activo = 1
        );

        IF @ModuloId IS NULL
        BEGIN
            THROW 51200, 'El módulo solicitado no existe o está inactivo.', 1;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
            WHERE NombreUsuario = @NombreUsuario
        )
        BEGIN
            THROW 51201, 'El nombre de usuario ya está registrado.', 1;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
            WHERE NumeroDocumento = @NumeroDocumento
        )
        BEGIN
            THROW 51202, 'El número de documento ya está registrado.', 1;
        END;

        INSERT INTO dbo.Usuarios
        (
            Nombre,
            Apellidos,
            NumeroDocumento,
            Area,
            NombreUsuario,
            PasswordHash
        )
        VALUES
        (
            @Nombre,
            @Apellidos,
            @NumeroDocumento,
            @Area,
            @NombreUsuario,
            @PasswordHash
        );

        DECLARE @UsuarioId INT = CONVERT(INT, SCOPE_IDENTITY());

        INSERT INTO dbo.UsuarioModulos (UsuarioId, ModuloId, PuedeAcceder)
        VALUES (@UsuarioId, @ModuloId, 1);

        IF @InicioTransaccion = 1
        BEGIN
            COMMIT TRANSACTION;
        END;

        SELECT
            usuario.Id,
            usuario.Nombre,
            usuario.Apellidos,
            usuario.NumeroDocumento,
            usuario.Area,
            usuario.NombreUsuario,
            usuario.Activo,
            usuario.FechaCreacion
        FROM dbo.Usuarios AS usuario
        WHERE usuario.Id = @UsuarioId;
    END TRY
    BEGIN CATCH
        IF @InicioTransaccion = 1 AND XACT_STATE() <> 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_ObtenerCredenciales
    @NombreUsuario NVARCHAR(60),
    @ModuloCodigo NVARCHAR(50) = N'PRODUCTOS'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        usuario.Id,
        usuario.Nombre,
        usuario.Apellidos,
        usuario.NumeroDocumento,
        usuario.Area,
        usuario.NombreUsuario,
        usuario.PasswordHash,
        usuario.Activo,
        usuario.FechaCreacion,
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.UsuarioModulos AS acceso
            INNER JOIN dbo.Modulos AS modulo
                ON modulo.Id = acceso.ModuloId
            WHERE acceso.UsuarioId = usuario.Id
              AND acceso.PuedeAcceder = 1
              AND modulo.Codigo = @ModuloCodigo
              AND modulo.Activo = 1
        ) THEN 1 ELSE 0 END) AS TieneAcceso
    FROM dbo.Usuarios AS usuario
    WHERE usuario.NombreUsuario = @NombreUsuario;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_ObtenerPorId
    @UsuarioId INT,
    @ModuloCodigo NVARCHAR(50) = N'PRODUCTOS'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        usuario.Id,
        usuario.Nombre,
        usuario.Apellidos,
        usuario.NumeroDocumento,
        usuario.Area,
        usuario.NombreUsuario,
        usuario.Activo,
        usuario.FechaCreacion,
        CONVERT(BIT, CASE WHEN EXISTS
        (
            SELECT 1
            FROM dbo.UsuarioModulos AS acceso
            INNER JOIN dbo.Modulos AS modulo
                ON modulo.Id = acceso.ModuloId
            WHERE acceso.UsuarioId = usuario.Id
              AND acceso.PuedeAcceder = 1
              AND modulo.Codigo = @ModuloCodigo
              AND modulo.Activo = 1
        ) THEN 1 ELSE 0 END) AS TieneAcceso
    FROM dbo.Usuarios AS usuario
    WHERE usuario.Id = @UsuarioId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_SugerirNombreUsuario
    @BaseUsuario NVARCHAR(58)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NombreUsuario NVARCHAR(60);

    ;WITH Numeros AS
    (
        SELECT numero
        FROM
        (
            VALUES
                (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),
                (10),(11),(12),(13),(14),(15),(16),(17),(18),(19),
                (20),(21),(22),(23),(24),(25),(26),(27),(28),(29),
                (30),(31),(32),(33),(34),(35),(36),(37),(38),(39),
                (40),(41),(42),(43),(44),(45),(46),(47),(48),(49),
                (50),(51),(52),(53),(54),(55),(56),(57),(58),(59),
                (60),(61),(62),(63),(64),(65),(66),(67),(68),(69),
                (70),(71),(72),(73),(74),(75),(76),(77),(78),(79),
                (80),(81),(82),(83),(84),(85),(86),(87),(88),(89),
                (90),(91),(92),(93),(94),(95),(96),(97),(98),(99)
        ) AS valores (numero)
    )
    SELECT TOP (1)
        @NombreUsuario = CONCAT(
            @BaseUsuario,
            RIGHT(CONCAT(N'0', CONVERT(NVARCHAR(2), numero)), 2))
    FROM Numeros
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE NombreUsuario = CONCAT(
            @BaseUsuario,
            RIGHT(CONCAT(N'0', CONVERT(NVARCHAR(2), numero)), 2))
    )
    ORDER BY numero;

    IF @NombreUsuario IS NULL
    BEGIN
        THROW 51203, 'No hay nombres de usuario disponibles para la base indicada.', 1;
    END;

    SELECT @NombreUsuario AS NombreUsuario;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_TieneAccesoModulo
    @UsuarioId INT,
    @ModuloCodigo NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CONVERT(BIT, CASE WHEN EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios AS usuario
        INNER JOIN dbo.UsuarioModulos AS acceso
            ON acceso.UsuarioId = usuario.Id
        INNER JOIN dbo.Modulos AS modulo
            ON modulo.Id = acceso.ModuloId
        WHERE usuario.Id = @UsuarioId
          AND usuario.Activo = 1
          AND acceso.PuedeAcceder = 1
          AND modulo.Codigo = @ModuloCodigo
          AND modulo.Activo = 1
    ) THEN 1 ELSE 0 END) AS TieneAcceso;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        usuario.Id,
        usuario.Nombre,
        usuario.Apellidos,
        usuario.NumeroDocumento,
        usuario.Area,
        usuario.NombreUsuario,
        usuario.Activo,
        usuario.FechaCreacion,
        STRING_AGG(
            CASE WHEN acceso.PuedeAcceder = 1 THEN modulo.Codigo END,
            N',') AS ModulosHabilitados
    FROM dbo.Usuarios AS usuario
    LEFT JOIN dbo.UsuarioModulos AS acceso
        ON acceso.UsuarioId = usuario.Id
    LEFT JOIN dbo.Modulos AS modulo
        ON modulo.Id = acceso.ModuloId
    GROUP BY
        usuario.Id,
        usuario.Nombre,
        usuario.Apellidos,
        usuario.NumeroDocumento,
        usuario.Area,
        usuario.NombreUsuario,
        usuario.Activo,
        usuario.FechaCreacion
    ORDER BY usuario.Id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_CambiarEstado
    @UsuarioId INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Usuarios
    SET Activo = @Activo
    WHERE Id = @UsuarioId;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 51204, 'El usuario indicado no existe.', 1;
    END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_AsignarAccesoModulo
    @UsuarioId INT,
    @ModuloCodigo NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ModuloId INT =
    (
        SELECT Id
        FROM dbo.Modulos
        WHERE Codigo = @ModuloCodigo
    );

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Id = @UsuarioId)
    BEGIN
        THROW 51204, 'El usuario indicado no existe.', 1;
    END;

    IF @ModuloId IS NULL
    BEGIN
        THROW 51200, 'El módulo solicitado no existe.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.UsuarioModulos
        WHERE UsuarioId = @UsuarioId
          AND ModuloId = @ModuloId
    )
    BEGIN
        UPDATE dbo.UsuarioModulos
        SET PuedeAcceder = 1,
            FechaAsignacion = SYSUTCDATETIME()
        WHERE UsuarioId = @UsuarioId
          AND ModuloId = @ModuloId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.UsuarioModulos (UsuarioId, ModuloId, PuedeAcceder)
        VALUES (@UsuarioId, @ModuloId, 1);
    END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Usuarios_RevocarAccesoModulo
    @UsuarioId INT,
    @ModuloCodigo NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE acceso
    SET PuedeAcceder = 0
    FROM dbo.UsuarioModulos AS acceso
    INNER JOIN dbo.Modulos AS modulo
        ON modulo.Id = acceso.ModuloId
    WHERE acceso.UsuarioId = @UsuarioId
      AND modulo.Codigo = @ModuloCodigo;

    IF @@ROWCOUNT = 0
    BEGIN
        THROW 51205, 'El usuario no tiene asignado el módulo indicado.', 1;
    END;
END;
GO

PRINT N'Procedimientos de autenticación y autorización creados.';
GO
