# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Subscriptions/Subscriptions.Domain/Subscriptions.Domain.csproj", "services/Subscriptions/Subscriptions.Domain/"]
COPY ["services/Subscriptions/Subscriptions.Application/Subscriptions.Application.csproj", "services/Subscriptions/Subscriptions.Application/"]
COPY ["services/Subscriptions/Subscriptions.Infrastructure/Subscriptions.Infrastructure.csproj", "services/Subscriptions/Subscriptions.Infrastructure/"]
COPY ["services/Subscriptions/Subscriptions.Api/Subscriptions.Api.csproj", "services/Subscriptions/Subscriptions.Api/"]

RUN dotnet restore "services/Subscriptions/Subscriptions.Api/Subscriptions.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Subscriptions/Subscriptions.Api"
RUN dotnet publish "Subscriptions.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Subscriptions.Api.dll"]
