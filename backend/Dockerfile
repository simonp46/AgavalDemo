# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/GestorInventario.Domain/GestorInventario.Domain.csproj src/GestorInventario.Domain/
COPY src/GestorInventario.Application/GestorInventario.Application.csproj src/GestorInventario.Application/
COPY src/GestorInventario.Infrastructure/GestorInventario.Infrastructure.csproj src/GestorInventario.Infrastructure/
COPY src/GestorInventario.Api/GestorInventario.Api.csproj src/GestorInventario.Api/

RUN --mount=type=secret,id=corporate_ca,required=false \
    if [ -s /run/secrets/corporate_ca ]; then \
        cp /run/secrets/corporate_ca /usr/local/share/ca-certificates/corporate-ca.crt; \
        update-ca-certificates; \
    fi \
    && dotnet restore src/GestorInventario.Api/GestorInventario.Api.csproj

COPY src/ src/

RUN dotnet publish src/GestorInventario.Api/GestorInventario.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=12 \
    CMD curl --fail --silent --show-error http://localhost:8080/health > /dev/null || exit 1

ENTRYPOINT ["dotnet", "GestorInventario.Api.dll"]
