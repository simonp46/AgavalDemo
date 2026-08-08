# Gestor de Inventario

Aplicación full stack para administrar productos y categorías, controlar
existencias y detectar productos con stock bajo. El alcance implementado
incluye listado y filtros, creación, edición, eliminación, consulta por
identificador y ajustes de stock por entrada o salida.
El módulo de productos está protegido por login, registro de usuarios y
permisos persistidos en SQL Server.

El proyecto implementa contratos REST explícitos, respuestas `ProblemDetails`
y control de acceso por sesión sin exponer entidades de dominio.

## 1. Descripción

La aplicación permite:

- Consultar productos y filtrar por categoría o estado de stock.
- Crear y editar productos con validaciones de negocio.
- Eliminar productos con confirmación previa.
- Registrar entradas y salidas de stock sin permitir existencias negativas.
- Identificar stock bajo cuando `stock < stockMinimo`.
- Consultar el catálogo precargado de categorías.
- Consumir errores HTTP uniformes mediante `ProblemDetails`.
- Registrar usuarios, iniciar/cerrar sesión y proteger el módulo
  `PRODUCTOS` mediante permisos revalidados en la base de datos.

El alcance principal está implementado e integrado de extremo a extremo.

## 2. Arquitectura

### Backend

El Backend aplica Clean Architecture y CQRS:

```text
HTTP
  |
  v
API ---------------> Application ---------------> Domain
 |                         ^                         ^
 |                         |                         |
 `-> Infrastructure ------'-------------------------'
          |
          v
      SQL Server
```

- **Domain** contiene `Producto`, `Categoria` y sus invariantes. No depende de
  ASP.NET Core, EF Core ni otras capas.
- **Application** define Commands, Queries, Handlers, DTOs, validaciones y
  puertos de persistencia.
- **Infrastructure** implementa los repositorios y `PersistenceContext` con EF
  Core y SQL Server.
- **API** actúa como composition root, recibe HTTP, delega mediante MediatR y
  transforma resultados a contratos REST.

Las dependencias permitidas son:

```text
Application -> Domain
Infrastructure -> Application -> Domain
API -> Application
API -> Infrastructure (solo composición)
```

### Frontend

Angular está organizado por responsabilidades:

```text
UI -> ProductosStore -> ProductosApiService -> API REST
       Signals             HttpClient
```

La feature `/productos` se carga de forma diferida. Sus páginas y componentes
son standalone y `OnPush`; el estado mutable permanece encapsulado en un store
de Signals y los valores derivados usan `computed()`.

## 3. Stack

| Área | Tecnología implementada |
|---|---|
| Backend | .NET SDK `10.0.302`, ASP.NET Core `10.0.10` |
| Persistencia | Entity Framework Core SQL Server `10.0.10`, scripts y SP SQL Server |
| Arquitectura Backend | Clean Architecture, CQRS, MediatR `12.5.0` |
| Documentación API | Swagger / OpenAPI con Swashbuckle `10.2.3` |
| Pruebas Backend | xUnit `2.9.3`, ASP.NET Core MVC Testing |
| Base de datos | SQL Server; validado con LocalDB `15.0.4382.1` |
| Frontend | Angular `20.3.27`, TypeScript `5.9.3`, RxJS `7.8` |
| UI | Angular Material y CDK `20.2.14` |
| Estado y renderizado | Signals, `computed()`, OnPush y zoneless |
| Seguridad | Cookie `HttpOnly`, PBKDF2-SHA512 y permisos por módulo |
| Formularios | Reactive Forms estrictamente tipados |
| Estilos | SCSS con BEM |
| Pruebas Frontend | Jasmine, Karma y Chrome |
| Contenedores | Docker, Docker Compose, Nginx y SQL Server 2022 |
| Integración continua | GitHub Actions con jobs Build y Test |

El entorno usado para comprobar los comandos utiliza Node.js `22.13.1` y npm
`10.9.2`.

## 4. Estructura

```text
/
|-- .env.example
|-- docker-compose.yml
|-- .github/workflows/ci.yml
|-- database/
|   |-- README.md
|   |-- docker-entrypoint.sh
|   `-- scripts/
|       |-- 001-create-database.sql
|       |-- 002-create-tables.sql
|       |-- 003-seed.sql
|       |-- 004-validation.sql
|       |-- 005-create-auth-tables.sql
|       |-- 006-auth-seed.sql
|       |-- 007-auth-stored-procedures.sql
|       `-- 008-auth-validation.sql
|-- backend/
|   |-- Dockerfile
|   |-- GestorInventario.sln
|   |-- src/
|   |   |-- GestorInventario.Domain/
|   |   |-- GestorInventario.Application/
|   |   |-- GestorInventario.Infrastructure/
|   |   `-- GestorInventario.Api/
|   `-- tests/
|       |-- GestorInventario.Domain.Tests/
|       |-- GestorInventario.Application.Tests/
|       `-- GestorInventario.Api.Tests/
|-- frontend/
|   |-- Dockerfile
|   |-- nginx.conf
|   `-- src/app/
|       |-- core/
|       |-- shared/
|       `-- features/
|           |-- auth/
|           `-- productos/
```

## 5. Requisitos

Para ejecutar el proyecto localmente se necesita:

- Git.
- .NET SDK `10.0.302` o un parche posterior compatible con `global.json`.
- Node.js y npm; las versiones comprobadas son `22.13.1` y `10.9.2`.
- SQL Server o SQL Server LocalDB.
- `sqlcmd`, incluido en las herramientas de línea de comandos de SQL Server.
- Chrome para ejecutar la suite Frontend con Karma.
- Docker Engine o Docker Desktop con Compose v2 para la ejecución en
  contenedores.

Los comandos siguientes usan PowerShell y parten de la raíz del repositorio.

## 6. Configuración de SQL Server

La configuración predeterminada usa autenticación integrada y LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;
Database=GestorInventarioDB;
Trusted_Connection=True;
TrustServerCertificate=True;
```

