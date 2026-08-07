SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Modulos
        WHERE Codigo = N'PRODUCTOS'
    )
    BEGIN
        INSERT INTO dbo.Modulos (Codigo, Nombre, Activo)
        VALUES (N'PRODUCTOS', N'Gestión de productos', 1);
    END;

    COMMIT TRANSACTION;
    PRINT N'Seed de módulos aplicado.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO
