# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Groups/Groups.Domain/Groups.Domain.csproj", "services/Groups/Groups.Domain/"]
COPY ["services/Groups/Groups.Application/Groups.Application.csproj", "services/Groups/Groups.Application/"]
COPY ["services/Groups/Groups.Infrastructure/Groups.Infrastructure.csproj", "services/Groups/Groups.Infrastructure/"]
COPY ["services/Groups/Groups.Api/Groups.Api.csproj", "services/Groups/Groups.Api/"]

RUN dotnet restore "services/Groups/Groups.Api/Groups.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Groups/Groups.Api"
RUN dotnet publish "Groups.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Groups.Api.dll"]