Crear y validar la base desde la raíz:

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

La última ejecución debe finalizar con:

```text
VALIDACION COMPLETA - GestorInventarioDB cumple el contrato P0.
```

Los scripts crean inventario, usuarios, módulos y permisos; insertan las
categorías Electrónica, Oficina y Aseo, agregan productos de ejemplo y validan
restricciones, autenticación y acceso. Las consultas y operaciones de base de
datos están descritas en [`database/README.md`](database/README.md).

Para otra instancia SQL Server, la cadena puede configurarse sin modificar
archivos:

```powershell
$env:ConnectionStrings__GestorInventarioDb = "Server=localhost;Database=GestorInventarioDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

No se deben guardar usuarios o contraseñas en el repositorio. El procedimiento
de reinicio de la base está documentado en
[`database/README.md`](database/README.md).

## 7. Configuración Backend

La configuración predeterminada está en
`backend/src/GestorInventario.Api/appsettings.json`:

- `ConnectionStrings:GestorInventarioDb`: conexión SQL Server.
- `Cors:AllowedOrigins`: contiene `http://localhost:4200`.
- `Logging`: niveles de log de ASP.NET Core.

Los valores pueden sobrescribirse mediante variables de entorno:

```powershell
$env:ConnectionStrings__GestorInventarioDb = "Server=(localdb)\MSSQLLocalDB;Database=GestorInventarioDB;Trusted_Connection=True;TrustServerCertificate=True;"
$env:Cors__AllowedOrigins__0 = "http://localhost:4200"
```

Restaurar y compilar:

```powershell
Set-Location backend
dotnet restore
dotnet build --configuration Release --no-restore
```

El perfil HTTP de desarrollo publica la API en `http://localhost:5186`.

## 8. Configuración Frontend

Instalar dependencias y generar el build:

```powershell
Set-Location frontend
npm install
npm run build
```

La configuración de URL del API es:

| Ambiente | Archivo | URL |
|---|---|---|
| Desarrollo | `environment.development.ts` | `http://localhost:5186/api` |
| Producción | `environment.ts` | `/api` |

El build de producción presupone que el API se publica bajo el mismo origen o
que un reverse proxy enruta `/api` al Backend. No existe un proxy de desarrollo
en el repositorio.

La aplicación configura `provideRouter()`, `provideHttpClient()` y
`provideZonelessChangeDetection()` en `app.config.ts`; `zone.js` no es una
dependencia funcional.

## 9. Migraciones

Este repositorio **no contiene migraciones EF Core**. Los scripts de
`database/scripts/` son la única fuente operativa P0 para crear y validar el
esquema.

Por esta razón:

- No se debe ejecutar `dotnet ef database update`.
- El Backend presupone una base previamente provisionada.
- No se usa `EnsureCreated()` ni `Database.Migrate()` al iniciar.
- Las configuraciones EF Core reproducen el esquema, pero no lo crean.

Si en el futuro se aprueba una estrategia de migraciones, deberá partir de una
baseline que reconozca la base existente y evite recrear `Categorias` y
`Productos`. Esa estrategia no está implementada actualmente.

## 10. Cómo ejecutar localmente

### Terminal 1 - Backend

Después de provisionar la base y compilar:

