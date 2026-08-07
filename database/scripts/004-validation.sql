SET NOCOUNT ON;
SET XACT_ABORT OFF;

USE [GestorInventarioDB];
GO

PRINT N'Iniciando validaciones de GestorInventarioDB...';

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    THROW 51100, 'Falta la tabla dbo.Categorias.', 1;
END;

IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
BEGIN
    THROW 51101, 'Falta la tabla dbo.Productos.', 1;
END;

DECLARE @ColumnasEsperadas TABLE
(
    Tabla SYSNAME NOT NULL,
    Columna SYSNAME NOT NULL,
    Tipo SYSNAME NOT NULL,
    LongitudMaxima SMALLINT NULL,
    PrecisionEsperada TINYINT NULL,
    EscalaEsperada TINYINT NULL,
    PermiteNull BIT NOT NULL,
    EsIdentity BIT NOT NULL
);

INSERT INTO @ColumnasEsperadas
(
    Tabla,
    Columna,
    Tipo,
    LongitudMaxima,
    PrecisionEsperada,
    EscalaEsperada,
    PermiteNull,
    EsIdentity
)
VALUES
    (N'Categorias', N'Id', N'int', NULL, NULL, NULL, 0, 1),
    (N'Categorias', N'Nombre', N'nvarchar', 200, NULL, NULL, 0, 0),
    (N'Categorias', N'Activo', N'bit', NULL, NULL, NULL, 0, 0),
    (N'Productos', N'Id', N'int', NULL, NULL, NULL, 0, 1),
    (N'Productos', N'Nombre', N'nvarchar', 300, NULL, NULL, 0, 0),
    (N'Productos', N'Descripcion', N'nvarchar', 1000, NULL, NULL, 1, 0),
    (N'Productos', N'Precio', N'decimal', NULL, 10, 2, 0, 0),
    (N'Productos', N'Stock', N'int', NULL, NULL, NULL, 0, 0),
    (N'Productos', N'StockMinimo', N'int', NULL, NULL, NULL, 0, 0),
    (N'Productos', N'CategoriaId', N'int', NULL, NULL, NULL, 0, 0),
    (N'Productos', N'FechaCreacion', N'datetime2', NULL, NULL, NULL, 0, 0);

IF EXISTS
(
    SELECT 1
    FROM @ColumnasEsperadas AS esperado
    LEFT JOIN sys.tables AS tabla
        ON tabla.name = esperado.Tabla
       AND SCHEMA_NAME(tabla.schema_id) = N'dbo'
    LEFT JOIN sys.columns AS columna
        ON columna.object_id = tabla.object_id
       AND columna.name = esperado.Columna
    LEFT JOIN sys.types AS tipo
        ON tipo.user_type_id = columna.user_type_id
    WHERE columna.object_id IS NULL
       OR tipo.name <> esperado.Tipo
       OR (esperado.LongitudMaxima IS NOT NULL
           AND columna.max_length <> esperado.LongitudMaxima)
       OR (esperado.PrecisionEsperada IS NOT NULL
           AND columna.precision <> esperado.PrecisionEsperada)
       OR (esperado.EscalaEsperada IS NOT NULL
           AND columna.scale <> esperado.EscalaEsperada)
       OR columna.is_nullable <> esperado.PermiteNull
       OR columna.is_identity <> esperado.EsIdentity
)
BEGIN
    SELECT
        esperado.Tabla,
        esperado.Columna,
        esperado.Tipo AS TipoEsperado,
        tipo.name AS TipoActual,
        esperado.LongitudMaxima,
        columna.max_length AS LongitudActual,
        esperado.PrecisionEsperada,
        columna.precision AS PrecisionActual,
        esperado.EscalaEsperada,
        columna.scale AS EscalaActual,
        esperado.PermiteNull,
        columna.is_nullable AS PermiteNullActual,
        esperado.EsIdentity,
        columna.is_identity AS EsIdentityActual
    FROM @ColumnasEsperadas AS esperado
    LEFT JOIN sys.tables AS tabla
        ON tabla.name = esperado.Tabla
       AND SCHEMA_NAME(tabla.schema_id) = N'dbo'
    LEFT JOIN sys.columns AS columna
        ON columna.object_id = tabla.object_id
       AND columna.name = esperado.Columna
    LEFT JOIN sys.types AS tipo
        ON tipo.user_type_id = columna.user_type_id;

    THROW 51102, 'La definicion de columnas no coincide con el contrato.', 1;
