# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["gateways/ApiGateway/ApiGateway.csproj", "gateways/ApiGateway/"]

RUN dotnet restore "gateways/ApiGateway/ApiGateway.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/gateways/ApiGateway"
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
