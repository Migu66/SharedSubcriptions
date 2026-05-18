# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Notifications/Notifications.Domain/Notifications.Domain.csproj", "services/Notifications/Notifications.Domain/"]
COPY ["services/Notifications/Notifications.Application/Notifications.Application.csproj", "services/Notifications/Notifications.Application/"]
COPY ["services/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj", "services/Notifications/Notifications.Infrastructure/"]
COPY ["services/Notifications/Notifications.Api/Notifications.Api.csproj", "services/Notifications/Notifications.Api/"]

RUN dotnet restore "services/Notifications/Notifications.Api/Notifications.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Notifications/Notifications.Api"
RUN dotnet publish "Notifications.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.Api.dll"]