```powershell
Set-Location backend
dotnet run --project src/GestorInventario.Api --configuration Release --no-build --launch-profile http
```

### Terminal 2 - Frontend

```powershell
Set-Location frontend
$env:NG_CLI_ANALYTICS = "false"
npm start
```

Abrir:

- Aplicación/login: `http://localhost:4200/login`
- Productos: `http://localhost:4200/productos` (requiere sesión)
- API: `http://localhost:5186/api/productos`
- Swagger: `http://localhost:5186/swagger`

`NG_CLI_ANALYTICS=false` evita preguntas interactivas del CLI en terminales o
entornos automatizados; no altera la aplicación.

## Ejecución con Docker

La composición levanta tres servicios en una red privada:

```text
frontend (Nginx :80)
  -> backend (ASP.NET Core :8080)
    -> sqlserver (SQL Server :1433)
```

El Frontend sirve la SPA y proxifica `/api` hacia el Backend. SQL Server
ejecuta los scripts `001` a `008` antes de quedar healthy; el Backend espera
ese healthcheck y el Frontend espera al Backend.

Crear la configuración local:

```powershell
Copy-Item .env.example .env
```

Cambiar obligatoriamente `MSSQL_SA_PASSWORD` en `.env`. El valor del ejemplo es
solo un placeholder y `.env` está excluido de Git.

Validar y construir:

```powershell
docker compose config --quiet
docker compose build
```

Si una red corporativa inspecciona TLS, el build puede requerir su certificado
raíz en formato PEM. Debe guardarse localmente como
`.docker/corporate-ca.crt` y habilitarse el override:

```powershell
New-Item -ItemType Directory -Force .docker
# Ubicar en .docker/corporate-ca.crt la CA pública aprobada por la organización.
Copy-Item docker-compose.corporate-ca.yml.example docker-compose.override.yml
docker compose build
```

`.docker/` y `docker-compose.override.yml` están excluidos de Git. La CA se
monta como secreto BuildKit únicamente durante `dotnet restore` y `npm ci`; no
queda incluida en las imágenes finales. No debe usarse esta alternativa para
desactivar la validación TLS.

Iniciar en segundo plano:

```powershell
docker compose up --detach
docker compose ps
```

Abrir:

- Aplicación/login: `http://localhost:4200/login`
- Productos: `http://localhost:4200/productos` (requiere sesión)
- API: `http://localhost:8080/api/productos`
- Swagger: `http://localhost:8080/swagger`
- SQL Server: `localhost,1433`

En el primer acceso, usar `Registrarse`, completar los datos y luego iniciar
sesión con el usuario sugerido o elegido. El registro asigna acceso al módulo
`PRODUCTOS`.

Consultar logs:

```powershell
docker compose logs --follow
```

Detener los contenedores conservando los datos:

```powershell
docker compose down
```

Reiniciar completamente la base Docker elimina el volumen y todos sus datos:

```powershell
docker compose down --volumes
```

Los puertos pueden cambiarse en `.env`. La composición usa
`ASPNETCORE_ENVIRONMENT=Development` para el entorno local, lo que mantiene
Swagger disponible y evita exigir terminación TLS dentro del contenedor. Un
Release debe usar configuración propia, HTTPS en el borde y secretos
administrados por la plataforma.

## 11. Swagger

Swagger está habilitado únicamente cuando
`ASPNETCORE_ENVIRONMENT=Development`, valor configurado por los perfiles de
`launchSettings.json`.

Con el perfil `http`:

```text
http://localhost:5186/swagger
```

La especificación describe requests, responses, códigos HTTP y respuestas de
error `application/problem+json`.

## 12. Endpoints principales

Base local: `http://localhost:5186/api`.

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/auth/login` | Valida credenciales e inicia una sesión por cookie. |
| `POST` | `/auth/registro` | Registra un usuario con acceso a `PRODUCTOS`. |
| `GET` | `/auth/sugerencia-usuario` | Sugiere un usuario disponible. |
| `GET` | `/auth/sesion` | Obtiene la sesión autenticada. |
| `POST` | `/auth/logout` | Cierra la sesión. |
| `GET` | `/productos` | Lista productos; admite `categoriaId` y `estadoStock`. |
| `GET` | `/productos/{id}` | Obtiene un producto. |
| `POST` | `/productos` | Crea un producto. |
| `PUT` | `/productos/{id}` | Reemplaza los campos editables. |
| `DELETE` | `/productos/{id}` | Elimina físicamente un producto. |
| `GET` | `/productos/stock-bajo` | Lista productos con `stock < stockMinimo`. |
| `PATCH` | `/productos/{id}/stock` | Ajusta stock con `ENTRADA` o `SALIDA`. |
| `GET` | `/categorias` | Lista el catálogo de categorías. |

Valores del filtro `estadoStock`:

- `bajo`
- `normal`

Los errores siguen el contrato `ProblemDetails` y usan los códigos HTTP `400`,
`401`, `403`, `404`, `409` y `500` según corresponda.

## 13. Cómo ejecutar tests

### Backend

```powershell
Set-Location backend
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build --no-restore
```

Resultado comprobado: `31/31 PASS`.

### Frontend

```powershell
Set-Location frontend
npm install
npm run build
npm test -- --watch=false
```

Resultado comprobado: `21/21 PASS`.

El proyecto Frontend no tiene un script de lint configurado.

### Base de datos

Desde la raíz:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\004-validation.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -f 65001 -b -i "database\scripts\008-auth-validation.sql"
```