END;

PRINT N'OK - Tipos, longitudes, nullability e IDENTITY verificados.';

IF OBJECT_ID(N'dbo.PK_Categorias', N'PK') IS NULL
   OR OBJECT_ID(N'dbo.PK_Productos', N'PK') IS NULL
   OR OBJECT_ID(N'dbo.FK_Productos_Categorias', N'F') IS NULL
   OR OBJECT_ID(N'dbo.CK_Productos_Stock', N'C') IS NULL
   OR OBJECT_ID(N'dbo.CK_Productos_Precio', N'C') IS NULL
BEGIN
    THROW 51103, 'Falta una PK, FK o constraint CHECK requerido.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Productos_Categorias'
      AND (is_disabled = 1 OR is_not_trusted = 1)
)
BEGIN
    THROW 51104, 'La FK de productos esta deshabilitada o no es confiable.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name IN (N'CK_Productos_Stock', N'CK_Productos_Precio')
      AND (is_disabled = 1 OR is_not_trusted = 1)
)
BEGIN
    THROW 51105, 'Un constraint CHECK esta deshabilitado o no es confiable.', 1;
END;

PRINT N'OK - PK, FK y constraints CHECK verificados.';

DECLARE @DefaultsEsperados TABLE
(
    Tabla SYSNAME NOT NULL,
    Columna SYSNAME NOT NULL
);

INSERT INTO @DefaultsEsperados (Tabla, Columna)
VALUES
    (N'Categorias', N'Activo'),
    (N'Productos', N'Stock'),
    (N'Productos', N'StockMinimo'),
    (N'Productos', N'FechaCreacion');

IF EXISTS
(
    SELECT 1
    FROM @DefaultsEsperados AS esperado
    LEFT JOIN sys.tables AS tabla
        ON tabla.name = esperado.Tabla
       AND SCHEMA_NAME(tabla.schema_id) = N'dbo'
    LEFT JOIN sys.columns AS columna
        ON columna.object_id = tabla.object_id
       AND columna.name = esperado.Columna
    LEFT JOIN sys.default_constraints AS defecto
        ON defecto.parent_object_id = columna.object_id
       AND defecto.parent_column_id = columna.column_id
    WHERE defecto.object_id IS NULL
)
BEGIN
    THROW 51106, 'Falta un default requerido.', 1;
END;

PRINT N'OK - Defaults requeridos verificados.';

DECLARE @CategoriasEsperadas TABLE
(
    Nombre NVARCHAR(100) NOT NULL
);

INSERT INTO @CategoriasEsperadas (Nombre)
VALUES
    (N'Electrónica'),
    (N'Oficina'),
    (N'Aseo');

IF EXISTS
(
    SELECT esperado.Nombre
    FROM @CategoriasEsperadas AS esperado
    LEFT JOIN dbo.Categorias AS categoria
        ON categoria.Nombre = esperado.Nombre
    GROUP BY esperado.Nombre
    HAVING COUNT(categoria.Id) <> 1
)
BEGIN
    THROW 51107, 'Las categorias seed faltan o estan duplicadas.', 1;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Productos)
BEGIN
    THROW 51108, 'No existen productos seed.', 1;
END;

IF EXISTS
(
    SELECT seed.Nombre
    FROM
    (
        VALUES
            (N'Audífonos Bluetooth'),
            (N'Resma papel carta'),
            (N'Limpiador multiusos')
    ) AS seed (Nombre)
    LEFT JOIN dbo.Productos AS producto
        ON producto.Nombre = seed.Nombre
    GROUP BY seed.Nombre
    HAVING COUNT(producto.Id) <> 1
)
BEGIN
    THROW 51109, 'Los productos seed faltan o estan duplicados.', 1;
END;

