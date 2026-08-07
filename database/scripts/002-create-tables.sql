SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Categorias
        (
            Id INT IDENTITY(1,1) NOT NULL,
            Nombre NVARCHAR(100) NOT NULL,
            Activo BIT NOT NULL
                CONSTRAINT DF_Categorias_Activo DEFAULT (1),
            CONSTRAINT PK_Categorias PRIMARY KEY (Id)
        );

        PRINT N'Tabla dbo.Categorias creada.';
    END
    ELSE
    BEGIN
        PRINT N'La tabla dbo.Categorias ya existe.';
    END;

    IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Productos
        (
            Id INT IDENTITY(1,1) NOT NULL,
            Nombre NVARCHAR(150) NOT NULL,
            Descripcion NVARCHAR(500) NULL,
            Precio DECIMAL(10,2) NOT NULL,
            Stock INT NOT NULL
                CONSTRAINT DF_Productos_Stock DEFAULT (0),
            StockMinimo INT NOT NULL
                CONSTRAINT DF_Productos_StockMinimo DEFAULT (5),
            CategoriaId INT NOT NULL,
            FechaCreacion DATETIME2 NOT NULL
                CONSTRAINT DF_Productos_FechaCreacion DEFAULT (GETDATE()),
            CONSTRAINT PK_Productos PRIMARY KEY (Id),
            CONSTRAINT FK_Productos_Categorias
                FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias (Id),
            CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0),
            CONSTRAINT CK_Productos_Precio CHECK (Precio > 0)
        );

        PRINT N'Tabla dbo.Productos creada.';
    END
    ELSE
    BEGIN
        PRINT N'La tabla dbo.Productos ya existe.';
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