Resultado comprobado: validación P0 completa.

## Integración continua

El workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) se ejecuta en
push a `main`/`master` y en pull requests.

El flujo mantiene dos jobs secuenciales:

1. **Build:** restore y build de Backend, `npm ci`, build de Frontend,
   validación de Compose y construcción de imágenes.
2. **Test:** tests Backend y tests Frontend con Chrome Headless.

`Test` depende de `Build`, por lo que una compilación fallida impide ejecutar
la segunda etapa. El workflow usa únicamente una contraseña efímera de CI para
validar la interpolación de Compose.

Release y Deploy permanecen separados del CI. El workflow
[`.github/workflows/deploy.yml`](.github/workflows/deploy.yml) solo se activa
después de que `CI` finaliza correctamente sobre `main`, o mediante una
ejecución manual explícita desde `main`.

## Despliegue gratuito automatizado

La entrega cloud mantiene el stack SQL Server y distribuye los contenedores de
esta forma:

1. **Vercel Hobby:** ejecuta `frontend/Dockerfile.vercel`, sirve Angular con
   Nginx y reenvía `/api` al Backend sin exponer otra URL al navegador.
2. **Azure Container Apps Consumption:** ejecuta la imagen .NET publicada en
   `ghcr.io/simonp46/agavaldemo-backend` y escala a cero sin tráfico.
3. **Azure SQL Database Free:** conserva tablas, constraints, seed,
   procedimientos almacenados y claves de protección de las cookies.

Vercel no se conecta directamente a Azure SQL. Esta separación evita abrir la
base a las IP dinámicas de Vercel Hobby y mantiene las credenciales únicamente
en Azure Container Apps y GitHub Environments.

### Alta única en Azure

1. Crear una suscripción de Azure y una **Azure SQL Database Free** desde
   `Azure SQL hub > Start free`.
2. Usar exactamente `GestorInventarioDB` como nombre de base y seleccionar la
   opción que pausa la base al agotar la cuota gratuita.
3. En el servidor lógico, habilitar acceso de red pública para redes
   seleccionadas y `Allow Azure services and resources to access this server`.
4. Crear el grupo y las credenciales de automatización desde Azure Cloud Shell:

```bash
SUBSCRIPTION_ID=$(az account show --query id --output tsv)
az group create --name gestor-inventario-rg --location eastus2
az ad sp create-for-rbac \
  --name agavaldemo-github \
  --role Contributor \
  --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/gestor-inventario-rg" \
  --sdk-auth
```

La salida JSON completa del último comando se guarda como
`AZURE_CREDENTIALS`. La cadena de Azure SQL debe apuntar a
`Initial Catalog=GestorInventarioDB`, usar `Encrypt=True` y
`TrustServerCertificate=False`.

### Alta única en Vercel

Crear un proyecto Hobby vacío y vincular la carpeta `frontend`:

```powershell
npm.cmd install --global vercel
vercel.cmd login
vercel.cmd link --cwd frontend
```

Crear un token en Vercel y tomar `orgId` y `projectId` de
`frontend/.vercel/project.json`. La carpeta `.vercel` es configuración local y
no debe confirmarse en Git.

### Secretos de GitHub

Crear un PAT clásico de GitHub con permiso `read:packages` para que Azure pueda
descargar la imagen privada de GHCR. Después registrar estos secretos en el
environment `production` del repositorio:

```powershell
gh secret set AZURE_CREDENTIALS --env production
gh secret set AZURE_SQL_CONNECTION_STRING --env production
gh secret set GHCR_PULL_TOKEN --env production
gh secret set VERCEL_TOKEN --env production
gh secret set VERCEL_ORG_ID --env production
gh secret set VERCEL_PROJECT_ID --env production
```