PRINT N'OK - Categorias y productos seed verificados.';
GO

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO dbo.Categorias (Nombre)
    VALUES (N'__VALIDACION_DEFAULTS__');

    DECLARE @CategoriaTemporalId INT = CONVERT(INT, SCOPE_IDENTITY());

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Categorias
        WHERE Id = @CategoriaTemporalId
          AND Activo = 1
    )
    BEGIN
        THROW 51110, 'El default de Categorias.Activo no produce 1.', 1;
    END;

    INSERT INTO dbo.Productos (Nombre, Precio, CategoriaId)
    VALUES (N'__VALIDACION_DEFAULTS__', 1.00, @CategoriaTemporalId);

    DECLARE @ProductoTemporalId INT = CONVERT(INT, SCOPE_IDENTITY());

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Productos
        WHERE Id = @ProductoTemporalId
          AND Stock = 0
          AND StockMinimo = 5
          AND FechaCreacion IS NOT NULL
    )
    BEGIN
        THROW 51111, 'Los defaults de Productos no coinciden con el contrato.', 1;
    END;

    ROLLBACK TRANSACTION;
    PRINT N'OK - Valores de defaults verificados.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CategoriaStockId INT =
    (
        SELECT MIN(Id)
        FROM dbo.Categorias
    );

    INSERT INTO dbo.Productos
    (
        Nombre,
        Precio,
        Stock,
        StockMinimo,
        CategoriaId
    )
    VALUES
    (
        N'__VALIDACION_STOCK_NEGATIVO__',
        1.00,
        -1,
        5,
        @CategoriaStockId
    );

    ROLLBACK TRANSACTION;
    THROW 51112, 'Stock negativo fue aceptado indebidamente.', 1;
END TRY
BEGIN CATCH
    DECLARE @StockError INT = ERROR_NUMBER();
    DECLARE @StockMensaje NVARCHAR(4000) = ERROR_MESSAGE();

    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    IF @StockError = 547
       AND @StockMensaje LIKE N'%CK_Productos_Stock%'
    BEGIN
        PRINT N'OK - Stock negativo rechazado por CK_Productos_Stock.';
    END
    ELSE
    BEGIN
        THROW;
    END;
END CATCH;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO dbo.Productos
    (
        Nombre,
        Precio,
        Stock,
        StockMinimo,
        CategoriaId
    )
    VALUES
    (
        N'__VALIDACION_CATEGORIA_INEXISTENTE__',
        1.00,
        0,
        5,
        2147483647
    );

    ROLLBACK TRANSACTION;
    THROW 51113, 'Categoria inexistente fue aceptada indebidamente.', 1;
END TRY
BEGIN CATCH
    DECLARE @FkError INT = ERROR_NUMBER();
    DECLARE @FkMensaje NVARCHAR(4000) = ERROR_MESSAGE();

    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    IF @FkError = 547
       AND @FkMensaje LIKE N'%FK_Productos_Categorias%'
    BEGIN
        PRINT N'OK - Categoria inexistente rechazada por FK_Productos_Categorias.';
    END
    ELSE
    BEGIN
        THROW;
    END;
END CATCH;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CategoriaPrecioId INT =
    (
        SELECT MIN(Id)
        FROM dbo.Categorias
    );

    INSERT INTO dbo.Productos
    (
        Nombre,
        Precio,
        Stock,
        StockMinimo,
        CategoriaId
    )
    VALUES
    (
        N'__VALIDACION_PRECIO_NO_POSITIVO__',
        0.00,
        0,
        5,
        @CategoriaPrecioId
    );

    ROLLBACK TRANSACTION;
    THROW 51114, 'Precio no positivo fue aceptado indebidamente.', 1;
END TRY
BEGIN CATCH
    DECLARE @PrecioError INT = ERROR_NUMBER();
    DECLARE @PrecioMensaje NVARCHAR(4000) = ERROR_MESSAGE();

    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    IF @PrecioError = 547
       AND @PrecioMensaje LIKE N'%CK_Productos_Precio%'
    BEGIN
        PRINT N'OK - Precio no positivo rechazado por CK_Productos_Precio.';
    END
    ELSE
    BEGIN
        THROW;
    END;
END CATCH;
GO

SELECT
    Id,
    Nombre,
    Activo
FROM dbo.Categorias
ORDER BY Id;

SELECT
    producto.Id,
    producto.Nombre,
    producto.Precio,
    producto.Stock,
    producto.StockMinimo,
    categoria.Nombre AS Categoria,
    producto.FechaCreacion,
    CONVERT(BIT, CASE
        WHEN producto.Stock < producto.StockMinimo THEN 1
        ELSE 0
    END) AS EsStockBajo
FROM dbo.Productos AS producto
INNER JOIN dbo.Categorias AS categoria
    ON categoria.Id = producto.CategoriaId
ORDER BY producto.Id;

PRINT N'VALIDACION COMPLETA - GestorInventarioDB cumple el contrato P0.';
GO
