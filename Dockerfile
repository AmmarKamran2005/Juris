# =============================================================
# Juris — multi-stage Docker build for Render / any Docker host
# =============================================================

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj/sln first for better layer caching
COPY ["global.json", "./"]
COPY ["Juris.sln", "./"]
COPY ["src/Juris.Domain/Juris.Domain.csproj", "src/Juris.Domain/"]
COPY ["src/Juris.Application/Juris.Application.csproj", "src/Juris.Application/"]
COPY ["src/Juris.Infrastructure/Juris.Infrastructure.csproj", "src/Juris.Infrastructure/"]
COPY ["src/Juris.Web/Juris.Web.csproj", "src/Juris.Web/"]
COPY ["src/Juris.Web.Client/Juris.Web.Client.csproj", "src/Juris.Web.Client/"]
COPY ["tests/Juris.Tests/Juris.Tests.csproj", "tests/Juris.Tests/"]

RUN dotnet restore "src/Juris.Web/Juris.Web.csproj"

# Copy the rest and publish
COPY . .
RUN dotnet publish "src/Juris.Web/Juris.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render injects PORT — bind Kestrel to it
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check — Render hits this to determine readiness
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:${PORT:-8080}/health || exit 1

EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Juris.Web.dll"]