Cada comando solicita el valor de forma interactiva y evita dejarlo en el
historial de la terminal. Los nombres predeterminados pueden sobrescribirse con
variables del repositorio:

```powershell
gh variable set AZURE_RESOURCE_GROUP --body gestor-inventario-rg
gh variable set AZURE_LOCATION --body eastus2
gh variable set AZURE_CONTAINERAPPS_ENVIRONMENT --body gestor-inventario-env
gh variable set AZURE_CONTAINER_APP --body gestor-inventario-api
```

### Publicar

El despliegue normal se inicia al aprobar y fusionar un Pull Request hacia
`main`. Para relanzarlo sin crear un commit:

```powershell
gh workflow run deploy.yml --ref main
```

El pipeline aplica scripts SQL idempotentes, publica una imagen Backend
versionada por SHA, crea o actualiza Container Apps, despliega el contenedor
Frontend en Vercel y valida `/health` y `/api/auth/sesion`.

## 14. Decisiones arquitectónicas

- **Clean Architecture:** Domain permanece independiente de frameworks y
  persistencia.
- **CQRS con MediatR:** cada lectura es una Query y cada escritura un Command.
- **Controllers delgados:** solo traducen HTTP y delegan mediante `ISender`.
- **Dependency Inversion:** Application define repositorios; Infrastructure
  los implementa con EF Core.
- **DTOs explícitos:** las entidades Domain no se serializan desde la API.
- **Errores uniformes:** el manejo global transforma fallos a `ProblemDetails`.
- **Consistencia de stock:** el ajuste usa transacción serializable y bloqueo
  de actualización para evitar salidas concurrentes que produzcan stock
  negativo.
- **Scripts SQL como fuente P0:** se evita mantener simultáneamente scripts y
  migraciones que puedan divergir.
- **Frontend por feature:** `core`, `shared` y `features/productos` separan
  capacidades transversales, reutilizables y funcionales.
- **Standalone, lazy y OnPush:** las rutas usan `loadChildren()` y
  `loadComponent()`; todos los componentes son standalone y `OnPush`.
- **Signals y zoneless:** el store expone estado readonly, actualiza con
  `set()/update()` y deriva datos con `computed()`.
- **Contrato único:** Frontend consume `esStockBajo` calculado por el Backend y
  no duplica la regla.
- **Autenticación desacoplada:** Application define los puertos de usuarios y
  hashing; Infrastructure implementa PBKDF2 y acceso a SP; API administra la
  cookie y los Controllers siguen delegando por MediatR.
- **Autorización vigente:** cada cookie se revalida contra el permiso
  `PRODUCTOS`, por lo que desactivar un usuario o revocar su módulo tiene efecto
  en la siguiente solicitud.

## 15. Limitaciones conocidas

- El registro es autoservicio y otorga acceso inicial a `PRODUCTOS`; no hay
  aprobación administrativa previa.
- No existe recuperación o cambio de contraseña ni UI administrativa de
  usuarios y permisos.
- No existe CRUD de categorías; solo consulta del catálogo.
- No se implementó `MovimientosInventario` ni historial de ajustes.
- La eliminación de productos es física.
- No hay paginación ni orden contractual para los listados.
- La base debe provisionarse con scripts antes de iniciar el Backend.
- Swagger solo está disponible en ambiente Development.
- El build Frontend de producción requiere enrutar `/api` hacia el Backend.
- No existe configuración de lint Frontend.
- La pantalla y el popup de autenticación están cubiertos indirectamente por
  tests del store, servicio y contratos API; no tienen pruebas visuales
  automatizadas dedicadas.
- `npm audit` reporta tres vulnerabilidades moderadas en tooling de Angular
  CLI, sin hallazgos altos o críticos.
- SQL Server 2022 para Linux publica imagen `amd64`; equipos ARM requieren
  emulación compatible.
- Vercel Hobby se limita a proyectos personales no comerciales y a sus cuotas
  mensuales.
- Container Apps y Azure SQL pueden presentar arranque en frío después de
  escalar o pausarse por inactividad.
- `Allow Azure services and resources` permite alcanzar el firewall desde
  recursos Azure externos; la autenticación SQL y los permisos siguen siendo
  obligatorios y deben usar credenciales fuertes.
- No se incluyen manifiestos Kubernetes; la entrega usa servicios serverless
  administrados y un pipeline Release/Deploy separado de CI.

## Documentación incluida

- [Operación de la base de datos](database/README.md)
- [Comandos de pruebas Backend](backend/tests/README.md)
- [Referencia del proyecto Angular](frontend/README.md)
