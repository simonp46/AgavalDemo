SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CategoriasSeed TABLE
    (
        Nombre NVARCHAR(100) NOT NULL
    );

    INSERT INTO @CategoriasSeed (Nombre)
    VALUES
        (N'Electrónica'),
        (N'Oficina'),
        (N'Aseo');

    INSERT INTO dbo.Categorias (Nombre)
    SELECT seed.Nombre
    FROM @CategoriasSeed AS seed
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categorias AS categoria
        WHERE categoria.Nombre = seed.Nombre
    );

    DECLARE @ElectronicaId INT =
    (
        SELECT MIN(Id)
        FROM dbo.Categorias
        WHERE Nombre = N'Electrónica'
    );

    DECLARE @OficinaId INT =
    (
        SELECT MIN(Id)
        FROM dbo.Categorias
        WHERE Nombre = N'Oficina'
    );

    DECLARE @AseoId INT =
    (
        SELECT MIN(Id)
        FROM dbo.Categorias
        WHERE Nombre = N'Aseo'
    );

    IF @ElectronicaId IS NULL OR @OficinaId IS NULL OR @AseoId IS NULL
    BEGIN
        THROW 51000, 'No fue posible resolver las categorias requeridas para el seed.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Productos
        WHERE Nombre = N'Audífonos Bluetooth'
    )
    BEGIN
        INSERT INTO dbo.Productos
        (
            Nombre,
            Descripcion,
            Precio,
            Stock,
            StockMinimo,
            CategoriaId
        )
        VALUES
        (
            N'Audífonos Bluetooth',
            N'Audífonos inalámbricos con micrófono.',
            199900.00,
            3,
            5,
            @ElectronicaId
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Productos
        WHERE Nombre = N'Resma papel carta'
    )
    BEGIN
        INSERT INTO dbo.Productos
        (
            Nombre,
            Descripcion,
            Precio,
            Stock,
            StockMinimo,
            CategoriaId
        )
        VALUES
        (
            N'Resma papel carta',
            N'Papel blanco tamaño carta, 500 hojas.',
            28000.00,
            20,
            5,
            @OficinaId
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Productos
        WHERE Nombre = N'Limpiador multiusos'
    )
    BEGIN
        INSERT INTO dbo.Productos
        (
            Nombre,
            Descripcion,
            Precio,
            Stock,
            StockMinimo,
            CategoriaId
        )
        VALUES
        (
            N'Limpiador multiusos',
            N'Limpiador líquido para superficies.',
            15000.00,
            12,
            4,
            @AseoId
        );
    END;

    COMMIT TRANSACTION;
    PRINT N'Seed de categorias y productos aplicado.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO
