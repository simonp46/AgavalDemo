SET NOCOUNT ON;

USE [master];
GO

IF DB_ID(N'GestorInventarioDB') IS NULL
BEGIN
    PRINT N'Creando base de datos GestorInventarioDB...';
    CREATE DATABASE [GestorInventarioDB];
END
ELSE
BEGIN
    PRINT N'La base de datos GestorInventarioDB ya existe.';
END;
GO

USE [GestorInventarioDB];
GO

PRINT N'Base de datos GestorInventarioDB disponible.';
GO
