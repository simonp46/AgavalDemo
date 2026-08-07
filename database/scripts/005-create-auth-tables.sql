SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Usuarios
        (
            Id INT IDENTITY(1,1) NOT NULL,
            Nombre NVARCHAR(100) NOT NULL,
            Apellidos NVARCHAR(100) NOT NULL,
            NumeroDocumento NVARCHAR(30) NOT NULL,
            Area NVARCHAR(100) NOT NULL,
            NombreUsuario NVARCHAR(60) NOT NULL,
            PasswordHash NVARCHAR(512) NOT NULL,
            Activo BIT NOT NULL
                CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
            FechaCreacion DATETIME2 NOT NULL
                CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT PK_Usuarios PRIMARY KEY (Id),
            CONSTRAINT UQ_Usuarios_NumeroDocumento UNIQUE (NumeroDocumento),
            CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
            CONSTRAINT CK_Usuarios_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
            CONSTRAINT CK_Usuarios_Apellidos CHECK (LEN(LTRIM(RTRIM(Apellidos))) > 0),
            CONSTRAINT CK_Usuarios_NumeroDocumento CHECK
                (LEN(LTRIM(RTRIM(NumeroDocumento))) > 0),
            CONSTRAINT CK_Usuarios_Area CHECK (LEN(LTRIM(RTRIM(Area))) > 0),
            CONSTRAINT CK_Usuarios_NombreUsuario CHECK
                (LEN(LTRIM(RTRIM(NombreUsuario))) >= 4),
            CONSTRAINT CK_Usuarios_PasswordHash CHECK (LEN(PasswordHash) >= 32)
        );

        PRINT N'Tabla dbo.Usuarios creada.';
    END
    ELSE
    BEGIN
        PRINT N'La tabla dbo.Usuarios ya existe.';
    END;

    IF OBJECT_ID(N'dbo.Modulos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Modulos
        (
            Id INT IDENTITY(1,1) NOT NULL,
            Codigo NVARCHAR(50) NOT NULL,
            Nombre NVARCHAR(100) NOT NULL,
            Activo BIT NOT NULL
                CONSTRAINT DF_Modulos_Activo DEFAULT (1),
            CONSTRAINT PK_Modulos PRIMARY KEY (Id),
            CONSTRAINT UQ_Modulos_Codigo UNIQUE (Codigo),
            CONSTRAINT CK_Modulos_Codigo CHECK (LEN(LTRIM(RTRIM(Codigo))) > 0),
            CONSTRAINT CK_Modulos_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
        );

        PRINT N'Tabla dbo.Modulos creada.';
    END
    ELSE
    BEGIN
        PRINT N'La tabla dbo.Modulos ya existe.';
    END;

    IF OBJECT_ID(N'dbo.UsuarioModulos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.UsuarioModulos
        (
            UsuarioId INT NOT NULL,
            ModuloId INT NOT NULL,
            PuedeAcceder BIT NOT NULL
                CONSTRAINT DF_UsuarioModulos_PuedeAcceder DEFAULT (1),
            FechaAsignacion DATETIME2 NOT NULL
                CONSTRAINT DF_UsuarioModulos_FechaAsignacion DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT PK_UsuarioModulos PRIMARY KEY (UsuarioId, ModuloId),
            CONSTRAINT FK_UsuarioModulos_Usuarios
                FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (Id),
            CONSTRAINT FK_UsuarioModulos_Modulos
                FOREIGN KEY (ModuloId) REFERENCES dbo.Modulos (Id)
        );

        PRINT N'Tabla dbo.UsuarioModulos creada.';
    END
    ELSE
    BEGIN
        PRINT N'La tabla dbo.UsuarioModulos ya existe.';
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO
