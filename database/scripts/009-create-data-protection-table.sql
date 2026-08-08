SET NOCOUNT ON;
SET XACT_ABORT ON;

USE [GestorInventarioDB];
GO

IF OBJECT_ID(N'dbo.DataProtectionKeys', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DataProtectionKeys
    (
        Id INT IDENTITY(1,1) NOT NULL,
        FriendlyName NVARCHAR(MAX) NULL,
        Xml NVARCHAR(MAX) NULL,
        CONSTRAINT PK_DataProtectionKeys PRIMARY KEY (Id)
    );

    PRINT N'Tabla dbo.DataProtectionKeys creada.';
END
ELSE
BEGIN
    PRINT N'La tabla dbo.DataProtectionKeys ya existe.';
END;
GO

