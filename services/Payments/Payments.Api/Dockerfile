# ── Stage 1: Restaurar y compilar ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY ["shared/SharedKernel/SharedSubscriptions.SharedKernel/SharedSubscriptions.SharedKernel.csproj", "shared/SharedKernel/SharedSubscriptions.SharedKernel/"]
COPY ["services/Payments/Payments.Domain/Payments.Domain.csproj", "services/Payments/Payments.Domain/"]
COPY ["services/Payments/Payments.Application/Payments.Application.csproj", "services/Payments/Payments.Application/"]
COPY ["services/Payments/Payments.Infrastructure/Payments.Infrastructure.csproj", "services/Payments/Payments.Infrastructure/"]
COPY ["services/Payments/Payments.Api/Payments.Api.csproj", "services/Payments/Payments.Api/"]

RUN dotnet restore "services/Payments/Payments.Api/Payments.Api.csproj"

# Copiar el código fuente y publicar
COPY . .
WORKDIR "/src/services/Payments/Payments.Api"
RUN dotnet publish "Payments.Api.csproj" -c Release -o /app/publish --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payments.Api.dll"]
