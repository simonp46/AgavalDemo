# Base de datos

Scripts SQL Server para crear y validar `GestorInventarioDB`.

## Alcance

La implementacion contiene:

- `dbo.Categorias`.
- `dbo.Productos`.
- Seed de categorias y productos de ejemplo.
- Constraints de stock, precio e integridad referencial.
- `dbo.Usuarios`, `dbo.Modulos` y `dbo.UsuarioModulos` para autenticacion y
  permisos del modulo `PRODUCTOS`.
- Procedimientos almacenados de registro, login, consulta y administracion de
  acceso.

`MovimientosInventario` no esta implementada porque pertenece al alcance P2.

## Requisitos

- SQL Server o SQL Server LocalDB.
- `sqlcmd` o SQL Server Management Studio.
- Usuario con permiso para crear bases de datos.

Los archivos estan codificados en UTF-8. Con `sqlcmd` se usa `-f 65001` para
preservar `Electrónica` y los demas textos Unicode.

## Scripts

| Orden | Archivo | Proposito |
|---:|---|---|
| 1 | `scripts/001-create-database.sql` | Crea `GestorInventarioDB` si no existe. |
| 2 | `scripts/002-create-tables.sql` | Crea tablas, PK, FK, defaults y checks. |
| 3 | `scripts/003-seed.sql` | Inserta categorias y productos sin duplicar el seed. |
| 4 | `scripts/004-validation.sql` | Valida metadatos, datos y restricciones con pruebas transaccionales. |
| 5 | `scripts/005-create-auth-tables.sql` | Crea usuarios, modulos y permisos. |
| 6 | `scripts/006-auth-seed.sql` | Inserta el modulo `PRODUCTOS`. |
| 7 | `scripts/007-auth-stored-procedures.sql` | Crea los SP de autenticacion y administracion. |
| 8 | `scripts/008-auth-validation.sql` | Valida autenticacion, permisos y SP. |

Los scripts de creacion, seed y procedimientos pueden reejecutarse. Si ya
existe un esquema incompatible, los scripts de validacion fallan y se debe
corregir o reiniciar la base de datos.

## Crear la base

### SQL Server LocalDB

Desde la raiz del repositorio:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\001-create-database.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\002-create-tables.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\003-seed.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\004-validation.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\005-create-auth-tables.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\006-auth-seed.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\007-auth-stored-procedures.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\008-auth-validation.sql"
```

### SQL Server con autenticacion integrada

```powershell
sqlcmd -S "localhost" -E -C -f 65001 -b -i "database\scripts\001-create-database.sql"
sqlcmd -S "localhost" -E -C -f 65001 -b -i "database\scripts\002-create-tables.sql"
sqlcmd -S "localhost" -E -C -f 65001 -b -i "database\scripts\003-seed.sql"
```

Para autenticacion SQL se reemplaza `-E` por las opciones `-U` y `-P`
gestionadas de forma segura. No se deben guardar credenciales en el
repositorio.

En SQL Server Management Studio se abren y ejecutan los scripts en el mismo
orden.

## Validar

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\004-validation.sql"
```

La validacion termina con:

```text
VALIDACION COMPLETA - GestorInventarioDB cumple el contrato P0.
```

El script comprueba:

- Tablas y columnas obligatorias.
- Tipos, longitudes, nullability e `IDENTITY`.
- PK, FK, defaults y constraints `CHECK`.
- Categorias y productos seed.
- Valores efectivos de los defaults.
- Rechazo de stock negativo.
- Rechazo de una categoria inexistente.
- Rechazo de precio no positivo.

Las pruebas negativas se ejecutan dentro de transacciones y no dejan filas de
validacion.

## Consultar datos

Los usuarios pueden consultarse con `dbo.usp_Usuarios_Listar`. Los cambios de
estado y permisos deben realizarse con los procedimientos
`dbo.usp_Usuarios_CambiarEstado`, `dbo.usp_Usuarios_AsignarAccesoModulo` y
`dbo.usp_Usuarios_RevocarAccesoModulo`; `PasswordHash` no se modifica a mano.

```sql
USE [GestorInventarioDB];

SELECT Id, Nombre, Activo
FROM dbo.Categorias
ORDER BY Id;

SELECT
    producto.Id,
    producto.Nombre,
    producto.Precio,
    producto.Stock,
    producto.StockMinimo,
    categoria.Nombre AS Categoria,
    producto.FechaCreacion
FROM dbo.Productos AS producto
INNER JOIN dbo.Categorias AS categoria
    ON categoria.Id = producto.CategoriaId
ORDER BY producto.Id;
```

## Reiniciar la base

Advertencia: este procedimiento elimina todos los datos de
`GestorInventarioDB`. Debe usarse solo en un entorno local o desechable.

Ejecutar en `master`:

```sql
USE [master];

IF DB_ID(N'GestorInventarioDB') IS NOT NULL
BEGIN
    ALTER DATABASE [GestorInventarioDB]
        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [GestorInventarioDB];
END;
```

Despues se vuelven a ejecutar los scripts `001` a `008` en ese orden.

## Compatibilidad con EF Core

El script SQL es la fuente operativa P0 del esquema:

- EF Core debe usar configuraciones explicitas para reproducir nombres,
  tipos, longitudes, defaults y constraints.
- El backend debe conectarse a una base previamente provisionada con estos
  scripts.
- No se debe usar `EnsureCreated()` porque evita un historial controlado y
  puede divergir del contrato.
- No se deben aplicar migraciones automaticas junto con los scripts P0.

Si el `ORCHESTRATOR_AGENT` aprueba migraciones posteriormente, la estrategia
compatible es crear una migracion baseline revisada para una base ya
existente, conservar el model snapshot alineado y generar cambios futuros
desde una unica fuente. La baseline no debe volver a crear las tablas ni
modificar una instalacion provisionada con estos scripts.
