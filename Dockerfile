# syntax=docker/dockerfile:1
# =====================================================================
#  EcoWaste API - Dockerfile multi-stage (.NET 8)
#  Estágio 1 (build): restaura dependências, compila e publica em Release
#  Estágio 2 (final): imagem enxuta só com o runtime ASP.NET + app publicada
# =====================================================================

# ---------- Estágio 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copia primeiro só os arquivos de projeto -> aproveita cache do "restore"
COPY src/EcoWaste.Api/EcoWaste.Api.csproj src/EcoWaste.Api/
RUN dotnet restore src/EcoWaste.Api/EcoWaste.Api.csproj

# Copia o restante do código e publica
COPY src/ src/
RUN dotnet publish src/EcoWaste.Api/EcoWaste.Api.csproj \
      -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Estágio 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# curl é usado pelo HEALTHCHECK
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Versão da imagem (preenchida pelo pipeline com o SHA do commit)
ARG APP_VERSION=local
ENV APP_VERSION=${APP_VERSION} \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Executa como usuário não-root (já existe nas imagens oficiais .NET 8)
USER app

HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=5 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EcoWaste.Api.dll"]
