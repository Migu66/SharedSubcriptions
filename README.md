# SharedSubscriptions

Plataforma SaaS para gestionar suscripciones digitales compartidas entre grupos.

## Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) 4.x o superior
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (solo para desarrollo local sin Docker)
- Git

## Arrancar el entorno completo

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/SharedSubscriptions.git
cd SharedSubscriptions
```

### 2. Configurar las variables de entorno

```bash
cp .env.example .env
```

Edita el archivo `.env` y rellena al menos estas variables obligatorias:

| Variable | Descripción |
|---|---|
| `SA_PASSWORD` | Contraseña de SQL Server (mínimo 8 caracteres, con mayúsculas y símbolos) |
| `RABBITMQ_USER` | Usuario de RabbitMQ |
| `RABBITMQ_PASS` | Contraseña de RabbitMQ |
| `JWT_SECRET` | Clave secreta JWT (mínimo 32 caracteres) |
| `STRIPE_SECRET_KEY` | Clave secreta de Stripe |
| `SENDGRID_API_KEY` | API Key de SendGrid para emails |
| `TELEGRAM_BOT_TOKEN` | Token del bot de Telegram |
| `FIREBASE_CREDENTIALS_PATH` | Ruta local al JSON de Firebase Admin SDK |

### 3. Levantar todos los servicios

```bash
docker compose up --build
```

La primera vez tardará varios minutos porque descarga las imágenes base y compila todos los servicios.

Para levantar solo la infraestructura (RabbitMQ, SQL Server, Seq):

```bash
docker compose up rabbitmq sqlserver seq
```

### 4. Verificar que todo funciona

Una vez arrancado, estos son los puntos de acceso:

| Servicio | URL | Descripción |
|---|---|---|
| API Gateway | http://localhost:5000 | Punto de entrada principal |
| Web App | http://localhost:5010 | Aplicación web Blazor |
| Seq (logs) | http://localhost:8081 | Visor de logs en tiempo real |
| RabbitMQ UI | http://localhost:15672 | Panel de administración del broker |
| Identity API | http://localhost:5002 | Solo en desarrollo |
| Groups API | http://localhost:5001 | Solo en desarrollo |
| Subscriptions API | http://localhost:5003 | Solo en desarrollo |
| Payments API | http://localhost:5004 | Solo en desarrollo |
| Notifications API | http://localhost:5005 | Solo en desarrollo |
| Analytics API | http://localhost:5006 | Solo en desarrollo |

Comprueba que el gateway responde:

```bash
curl http://localhost:5000/api/auth/register
# Debe devolver 400 o 422 (no 502 ni 404)
```

### 5. Registrar un usuario de prueba

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test1234!","firstName":"Juan","lastName":"García"}'
```

### 6. Hacer login y obtener el token JWT

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test1234!"}'
```

La respuesta incluye el `accessToken` que se usa en las llamadas autenticadas:

```bash
curl http://localhost:5000/api/groups/user/{userId} \
  -H "Authorization: Bearer {accessToken}"
```

## Detener el entorno

```bash
# Detener sin borrar datos
docker compose down

# Detener y borrar todos los datos (volúmenes)
docker compose down -v
```

## Estructura del proyecto

```
SharedSubscriptions/
├── clients/
│   ├── Mobile/MobileApp/          # .NET MAUI Blazor Hybrid (iOS + Android)
│   └── Web/WebApp/                # Blazor Web App (SSR + InteractiveServer)
├── gateways/
│   └── ApiGateway/                # YARP Reverse Proxy
├── services/
│   ├── Analytics/                 # Servicio de estadísticas (solo lectura)
│   ├── Groups/                    # Gestión de grupos y miembros
│   ├── Identity/                  # Autenticación y tokens JWT
│   ├── Notifications/             # Notificaciones (email, push, Telegram, WhatsApp)
│   ├── Payments/                  # Pagos, deudas y prorrateo
│   └── Subscriptions/             # Catálogo de suscripciones
├── shared/
│   ├── RazorComponents/SharedUI/  # Componentes Blazor compartidos (web + móvil)
│   └── SharedKernel/              # Clases base DDD (Result, AggregateRoot, etc.)
├── infra/                         # Configuración de infraestructura adicional
├── docker-compose.yml             # Definición de todos los servicios
├── docker-compose.override.yml    # Overrides para desarrollo local
└── .env.example                   # Plantilla de variables de entorno
```

## Arquitectura

El sistema sigue una **arquitectura de microservicios** donde cada servicio es un Bounded Context DDD independiente. Los servicios se comunican mediante eventos de integración a través de RabbitMQ (MassTransit). Cada servicio tiene su propia base de datos en SQL Server.

Cada microservicio sigue **Clean Architecture** con cuatro capas: Domain → Application → Infrastructure → Api.

## Ejecutar los tests

```bash
# Todos los tests
dotnet test SharedSubscriptions.slnx

# Solo tests de dominio
dotnet test --filter "FullyQualifiedName~Domain.Tests"

# Solo tests de aplicación
dotnet test --filter "FullyQualifiedName~Application.Tests"
```
