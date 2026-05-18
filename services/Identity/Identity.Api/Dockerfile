# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Identity/Identity.Domain/Identity.Domain.csproj", "services/Identity/Identity.Domain/"]
COPY ["services/Identity/Identity.Application/Identity.Application.csproj", "services/Identity/Identity.Application/"]
COPY ["services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj", "services/Identity/Identity.Infrastructure/"]
COPY ["services/Identity/Identity.Api/Identity.Api.csproj", "services/Identity/Identity.Api/"]

RUN dotnet restore "services/Identity/Identity.Api/Identity.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Identity/Identity.Api"
RUN dotnet publish "Identity.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
