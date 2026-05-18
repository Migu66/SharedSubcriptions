# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Analytics/Analytics.Domain/Analytics.Domain.csproj", "services/Analytics/Analytics.Domain/"]
COPY ["services/Analytics/Analytics.Application/Analytics.Application.csproj", "services/Analytics/Analytics.Application/"]
COPY ["services/Analytics/Analytics.Infrastructure/Analytics.Infrastructure.csproj", "services/Analytics/Analytics.Infrastructure/"]
COPY ["services/Analytics/Analytics.Api/Analytics.Api.csproj", "services/Analytics/Analytics.Api/"]

RUN dotnet restore "services/Analytics/Analytics.Api/Analytics.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Analytics/Analytics.Api"
RUN dotnet publish "Analytics.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Api.dll"]
