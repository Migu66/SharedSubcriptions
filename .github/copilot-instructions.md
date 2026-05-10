## 1. Visión general del producto

**SharedSubscriptions** es una plataforma SaaS para gestionar suscripciones digitales compartidas (Netflix, Spotify, Disney+, etc.) entre grupos de amigos o familias. Resuelve el problema cotidiano de saber quién debe pagar qué cada mes, eliminando el caos de mensajes de WhatsApp.

El sistema permite que una persona (el administrador del grupo) gestione el pago centralizado de una suscripción y que el resto de miembros le reembolsen su parte proporcional. El valor diferencial está en la automatización del cálculo de cuotas, el seguimiento visual del estado de cada pago y los recordatorios automáticos por mensajería.

### Canales de acceso

El producto tiene tres superficies de usuario. La primera es una aplicación web accesible desde el navegador. La segunda es una aplicación móvil nativa para iOS y Android que no requiere navegador instalado. La tercera es una API REST pública para integraciones externas.

---

## 2. Funcionalidades clave del MVP

### Panel del administrador

El dueño de la suscripción, es decir, quien pone la tarjeta de crédito, es el administrador del grupo. Este usuario crea el grupo, define el nombre del servicio contratado, establece el coste total mensual o anual, configura el ciclo de facturación y añade a los demás miembros mediante su dirección de email. El administrador es el único que puede modificar el coste, cambiar el ciclo de facturación o expulsar miembros.

### Cálculo prorrateado automático

Cuando un nuevo miembro se incorpora a mitad de mes, el sistema calcula automáticamente la cuota reducida correspondiente a los días restantes del ciclo. Del mismo modo, cuando el proveedor sube el precio de la suscripción (algo frecuente en 2026), el sistema recalcula de forma inmediata la cuota de cada miembro y notifica el cambio a todos. El cálculo siempre divide el coste total actualizado entre el número de miembros activos en ese momento.

### Sistema de semáforo de pagos

Cada miembro tiene en todo momento un estado de pago visible en el panel. Los tres estados posibles son los siguientes:

El estado **verde** indica que el miembro ha confirmado y pagado su cuota al administrador en el ciclo actual.

El estado **amarillo** indica que el pago está pendiente y que falta un día o menos para la fecha de cobro del ciclo.

El estado **rojo** indica morosidad: el administrador ya pagó al proveedor pero el miembro todavía no le ha reembolsado su parte pasada la fecha límite.

El semáforo es el elemento central del panel y debe ser visible de forma prominente tanto en la vista del administrador como en la vista personal de cada miembro.

### Notificaciones inteligentes

El sistema envía recordatorios automáticos a través de bots de WhatsApp y Telegram. Los mensajes se personalizan con el nombre del miembro, el nombre del servicio, la fecha de renovación y el importe exacto de su cuota. Un ejemplo de mensaje sería: "Hola, Juan. Mañana se renueva Disney+. Tu cuota de 2,99 € está pendiente." Los recordatorios se envían en tres momentos: tres días antes del cobro, el día anterior al cobro y el mismo día si el pago sigue sin confirmarse.

---

## 3. Arquitectura general

El sistema sigue una arquitectura de microservicios donde cada servicio es un Bounded Context DDD independiente. Los servicios nunca comparten base de datos. La comunicación entre ellos se realiza mediante eventos de integración a través de un message broker.

### Los seis microservicios del sistema

**Identity Service** — Bounded Context: identidad y autenticación. Se encarga del registro, el login, los tokens JWT y los roles de usuario.

**Groups Service** — Bounded Context: gestión de grupos. Gestiona la creación de grupos, las invitaciones por email, la gestión de miembros y los roles dentro del grupo (administrador o miembro).

**Subscriptions Service** — Bounded Context: catálogo de suscripciones. Gestiona los servicios contratados, el coste, el ciclo de facturación, las fechas de cobro y los cambios de precio.

**Payments Service** — Bounded Context: pagos y deudas. Registra quién pagó y cuándo, calcula el prorrateo, gestiona las deudas entre miembros e integra con Stripe para pagos online.

**Notifications Service** — Bounded Context: notificaciones. Gestiona los recordatorios automáticos por email, push móvil, Telegram y WhatsApp. Es un servicio de soporte que reacciona a eventos de otros servicios y no tiene lógica de negocio propia.

**Analytics Service** — Bounded Context: informes y estadísticas. Calcula el ahorro anual por grupo, el gasto histórico por servicio y los datos para los gráficos del dashboard. Es un servicio de solo lectura que mantiene proyecciones a partir de los eventos del resto del sistema.

### API Gateway

Existe un API Gateway basado en YARP que actúa como punto de entrada único para todos los clientes. Enruta las peticiones al microservicio correspondiente y aplica autenticación centralizada.

---

## 4. Estructura interna de cada microservicio

Cada servicio sigue Clean Architecture con cuatro capas.

La **capa de Dominio** contiene los agregados, entidades, value objects, domain events, excepciones de dominio e interfaces de repositorios. No tiene dependencias externas de ningún tipo.

La **capa de Aplicación** contiene los casos de uso implementados con el patrón CQRS mediante MediatR, los DTOs, los validadores con FluentValidation y los manejadores de eventos de integración.

La **capa de Infraestructura** contiene las implementaciones concretas de los repositorios con Entity Framework Core, la configuración de la base de datos, la publicación y consumo de eventos con MassTransit y los clientes de servicios externos como Stripe o Telegram.

La **capa de API** contiene los endpoints implementados con Minimal APIs, los hubs de SignalR y los middlewares.

### Regla de dependencias (inviolable)

El Dominio no depende de ninguna otra capa. La Aplicación depende solo del Dominio. La Infraestructura depende de la Aplicación y del Dominio. La API depende de la Aplicación.

---

## 5. Patrones DDD que se aplican en todo el proyecto

### Aggregate Root

Los agregados encapsulan su estado. Las propiedades son privadas o de solo inicialización. Las colecciones internas se exponen únicamente como IReadOnlyCollection. Los métodos de negocio devuelven Result en lugar de lanzar excepciones. Cualquier cambio de estado relevante emite un Domain Event. Los agregados tienen un constructor privado vacío para compatibilidad con Entity Framework Core y un método de fábrica estático para su creación.

### Value Objects

Son inmutables. Se implementan como records en C# 13. Toda la validación ocurre en el método de fábrica estático que devuelve Result. Nunca se crean con new desde fuera del value object. Los value objects relevantes en este proyecto son GroupName, Money (que agrupa Amount y Currency), BillingSchedule, MemberQuota y PaymentStatus.

### Strongly-typed IDs

Todos los identificadores de dominio son tipos fuertes. Nunca se usa Guid crudo en las firmas de los métodos de dominio o en los constructores de agregados. Los identificadores del proyecto son GroupId, SubscriptionId, UserId, PaymentRecordId, DebtId y NotificationId.

### Result pattern

Los métodos de dominio que pueden fallar por reglas de negocio devuelven Result o Result<T> en lugar de lanzar excepciones. Las excepciones se reservan para errores inesperados de infraestructura. Cada agregado tiene una clase estática de errores con propiedades de tipo Error que incluyen un código identificador y un mensaje descriptivo en español.

### Domain Events vs Integration Events

Los Domain Events son sincrónicos, permanecen dentro del mismo servicio y son procesados por MediatR en la misma transacción. Los Integration Events cruzan fronteras entre microservicios, viajan a través del message broker y garantizan la consistencia eventual. Los Integration Events se publican desde los manejadores de Domain Events usando el patrón Outbox para garantizar que ningún evento se pierda aunque el sistema falle.

### CQRS con MediatR

Los Commands modifican el estado y devuelven Result. Las Queries solo leen datos y pueden ir directamente a proyecciones optimizadas sin pasar por los agregados. Los handlers de Commands son clases internal sealed. Los endpoints de la API solo invocan a MediatR, sin ninguna lógica de negocio en ellos.

---

## 6. Modelo de dominio central

Un usuario puede pertenecer a múltiples grupos. Un grupo tiene exactamente un administrador y puede tener varios miembros. Un grupo puede tener varias suscripciones activas, por ejemplo Netflix y Spotify gestionados por el mismo grupo de amigos. Cada suscripción tiene un coste total, un ciclo de facturación y una fecha de próximo cobro.

A partir de cada suscripción se generan cuotas individuales para cada miembro activo en el momento del ciclo. Cada cuota tiene un estado de semáforo (verde, amarillo o rojo). Cuando el administrador confirma que pagó al proveedor, se registra un PaymentRecord y se generan los objetos Debt correspondientes para cada miembro que aún no ha reembolsado su parte.

### Flujo de un ciclo de pago

Primero, el sistema detecta que se acerca la fecha de cobro de una suscripción y emite un evento BillingDueSoon. Segundo, Notifications Service envía recordatorios personalizados a todos los miembros con cuota pendiente. Tercero, el administrador paga al proveedor y lo confirma en la aplicación. Cuarto, Payments Service registra el pago, calcula las deudas proporcionales y emite un evento de integración. Quinto, los estados de semáforo de los miembros que no han pagado pasan a rojo pasada la fecha límite. Sexto, los miembros pagan su cuota al administrador (online vía Stripe o marcándolo manualmente) y su semáforo pasa a verde.

### Cálculo del prorrateo

La cuota base de un miembro es el coste total dividido entre el número de miembros activos. Si el miembro se incorpora el día D de un ciclo mensual de N días, su primera cuota es la cuota base multiplicada por los días restantes del ciclo dividido entre N. Cuando el proveedor cambia el precio, el sistema recalcula todas las cuotas activas del próximo ciclo e informa del cambio mediante notificación. Este cálculo reside exclusivamente en Payments Service y es la lógica de negocio más compleja del sistema.

---

## 7. Eventos de integración entre microservicios

Groups Service emite GroupCreatedIntegrationEvent cuando se crea un grupo, MemberAddedToGroupIntegrationEvent cuando se añade un miembro y MemberRemovedFromGroupIntegrationEvent cuando se elimina uno.

Subscriptions Service emite SubscriptionCreatedIntegrationEvent cuando se crea una suscripción, SubscriptionPriceChangedIntegrationEvent cuando cambia el precio y BillingDueSoonIntegrationEvent tres días antes de cada cobro.

Payments Service emite PaymentConfirmedIntegrationEvent cuando el administrador confirma el pago al proveedor, DebtCreatedIntegrationEvent cuando se generan las deudas de los miembros y DebtSettledIntegrationEvent cuando un miembro salda su deuda.

Notifications Service no emite eventos propios, solo los consume de todos los demás servicios para enviar las notificaciones correspondientes en cada caso.

Analytics Service consume todos los eventos anteriores para mantener sus proyecciones de lectura actualizadas en tiempo real.

---

## 8. Stack tecnológico

El lenguaje y runtime es .NET 10 con C# 13. Las APIs usan ASP.NET Core con Minimal APIs. El ORM es Entity Framework Core 10. Cada microservicio tiene su propia base de datos SQL Server o PostgreSQL. El message broker es RabbitMQ en desarrollo y Azure Service Bus en producción. La abstracción de mensajería es MassTransit. El mediador de CQRS es MediatR. La validación usa FluentValidation. La autenticación combina ASP.NET Core Identity con JWT Bearer. El API Gateway usa YARP. El tiempo real usa SignalR. Los pagos online integran Stripe.net. El email usa SendGrid. Las notificaciones push usan Firebase Cloud Messaging. Los bots de mensajería usan Telegram.Bot y la API oficial de WhatsApp Business. El frontend web es Blazor Web App con SSR e InteractiveServer. El frontend móvil es .NET MAUI Blazor Hybrid. La UI compartida entre web y móvil es una Razor Class Library. Los contenedores usan Docker y Docker Compose en desarrollo y Kubernetes en producción. Los tests unitarios usan xUnit con FluentAssertions y NSubstitute. Los tests de integración usan Testcontainers for .NET. El logging usa Serilog hacia Seq en desarrollo y Azure Monitor en producción.

---

## 9. Orden de desarrollo del proyecto

Este es el orden exacto en el que se construye el sistema. Cada paso desbloquea el siguiente. Nunca se empieza un servicio sin haber completado los anteriores.

### Fase 1 — SharedKernel ----

Es lo primero que se construye porque todos los servicios dependen de él. Contiene las clases base abstractas que todo el sistema comparte: la clase base de AggregateRoot con soporte para domain events, la interfaz y clase base de Entity, la clase base de ValueObject, las interfaces IDomainEvent e IIntegrationEvent, la clase Result y Result<T> con el tipo Error, la interfaz IRepository<T> genérica, la interfaz IUnitOfWork, la interfaz IDateTimeProvider y la clase base IntegrationEvent. Hasta que estas clases no existen, no se puede escribir ningún agregado en ningún servicio.

#### Fase 1.1 — Estructura del proyecto SharedKernel----

Crear el proyecto `SharedKernel` como una Class Library de .NET 10. Añadirlo a la solución principal. Crear la estructura de carpetas interna: `Abstractions`, `Domain`, `Results` y `Events`. En este paso no se escribe ninguna clase, solo la estructura y el `.csproj` sin dependencias externas de ningún tipo.

#### Fase 1.2 — Result pattern: Error, Result y Result<T>----

Crear en la carpeta `Results` el tipo `Error` como record inmutable con propiedades `string Code` y `string Description`. Crear la clase `Result` con propiedades `bool IsSuccess`, `bool IsFailure` y `Error Error`, método de fábrica estático `Success()` y `Failure(Error)`. Crear la clase genérica `Result<T>` que hereda de `Result` con la propiedad `T Value` y métodos de fábrica `Success(T value)` y `Failure(Error)`. Ninguna de estas clases tiene dependencias externas.

#### Fase 1.3 — Interfaces de dominio base----

Crear en la carpeta `Domain` las interfaces: `IDomainEvent` (marker interface vacía), `IEntity` con propiedad `Guid Id`. Crear la clase abstracta `Entity` con propiedad `Guid Id` con `protected set`, constructor protegido que acepta `Guid id` y constructor privado vacío para EF Core. Esta clase es la base de todas las entidades del sistema.

#### Fase 1.4 — AggregateRoot con soporte para Domain Events----

Crear la clase abstracta `AggregateRoot` que hereda de `Entity`. Incluye una lista privada `List<IDomainEvent>` expuesta como `IReadOnlyCollection<IDomainEvent> DomainEvents`. Método protegido `RaiseDomainEvent(IDomainEvent domainEvent)` que añade el evento a la lista. Método público `ClearDomainEvents()` que vacía la lista tras su procesamiento. Constructor protegido y constructor privado vacío para EF Core.

#### Fase 1.5 — Integration Events e interfaces de infraestructura----

Crear en la carpeta `Events` la interfaz `IIntegrationEvent` (marker interface vacía) y la clase abstracta `IntegrationEvent` con propiedad `Guid Id` generado en el constructor, `DateTime OccurredOnUtc` y `string EventType` con el nombre del tipo concreto. Crear en la carpeta `Abstractions` las interfaces: `IUnitOfWork` con método `Task<int> SaveChangesAsync(CancellationToken)`, `IDateTimeProvider` con propiedad `DateTime UtcNow` e `IRepository<T>` genérica con métodos `GetByIdAsync`, `AddAsync` y `UpdateAsync`.

#### Fase 1.6 — Tests unitarios del SharedKernel----

Crear el proyecto `SharedKernel.Tests` con xUnit y FluentAssertions. Tests para `Result`: creación de resultado exitoso, creación de resultado fallido, verificación de `IsSuccess` e `IsFailure`. Tests para `AggregateRoot`: que `RaiseDomainEvent` añade el evento correctamente, que `ClearDomainEvents` vacía la lista. Tests para `Error`: que dos errores con el mismo código y descripción son iguales por ser records.

### Fase 2 — Groups Service (completo)

Es el segundo porque es el Bounded Context central del negocio. Sin grupo no existe ninguna otra entidad del sistema. Se construye en este orden interno: primero el Domain (agregado Group, agregado Invitation, value objects GroupName y GroupId, domain events, errores de dominio e interfaz IGroupRepository), luego el Application (commands CreateGroup y AddMember, queries GetGroupDetails y GetGroupsByUser, validadores y DTOs), luego el Infrastructure (GroupsDbContext, configuraciones de EF Core, implementación de GroupRepository, publicación de integration events con MassTransit) y finalmente el Api (endpoints Minimal API, registro de dependencias en Program.cs).

#### Fase 2.1 — Estructura de proyectos del Groups Service ----

Crear la solución de carpetas y los cuatro proyectos de la Clean Architecture para el Groups Service: `Groups.Domain`, `Groups.Application`, `Groups.Infrastructure` y `Groups.Api`. Configurar las referencias entre proyectos respetando la regla de dependencias: Domain sin referencias externas, Application referencia Domain, Infrastructure referencia Application y Domain, Api referencia Application. Añadir los cuatro proyectos a la solución principal. En este paso no se escribe ninguna clase de negocio, solo la estructura de carpetas y los archivos `.csproj` con sus dependencias y los paquetes NuGet base de cada capa.

#### Fase 2.2 — Strongly-typed ID: GroupId ----

Crear el value object `GroupId` en la capa Domain. Es un record inmutable que envuelve un `Guid`. Incluye un método de fábrica estático `New()` que genera un nuevo identificador y un método `From(Guid value)` que devuelve `Result<GroupId>` validando que el guid no sea vacío. Registrar la conversión de `GroupId` en EF Core mediante un `ValueConverter` en la capa Infrastructure (se deja preparado aunque aún no existe el DbContext).

#### Fase 2.3 — Value Object: GroupName ----

Crear el value object `GroupName` en la capa Domain como record inmutable. El método de fábrica estático `Create(string value)` devuelve `Result<GroupName>`. Las reglas de validación son: el nombre no puede ser nulo ni vacío, debe tener entre 3 y 100 caracteres. Añadir la clase estática `GroupNameErrors` con los errores de dominio correspondientes en español. Este value object no depende de ningún otro tipo del proyecto.

#### Fase 2.4 — Entidad Member ----

Crear la entidad `Member` dentro de la capa Domain. Representa a un usuario dentro de un grupo. Propiedades: `UserId` (strongly-typed), `Email` (string, solo lectura), `Role` (enum `GroupRole` con valores `Admin` y `Member`), `JoinedAt` (DateTime). La entidad tiene constructor privado vacío para EF Core y método de fábrica estático `Create(UserId, string email, GroupRole, DateTime)` que devuelve `Result<Member>`. No tiene lógica de negocio propia, es controlada por el agregado `Group`.

#### Fase 2.5 — Agregado Group (estructura base) ----

Crear el agregado `Group` en la capa Domain como la clase raíz que hereda de `AggregateRoot`. Propiedades: `Id` de tipo `GroupId`, `Name` de tipo `GroupName`, `AdminId` de tipo `UserId`, `CreatedAt` de tipo `DateTime` y la colección interna privada de `Member` expuesta como `IReadOnlyCollection<Member>`. Constructor privado vacío para EF Core. Método de fábrica estático `Create(GroupName, UserId adminId, DateTime)` que devuelve `Result<Group>` y emite el domain event `GroupCreatedEvent`. En esta fase solo se crea la estructura y el método `Create`, sin los métodos de negocio adicionales.

#### Fase 2.6 — Domain Events del agregado Group ----

Crear los tres domain events que emite el agregado `Group`: `GroupCreatedEvent` (contiene `GroupId` y `UserId adminId`), `MemberAddedEvent` (contiene `GroupId`, `UserId` y `string email`) y `MemberRemovedEvent` (contiene `GroupId` y `UserId`). Todos implementan la interfaz `IDomainEvent` del SharedKernel. Son records inmutables. No tienen lógica, solo transportan datos.

#### Fase 2.7 — Métodos de negocio del agregado Group ----

Añadir al agregado `Group` los métodos de negocio: `AddMember(UserId, string email, DateTime joinedAt)` que devuelve `Result` y emite `MemberAddedEvent`, y `RemoveMember(UserId)` que devuelve `Result` y emite `MemberRemovedEvent`. Añadir la clase estática `GroupErrors` con todos los errores de dominio del agregado: `Group.MemberAlreadyExists`, `Group.MemberNotFound`, `Group.AdminCannotBeRemoved`, `Group.NameRequired`, `Group.NameTooShort` y `Group.NameTooLong`. Los mensajes descriptivos van en español.

#### Fase 2.8 — Agregado Invitation ----

Crear el agregado `Invitation` en la capa Domain. Propiedades: `Id` de tipo `InvitationId` (crear también este strongly-typed ID), `GroupId`, `InviteeEmail` (string), `Status` (enum `InvitationStatus` con valores `Pending`, `Accepted` y `Cancelled`), `CreatedAt` y `ExpiresAt`. Método de fábrica `Create(GroupId, string email, DateTime createdAt, DateTime expiresAt)` que devuelve `Result<Invitation>`. Métodos de negocio `Accept()` y `Cancel()` que devuelven `Result`. Clase estática `InvitationErrors` con los errores correspondientes en español.

#### Fase 2.9 — Interfaz IGroupRepository ----

Crear la interfaz `IGroupRepository` en la capa Domain. Métodos: `GetByIdAsync(GroupId, CancellationToken)`, `AddAsync(Group, CancellationToken)`, `UpdateAsync(Group, CancellationToken)` y `GetByUserIdAsync(UserId, CancellationToken)` que devuelve `IReadOnlyList<Group>`. Crear también `IInvitationRepository` con `GetByIdAsync`, `AddAsync` y `GetPendingByEmailAsync`. Estas interfaces no tienen implementación en esta fase, solo la definición del contrato.

#### Fase 2.10 — Command: CreateGroup ----

Crear en la capa Application el command `CreateGroupCommand` con propiedades `string Name` y `UserId AdminId`. Crear el handler `CreateGroupCommandHandler` como `internal sealed class` que implementa `IRequestHandler<CreateGroupCommand, Result<GroupId>>`. El handler usa `IGroupRepository`, `IUnitOfWork` e `IDateTimeProvider`. Lógica: crear `GroupName` desde el comando, crear el agregado `Group`, persistirlo y llamar a `SaveChangesAsync`. Crear el validador `CreateGroupCommandValidator` con FluentValidation que valida que `Name` no esté vacío y tenga entre 3 y 100 caracteres.

#### Fase 2.11 — Command: AddMember ----

Crear el command `AddMemberCommand` con propiedades `GroupId`, `UserId AdminId` (quien ejecuta la acción), `string InviteeEmail` y `UserId NewMemberId`. Crear el handler `AddMemberCommandHandler` como `internal sealed class` que devuelve `Result`. El handler verifica que el grupo existe, que el solicitante es el administrador y llama a `group.AddMember(...)`. Crear el validador `AddMemberCommandValidator` que valida que el email tenga formato válido y que los IDs no sean vacíos.

#### Fase 2.12 — Command: RemoveMember ----

Crear el command `RemoveMemberCommand` con propiedades `GroupId`, `UserId AdminId` y `UserId MemberToRemoveId`. Crear el handler `RemoveMemberCommandHandler` como `internal sealed class` que devuelve `Result`. El handler carga el grupo, verifica que el solicitante es el administrador y llama a `group.RemoveMember(...)`. Crear el validador correspondiente con FluentValidation.

#### Fase 2.13 — Query: GetGroupDetailsQuery ----

Crear la query `GetGroupDetailsQuery` con propiedad `GroupId`. Crear el DTO `GroupDetailsDto` con propiedades: `GroupId Id`, `string Name`, `UserId AdminId`, `DateTime CreatedAt` y `IReadOnlyList<MemberDto> Members`. Crear `MemberDto` con `UserId Id`, `string Email`, `string Role` y `DateTime JoinedAt`. Crear el handler `GetGroupDetailsQueryHandler` como `internal sealed class` que devuelve `Result<GroupDetailsDto>`. El handler carga el grupo y lo proyecta al DTO.

#### Fase 2.14 — Query: GetGroupsByUserQuery ----

Crear la query `GetGroupsByUserQuery` con propiedad `UserId`. Crear el DTO `GroupSummaryDto` con `GroupId Id`, `string Name`, `int MemberCount` y `string UserRole`. Crear el handler `GetGroupsByUserQueryHandler` como `internal sealed class` que devuelve `Result<IReadOnlyList<GroupSummaryDto>>`. El handler usa `IGroupRepository.GetByUserIdAsync` y proyecta los resultados.

#### Fase 2.15 — Integration Events del Groups Service ----

Crear en la capa Application (o en un proyecto compartido de contratos) los tres integration events: `GroupCreatedIntegrationEvent` con `GroupId` y `UserId AdminId`, `MemberAddedToGroupIntegrationEvent` con `GroupId`, `UserId` y `string Email`, y `MemberRemovedFromGroupIntegrationEvent` con `GroupId` y `UserId`. Todos heredan de `IntegrationEvent` del SharedKernel. Crear los manejadores de domain events que publican estos integration events usando el patrón Outbox: `GroupCreatedEventHandler`, `MemberAddedEventHandler` y `MemberRemovedEventHandler`.

#### Fase 2.16 — GroupsDbContext y configuraciones EF Core ----

Crear en la capa Infrastructure el `GroupsDbContext` que hereda de `DbContext`. `DbSet<Group>` y `DbSet<Invitation>`. Crear las clases de configuración de EF Core: `GroupConfiguration` que implementa `IEntityTypeConfiguration<Group>` (mapea la tabla, configura `GroupId` con el `ValueConverter`, mapea `GroupName` como owned type, configura la colección de `Member` como owned entity collection) e `InvitationConfiguration` que implementa `IEntityTypeConfiguration<Invitation>`. Crear la migration inicial.

#### Fase 2.17 — Implementaciones de repositorios ----

Crear `GroupRepository` en la capa Infrastructure que implementa `IGroupRepository` usando `GroupsDbContext`. Implementar todos los métodos de la interfaz con `async/await`. Crear `InvitationRepository` que implementa `IInvitationRepository`. Crear la implementación de `IUnitOfWork` basada en `GroupsDbContext.SaveChangesAsync`. Registrar todo en el contenedor de dependencias.

#### Fase 2.18 — Publicación de Integration Events con MassTransit y Outbox ----

Configurar MassTransit en la capa Infrastructure para el Groups Service. Configurar el patrón Outbox de MassTransit con EF Core para que los integration events se publiquen de forma transaccional junto con los cambios del agregado. Configurar la conexión a RabbitMQ (en desarrollo) leyendo la cadena de conexión desde `appsettings.json`. Registrar los publicadores en el contenedor de dependencias.

#### Fase 2.19 — Endpoints Minimal API ----

Crear en la capa Api los endpoints del Groups Service usando Minimal APIs organizados en una clase de extensión `GroupEndpoints`. Endpoints: `POST /api/groups` (CreateGroup), `GET /api/groups/{groupId}` (GetGroupDetails), `GET /api/groups/user/{userId}` (GetGroupsByUser), `POST /api/groups/{groupId}/members` (AddMember) y `DELETE /api/groups/{groupId}/members/{memberId}` (RemoveMember). Todos los endpoints solo invocan a MediatR y devuelven los códigos HTTP adecuados. Proteger todos los endpoints con autenticación JWT Bearer.

#### Fase 2.20 — Program.cs y registro de dependencias ----

Completar el `Program.cs` del proyecto `Groups.Api`. Registrar: `GroupsDbContext` con la cadena de conexión desde configuración, los repositorios, MediatR con todos los handlers de la capa Application, FluentValidation, MassTransit con la configuración del Outbox, autenticación JWT Bearer y los endpoints. Añadir el middleware de manejo de errores. Configurar Serilog para logging hacia Seq en desarrollo. Asegurarse de que la aplicación arranca y los endpoints responden correctamente.

#### Fase 2.21 — Tests unitarios del dominio ----

Crear el proyecto `Groups.Domain.Tests` con xUnit, FluentAssertions y NSubstitute. Escribir tests para el agregado `Group`: creación exitosa, fallo por nombre inválido, añadir miembro correctamente, fallo al añadir miembro duplicado, fallo al eliminar al administrador, eliminación exitosa de un miembro. Tests para el value object `GroupName`: creación válida, fallo por vacío, fallo por nombre demasiado corto, fallo por nombre demasiado largo. Tests para el agregado `Invitation`: creación, aceptación y cancelación. Los tests de dominio son C# puro sin mocks, siguiendo el patrón Arrange, Act, Assert con comentarios explícitos.

#### Fase 2.22 — Tests unitarios de la capa Application ----

Crear el proyecto `Groups.Application.Tests`. Escribir tests para `CreateGroupCommandHandler`: creación exitosa verifica que se llama a `AddAsync` y `SaveChangesAsync`, fallo por nombre inválido retorna el error correspondiente. Tests para `AddMemberCommandHandler`: añadir miembro exitosamente, fallo cuando el grupo no existe, fallo cuando el solicitante no es el administrador, fallo cuando el miembro ya existe. Tests para `RemoveMemberCommandHandler` y para los dos query handlers. Usar NSubstitute para los repositorios y el UnitOfWork.

### Fase 3 — Identity Service (completo)

Es el tercero porque se necesitan usuarios reales para probar el flujo completo de grupos. Se construye en el mismo orden interno: Domain, Application, Infrastructure y Api. Usa ASP.NET Core Identity como base. Emite UserRegisteredIntegrationEvent y UserDeletedIntegrationEvent. Groups Service debe estar preparado para consumir UserDeletedIntegrationEvent y eliminar al miembro de sus grupos.

#### Fase 3.1 — Estructura de proyectos del Identity Service ----

Crear los cuatro proyectos de la Clean Architecture: `Identity.Domain`, `Identity.Application`, `Identity.Infrastructure` e `Identity.Api`. Configurar las referencias entre proyectos respetando la regla de dependencias. Añadirlos a la solución principal. En este paso solo se crean los `.csproj` con sus paquetes NuGet base, incluyendo `Microsoft.AspNetCore.Identity.EntityFrameworkCore` en Infrastructure.

#### Fase 3.2 — Strongly-typed ID: UserId ----

Crear el value object `UserId` en `Identity.Domain`. Record inmutable que envuelve un `Guid`. Métodos de fábrica `New()` y `From(Guid value)` que devuelve `Result<UserId>` validando que el guid no sea vacío. Preparar el `ValueConverter` para EF Core. Este ID también se referenciará desde otros servicios a través del SharedKernel o contratos compartidos.

#### Fase 3.3 — Agregado ApplicationUser ----

Crear el agregado `ApplicationUser` en `Identity.Domain` que hereda de `AggregateRoot` y de `IdentityUser<Guid>` para compatibilidad con ASP.NET Core Identity. Propiedades adicionales: `string FirstName`, `string LastName`, `DateTime CreatedAt`. Método de fábrica estático `Create(string email, string firstName, string lastName, DateTime)` que devuelve `Result<ApplicationUser>` y emite `UserRegisteredEvent`. Clase estática `UserErrors` con errores en español: `User.EmailAlreadyExists`, `User.InvalidEmail`, `User.NotFound`.

#### Fase 3.4 — Domain Events del Identity Service ----

Crear los domain events: `UserRegisteredEvent` (contiene `UserId`, `string Email`, `string FirstName`, `string LastName`) y `UserDeletedEvent` (contiene `UserId` y `string Email`). Ambos implementan `IDomainEvent` y son records inmutables.

#### Fase 3.5 — Interfaz IUserRepository ----

Crear la interfaz `IUserRepository` en `Identity.Domain`. Métodos: `GetByIdAsync(UserId, CancellationToken)`, `GetByEmailAsync(string email, CancellationToken)`, `AddAsync(ApplicationUser, CancellationToken)` y `ExistsWithEmailAsync(string email, CancellationToken)` que devuelve `Task<bool>`.

#### Fase 3.6 — Command: RegisterUser ----

Crear en `Identity.Application` el command `RegisterUserCommand` con propiedades `string Email`, `string Password`, `string FirstName` y `string LastName`. Crear el handler `RegisterUserCommandHandler` como `internal sealed class` que devuelve `Result<UserId>`. El handler verifica que el email no exista, crea el `ApplicationUser` y usa `UserManager<ApplicationUser>` para persistirlo con la contraseña hasheada. Crear el validador `RegisterUserCommandValidator` con FluentValidation.

#### Fase 3.7 — Command: LoginUser ----

Crear el command `LoginUserCommand` con propiedades `string Email` y `string Password`. Crear el handler `LoginUserCommandHandler` como `internal sealed class` que devuelve `Result<AuthTokenDto>`. El handler verifica credenciales con `SignInManager`, genera el token JWT y el refresh token. Crear el DTO `AuthTokenDto` con `string AccessToken`, `string RefreshToken` y `DateTime ExpiresAt`. Crear el validador correspondiente.

#### Fase 3.8 — Command: RefreshToken ----

Crear el command `RefreshTokenCommand` con propiedad `string RefreshToken`. Crear el handler `RefreshTokenCommandHandler` que valida el refresh token almacenado, genera un nuevo par de tokens y devuelve `Result<AuthTokenDto>`. Los refresh tokens se almacenan en base de datos con fecha de expiración.

#### Fase 3.9 — Query: GetUserProfileQuery ----

Crear la query `GetUserProfileQuery` con propiedad `UserId`. Crear el DTO `UserProfileDto` con `UserId Id`, `string Email`, `string FirstName`, `string LastName` y `DateTime CreatedAt`. Crear el handler `GetUserProfileQueryHandler` como `internal sealed class` que devuelve `Result<UserProfileDto>`.

#### Fase 3.10 — Integration Events del Identity Service ----

Crear los integration events: `UserRegisteredIntegrationEvent` con `UserId`, `string Email`, `string FirstName` y `string LastName`, y `UserDeletedIntegrationEvent` con `UserId` y `string Email`. Crear los handlers de domain events `UserRegisteredEventHandler` y `UserDeletedEventHandler` que publican los integration events mediante el patrón Outbox.

#### Fase 3.11 — IdentityDbContext y configuraciones EF Core ----

Crear `IdentityDbContext` que hereda de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`. Crear `ApplicationUserConfiguration` que implementa `IEntityTypeConfiguration<ApplicationUser>`. Crear la entidad `RefreshToken` con su configuración EF Core para almacenar refresh tokens. Crear la migración inicial.

#### Fase 3.12 — Implementación de repositorios y JWT ----

Crear `UserRepository` en `Identity.Infrastructure` que implementa `IUserRepository`. Crear el servicio `JwtTokenService` que genera tokens JWT firmados con la clave secreta desde configuración, con claims de `UserId`, `email` y `roles`. Crear `RefreshTokenRepository`. Registrar todo en el contenedor de dependencias.

#### Fase 3.13 — MassTransit y Outbox en Identity Service ----

Configurar MassTransit con el patrón Outbox de EF Core en `Identity.Infrastructure`. Configurar el consumidor de `UserDeletedIntegrationEvent` en Groups Service (añadir el handler `UserDeletedIntegrationEventConsumer` que llama a `RemoveMember` para todos los grupos del usuario). Configurar la conexión a RabbitMQ desde `appsettings.json`.

#### Fase 3.14 — Endpoints Minimal API del Identity Service ----

Crear en `Identity.Api` la clase de extensión `IdentityEndpoints` con los endpoints: `POST /api/auth/register` (RegisterUser), `POST /api/auth/login` (LoginUser), `POST /api/auth/refresh` (RefreshToken) y `GET /api/users/{userId}/profile` (GetUserProfile). El endpoint de perfil requiere autenticación JWT. Los demás son públicos.

#### Fase 3.15 — Tests unitarios del Identity Service ----

Crear `Identity.Domain.Tests` con tests para `ApplicationUser`: creación exitosa, emisión de `UserRegisteredEvent`, errores de validación. Crear `Identity.Application.Tests` con tests para `RegisterUserCommandHandler`: registro exitoso, fallo por email duplicado. Tests para `LoginUserCommandHandler`: login exitoso, fallo por credenciales incorrectas. Usar NSubstitute para `IUserRepository`, `UserManager` y `JwtTokenService`.

### Fase 4 — Subscriptions Service (completo)

Es el cuarto porque depende de que existan grupos (Groups Service) y usuarios autenticados (Identity Service). Gestiona el agregado Subscription con sus value objects Money y BillingSchedule. Consume GroupCreatedIntegrationEvent y MemberRemovedIntegrationEvent. Emite BillingDueSoonIntegrationEvent que es el disparador de todo el flujo de pagos.

#### Fase 4.1 — Estructura de proyectos del Subscriptions Service ----

Crear los cuatro proyectos de la Clean Architecture: `Subscriptions.Domain`, `Subscriptions.Application`, `Subscriptions.Infrastructure` y `Subscriptions.Api`. Configurar las referencias entre proyectos y añadirlos a la solución principal. Solo se crean los `.csproj` con los paquetes NuGet base de cada capa, sin ninguna clase de negocio.

#### Fase 4.2 — Strongly-typed ID: SubscriptionId ----

Crear el value object `SubscriptionId` en `Subscriptions.Domain` como record inmutable que envuelve un `Guid`. Métodos de fábrica `New()` y `From(Guid value)` que devuelve `Result<SubscriptionId>` validando que el guid no sea vacío. Preparar el `ValueConverter` para EF Core.

#### Fase 4.3 — Value Object: Money ----

Crear el value object `Money` en `Subscriptions.Domain` como record inmutable con propiedades `decimal Amount` y `string Currency`. El método de fábrica estático `Create(decimal amount, string currency)` devuelve `Result<Money>`. Reglas de validación: el importe no puede ser negativo, la moneda debe ser un código ISO de tres letras no vacío. Clase estática `MoneyErrors` con los errores en español.

#### Fase 4.4 — Value Object: BillingSchedule ----

Crear el value object `BillingSchedule` en `Subscriptions.Domain` como record inmutable con propiedad `BillingCycle` (enum con valores `Monthly` y `Annual`) y `DateTime NextBillingDate`. El método de fábrica `Create(BillingCycle cycle, DateTime nextBillingDate)` devuelve `Result<BillingSchedule>`. Incluye el método `CalculateNextBillingDate()` que avanza la fecha según el ciclo.

#### Fase 4.5 — Agregado Subscription ----

Crear el agregado `Subscription` en `Subscriptions.Domain` que hereda de `AggregateRoot`. Propiedades: `Id` de tipo `SubscriptionId`, `GroupId` (referencia al contexto de Groups), `ServiceName` (string), `TotalCost` de tipo `Money`, `BillingSchedule` de tipo `BillingSchedule`, `CreatedAt` y `IsActive`. Método de fábrica `Create(GroupId, string serviceName, Money, BillingSchedule, DateTime)` que devuelve `Result<Subscription>` y emite `SubscriptionCreatedEvent`. Clase estática `SubscriptionErrors` con errores en español.

#### Fase 4.6 — Métodos de negocio del agregado Subscription ----

Añadir al agregado `Subscription` los métodos: `UpdatePrice(Money newCost)` que devuelve `Result` y emite `SubscriptionPriceChangedEvent`, `Deactivate()` que devuelve `Result` y emite `SubscriptionDeactivatedEvent`, y `AdvanceBillingCycle()` que actualiza la fecha de próximo cobro y emite `BillingCycleAdvancedEvent`.

#### Fase 4.7 — Domain Events del Subscriptions Service ----

Crear los domain events: `SubscriptionCreatedEvent` (contiene `SubscriptionId`, `GroupId`, `string ServiceName`, `Money TotalCost`), `SubscriptionPriceChangedEvent` (contiene `SubscriptionId`, `Money OldCost`, `Money NewCost`) y `BillingCycleAdvancedEvent` (contiene `SubscriptionId`, `DateTime NewBillingDate`). Todos implementan `IDomainEvent` y son records inmutables.

#### Fase 4.8 — Interfaz ISubscriptionRepository ----

Crear la interfaz `ISubscriptionRepository` en `Subscriptions.Domain`. Métodos: `GetByIdAsync(SubscriptionId, CancellationToken)`, `AddAsync(Subscription, CancellationToken)`, `UpdateAsync(Subscription, CancellationToken)`, `GetByGroupIdAsync(GroupId, CancellationToken)` que devuelve `IReadOnlyList<Subscription>` y `GetDueSoonAsync(DateTime threshold, CancellationToken)` que devuelve las suscripciones próximas a vencer.

#### Fase 4.9 — Command: CreateSubscription ----

Crear el command `CreateSubscriptionCommand` con propiedades `GroupId`, `UserId AdminId`, `string ServiceName`, `decimal TotalCost`, `string Currency`, `BillingCycle` y `DateTime FirstBillingDate`. Crear el handler `CreateSubscriptionCommandHandler` como `internal sealed class` que devuelve `Result<SubscriptionId>`. Crear el validador `CreateSubscriptionCommandValidator` con FluentValidation.

#### Fase 4.10 — Command: UpdateSubscriptionPrice ----

Crear el command `UpdateSubscriptionPriceCommand` con propiedades `SubscriptionId`, `UserId AdminId`, `decimal NewAmount` y `string Currency`. Crear el handler `UpdateSubscriptionPriceCommandHandler` que verifica que el solicitante es administrador del grupo, llama a `subscription.UpdatePrice(...)` y persiste el cambio. Crear el validador correspondiente.

#### Fase 4.11 — Queries del Subscriptions Service ----

Crear la query `GetSubscriptionDetailsQuery` con propiedad `SubscriptionId` y su handler que devuelve `Result<SubscriptionDetailsDto>`. Crear la query `GetSubscriptionsByGroupQuery` con propiedad `GroupId` y su handler que devuelve `Result<IReadOnlyList<SubscriptionSummaryDto>>`. Crear los DTOs correspondientes con todos los campos necesarios.

#### Fase 4.12 — Integration Events del Subscriptions Service ----

Crear los integration events: `SubscriptionCreatedIntegrationEvent`, `SubscriptionPriceChangedIntegrationEvent` (con `SubscriptionId`, `GroupId`, `Money OldCost`, `Money NewCost`, lista de `MemberIds` afectados) y `BillingDueSoonIntegrationEvent` (con `SubscriptionId`, `GroupId`, `string ServiceName`, `Money TotalCost`, `DateTime BillingDate`). Crear los handlers de domain events que los publican mediante el patrón Outbox.

#### Fase 4.13 — Consumidor de eventos externos ----

Crear en `Subscriptions.Infrastructure` el consumidor `MemberRemovedFromGroupIntegrationEventConsumer` que reacciona cuando un miembro es eliminado de un grupo y actualiza el recuento de participantes de las suscripciones activas de ese grupo. Configurar el consumidor en MassTransit.

#### Fase 4.14 — SubscriptionsDbContext, repositorio, endpoints y Program.cs ----

Crear `SubscriptionsDbContext` con la configuración EF Core del agregado `Subscription`. Crear `SubscriptionRepository` que implementa `ISubscriptionRepository`. Crear la clase de extensión `SubscriptionEndpoints` con los endpoints Minimal API: `POST /api/subscriptions`, `GET /api/subscriptions/{id}`, `GET /api/subscriptions/group/{groupId}` y `PUT /api/subscriptions/{id}/price`. Completar `Program.cs` con todos los registros de dependencias, MassTransit con Outbox, Serilog y la migración inicial.

#### Fase 4.15 — Tests unitarios del Subscriptions Service ----

Crear `Subscriptions.Domain.Tests` con tests para el agregado `Subscription`: creación exitosa, actualización de precio, emisión correcta de domain events, desactivación. Tests para los value objects `Money` y `BillingSchedule`: creación válida y casos de error. Crear `Subscriptions.Application.Tests` con tests para los command handlers usando NSubstitute.

### Fase 5 — Payments Service (completo)

Es el quinto porque depende de que existan suscripciones (Subscriptions Service). Contiene la lógica de negocio más compleja: el cálculo prorrateado, la generación de deudas y la integración con Stripe. Consume BillingDueSoonIntegrationEvent y MemberAddedToGroupIntegrationEvent. Emite PaymentConfirmedIntegrationEvent y DebtSettledIntegrationEvent.

#### Fase 5.1 — Estructura de proyectos del Payments Service ----

Crear los cuatro proyectos de la Clean Architecture: `Payments.Domain`, `Payments.Application`, `Payments.Infrastructure` y `Payments.Api`. Configurar las referencias entre proyectos y añadirlos a la solución principal. Solo se crean los `.csproj` con los paquetes NuGet base de cada capa, incluyendo `Stripe.net` en Infrastructure.

#### Fase 5.2 — Strongly-typed IDs: PaymentRecordId y DebtId ----

Crear los value objects `PaymentRecordId` y `DebtId` en `Payments.Domain` como records inmutables que envuelven un `Guid`. Ambos incluyen métodos de fábrica `New()` y `From(Guid value)` que devuelve `Result<T>` validando que el guid no sea vacío. Preparar los `ValueConverter` para EF Core.

#### Fase 5.3 — Value Object: MemberQuota ----

Crear el value object `MemberQuota` en `Payments.Domain` como record inmutable con propiedades `UserId MemberId`, `decimal Amount`, `string Currency` y `bool IsProrrated`. El método de fábrica `Create(UserId, decimal amount, string currency, bool isProrrated)` devuelve `Result<MemberQuota>`. Incluye el método estático `Calculate(Money totalCost, int memberCount)` que devuelve la cuota base, y `CalculateProrrated(Money totalCost, int memberCount, int remainingDays, int totalDays)` que devuelve la cuota prorateada.

#### Fase 5.4 — Agregado PaymentRecord ----

Crear el agregado `PaymentRecord` en `Payments.Domain` que hereda de `AggregateRoot`. Propiedades: `Id` de tipo `PaymentRecordId`, `SubscriptionId`, `GroupId`, `AdminId` de tipo `UserId`, `TotalAmount` de tipo `Money`, `PaidAt` de tipo `DateTime` e `IReadOnlyCollection<MemberQuota> MemberQuotas`. Método de fábrica `Create(SubscriptionId, GroupId, UserId adminId, Money, IReadOnlyList<MemberQuota>, DateTime)` que devuelve `Result<PaymentRecord>` y emite `PaymentRecordCreatedEvent`. Clase estática `PaymentRecordErrors` con errores en español.

#### Fase 5.5 — Agregado Debt ----

Crear el agregado `Debt` en `Payments.Domain` que hereda de `AggregateRoot`. Propiedades: `Id` de tipo `DebtId`, `PaymentRecordId`, `DebtorId` de tipo `UserId`, `CreditorId` de tipo `UserId`, `Amount` de tipo `Money`, `Status` (enum `DebtStatus` con valores `Pending`, `Settled` y `Cancelled`), `CreatedAt` y `SettledAt` nullable. Método de fábrica `Create(PaymentRecordId, UserId debtorId, UserId creditorId, Money, DateTime)` que devuelve `Result<Debt>`. Método de negocio `Settle(DateTime settledAt)` que devuelve `Result` y emite `DebtSettledEvent`. Clase estática `DebtErrors` con errores en español.

#### Fase 5.6 — Domain Events del Payments Service ----

Crear los domain events: `PaymentRecordCreatedEvent` (contiene `PaymentRecordId`, `SubscriptionId`, `GroupId`, `UserId AdminId`, `IReadOnlyList<MemberQuota> Quotas`), `DebtSettledEvent` (contiene `DebtId`, `UserId DebtorId`, `UserId CreditorId`, `Money Amount`). Todos implementan `IDomainEvent` y son records inmutables.

#### Fase 5.7 — Interfaces de repositorios del Payments Service ----

Crear `IPaymentRecordRepository` en `Payments.Domain` con métodos `GetByIdAsync`, `AddAsync`, `UpdateAsync` y `GetBySubscriptionIdAsync`. Crear `IDebtRepository` con `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `GetPendingByDebtorIdAsync` y `GetPendingByCreditorIdAsync`.

#### Fase 5.8 — Command: ConfirmAdminPayment ----

Crear el command `ConfirmAdminPaymentCommand` con propiedades `SubscriptionId`, `UserId AdminId`, `decimal TotalAmount`, `string Currency` y `DateTime PaidAt`. Crear el handler `ConfirmAdminPaymentCommandHandler` como `internal sealed class` que devuelve `Result<PaymentRecordId>`. El handler calcula las cuotas de cada miembro activo, crea el `PaymentRecord`, genera los objetos `Debt` para cada miembro y persiste todo en una sola transacción. Crear el validador correspondiente con FluentValidation.

#### Fase 5.9 — Command: SettleDebt ----

Crear el command `SettleDebtCommand` con propiedades `DebtId` y `UserId DebtorId`. Crear el handler `SettleDebtCommandHandler` como `internal sealed class` que devuelve `Result`. El handler carga la deuda, verifica que el solicitante es el deudor y llama a `debt.Settle(...)`. Crear el validador correspondiente.

#### Fase 5.10 — Command: SettleDebtManually ----

Crear el command `SettleDebtManuallyCommand` con propiedades `DebtId` y `UserId CreditorId`. Crear el handler `SettleDebtManuallyCommandHandler` como `internal sealed class` que devuelve `Result`. El handler permite que el administrador (acreedor) marque la deuda como saldada manualmente, verificando que el solicitante es el acreedor. Crear el validador correspondiente.

#### Fase 5.11 — Queries del Payments Service

Crear la query `GetPaymentHistoryQuery` con propiedades `SubscriptionId` y su handler que devuelve `Result<IReadOnlyList<PaymentRecordDto>>`. Crear la query `GetPendingDebtsQuery` con propiedad `UserId` y su handler que devuelve `Result<IReadOnlyList<DebtDto>>`. Crear los DTOs `PaymentRecordDto` y `DebtDto` con todos los campos necesarios incluyendo el estado del semáforo calculado.

#### Fase 5.12 — Integration Events del Payments Service

Crear los integration events: `PaymentConfirmedIntegrationEvent` (con `PaymentRecordId`, `SubscriptionId`, `GroupId`, `UserId AdminId`, `Money TotalAmount`, `IReadOnlyList<MemberQuotaDto> Quotas`), `DebtCreatedIntegrationEvent` (con `DebtId`, `SubscriptionId`, `UserId DebtorId`, `UserId CreditorId`, `Money Amount`) y `DebtSettledIntegrationEvent` (con `DebtId`, `UserId DebtorId`, `Money Amount`). Crear los handlers de domain events que los publican mediante el patrón Outbox.

#### Fase 5.13 — Consumidores de eventos externos

Crear en `Payments.Infrastructure` el consumidor `BillingDueSoonIntegrationEventConsumer` que reacciona al evento de cobro inminente y prepara las cuotas del próximo ciclo. Crear `MemberAddedToGroupIntegrationEventConsumer` que registra al nuevo miembro para el cálculo de prorrateo. Configurar ambos consumidores en MassTransit.

#### Fase 5.14 — Integración con Stripe

Crear el servicio `StripePaymentService` en `Payments.Infrastructure` que envuelve la API de Stripe.net. Implementar el método `CreatePaymentIntentAsync(Money amount, UserId debtorId)` que crea un PaymentIntent en Stripe y devuelve el `client_secret`. Implementar el webhook handler para procesar los eventos `payment_intent.succeeded` de Stripe y llamar a `SettleDebtCommand`. Registrar el servicio en el contenedor de dependencias.

#### Fase 5.15 — PaymentsDbContext, repositorios, endpoints, tests y Program.cs

Crear `PaymentsDbContext` con la configuración EF Core de los agregados `PaymentRecord` y `Debt`. Crear las implementaciones de repositorios. Crear la clase de extensión `PaymentEndpoints` con los endpoints: `POST /api/payments/confirm`, `POST /api/payments/debts/{debtId}/settle`, `POST /api/payments/debts/{debtId}/settle-manual`, `GET /api/payments/history/{subscriptionId}` y `GET /api/payments/debts/pending/{userId}`. Completar `Program.cs`. Crear `Payments.Domain.Tests` con tests para el cálculo de prorrateo y `Payments.Application.Tests` con tests para los command handlers.

### Fase 6 — Notifications Service (completo)

Es el sexto porque solo reacciona a eventos de los demás servicios. No tiene lógica de negocio propia. Consume BillingDueSoonIntegrationEvent, PaymentConfirmedIntegrationEvent y MemberAddedToGroupIntegrationEvent. Integra con Telegram.Bot, la API de WhatsApp Business, SendGrid y Firebase Cloud Messaging.

#### Fase 6.1 — Estructura de proyectos del Notifications Service

Crear los cuatro proyectos de la Clean Architecture: `Notifications.Domain`, `Notifications.Application`, `Notifications.Infrastructure` y `Notifications.Api`. Configurar las referencias entre proyectos y añadirlos a la solución principal. En Infrastructure añadir los paquetes NuGet: `Telegram.Bot`, `SendGrid` y `FirebaseAdmin`. En este paso no se escribe ninguna clase de negocio.

#### Fase 6.2 — Strongly-typed ID y entidad NotificationLog

Crear el value object `NotificationId` en `Notifications.Domain` como record inmutable que envuelve un `Guid`. Crear la entidad `NotificationLog` con propiedades `NotificationId Id`, `string RecipientUserId`, `string Channel` (enum `NotificationChannel` con valores `Email`, `Push`, `Telegram` y `WhatsApp`), `string Message`, `DateTime SentAt` y `bool Success`. Esta entidad sirve para auditar todos los envíos realizados.

#### Fase 6.3 — Interfaces de canales de notificación

Crear en `Notifications.Application` las interfaces de los canales: `IEmailSender` con método `SendAsync(string to, string subject, string body, CancellationToken)`, `IPushNotificationSender` con método `SendAsync(string deviceToken, string title, string body, CancellationToken)`, `ITelegramSender` con método `SendAsync(string chatId, string message, CancellationToken)` y `IWhatsAppSender` con método `SendAsync(string phoneNumber, string message, CancellationToken)`. Todas devuelven `Task<Result>`.

#### Fase 6.4 — Consumidor: BillingDueSoonIntegrationEventConsumer

Crear en `Notifications.Application` el consumidor `BillingDueSoonIntegrationEventConsumer` que implementa `IConsumer<BillingDueSoonIntegrationEvent>`. Al recibir el evento, construye el mensaje personalizado con el nombre del servicio, la fecha de cobro y el importe de la cuota, y lo envía a cada miembro del grupo por todos sus canales configurados. Registrar el consumidor en MassTransit.

#### Fase 6.5 — Consumidor: PaymentConfirmedIntegrationEventConsumer

Crear el consumidor `PaymentConfirmedIntegrationEventConsumer` que notifica a cada miembro deudor que el administrador ha confirmado el pago al proveedor y que su deuda está pendiente de reembolso. El mensaje incluye el nombre del servicio, el importe de su cuota y un enlace directo al panel de pagos.

#### Fase 6.6 — Consumidor: DebtSettledIntegrationEventConsumer

Crear el consumidor `DebtSettledIntegrationEventConsumer` que notifica al administrador (acreedor) que un miembro ha saldado su deuda. El mensaje incluye el nombre del miembro y el importe recibido.

#### Fase 6.7 — Implementaciones de los canales de notificación

Crear en `Notifications.Infrastructure` las implementaciones: `SendGridEmailSender` que usa la API de SendGrid, `FirebasePushNotificationSender` que usa Firebase Admin SDK, `TelegramBotSender` que usa `Telegram.Bot` y `WhatsAppBusinessSender` que usa la API oficial de WhatsApp Business. Todas leen sus credenciales desde `appsettings.json`. Registrar todas las implementaciones en el contenedor de dependencias.

#### Fase 6.8 — NotificationsDbContext, Program.cs y tests

Crear `NotificationsDbContext` con la tabla `NotificationLog` para auditoría. Completar `Program.cs` con el registro de todos los consumidores de MassTransit, los senders, Serilog y la migración inicial. Crear `Notifications.Application.Tests` con tests para los consumidores usando NSubstitute para verificar que se llama al canal correcto con el mensaje esperado en cada escenario.

### Fase 7 — Analytics Service (completo)

Es el séptimo porque consume eventos de todos los demás servicios para construir sus proyecciones. Es un servicio de solo lectura con CQRS puro (solo Queries, sin Commands). Mantiene read models optimizados para los gráficos del dashboard: ahorro anual por grupo, gasto por servicio y evolución histórica de deudas.

#### Fase 7.1 — Estructura de proyectos del Analytics Service

Crear los cuatro proyectos de la Clean Architecture: `Analytics.Domain`, `Analytics.Application`, `Analytics.Infrastructure` y `Analytics.Api`. Configurar las referencias entre proyectos y añadirlos a la solución principal. Este servicio no tiene Commands, solo Queries y consumidores de eventos. Solo se crean los `.csproj` con los paquetes NuGet base de cada capa.

#### Fase 7.2 — Read Models

Crear en `Analytics.Domain` los read models que este servicio mantiene: `GroupSavingsReadModel` (con `GroupId`, `int Year`, `decimal TotalSpent`, `decimal EstimatedSavings`), `ServiceSpendingReadModel` (con `string ServiceName`, `decimal TotalSpent`, `int PaymentCount`) y `DebtHistoryReadModel` (con `UserId`, `decimal TotalDebt`, `decimal TotalSettled`, `int PendingCount`). Son clases planas de solo lectura sin lógica de dominio.

#### Fase 7.3 — Consumidores de eventos de integración

Crear en `Analytics.Application` los consumidores: `PaymentConfirmedIntegrationEventConsumer` que actualiza `GroupSavingsReadModel` y `ServiceSpendingReadModel`, y `DebtSettledIntegrationEventConsumer` que actualiza `DebtHistoryReadModel`. Crear también `SubscriptionCreatedIntegrationEventConsumer` y `MemberAddedToGroupIntegrationEventConsumer` para mantener el contexto necesario en las proyecciones. Registrar todos en MassTransit.

#### Fase 7.4 — Queries del Analytics Service

Crear la query `GetGroupSavingsQuery` con propiedad `GroupId` y `int Year`, handler que devuelve `Result<GroupSavingsDto>`. Crear la query `GetServiceSpendingQuery` con propiedad `GroupId`, handler que devuelve `Result<IReadOnlyList<ServiceSpendingDto>>`. Crear la query `GetDebtHistoryQuery` con propiedad `UserId`, handler que devuelve `Result<DebtHistoryDto>`. Crear los DTOs correspondientes.

#### Fase 7.5 — AnalyticsDbContext y repositorios

Crear `AnalyticsDbContext` con las tablas para cada read model. Crear las interfaces `IGroupSavingsRepository`, `IServiceSpendingRepository` e `IDebtHistoryRepository` en `Analytics.Domain`, con sus implementaciones en `Analytics.Infrastructure`. Los métodos de escritura son llamados solo desde los consumidores de eventos.

#### Fase 7.6 — Endpoints Minimal API del Analytics Service

Crear la clase de extensión `AnalyticsEndpoints` con los endpoints: `GET /api/analytics/groups/{groupId}/savings` (GetGroupSavings), `GET /api/analytics/groups/{groupId}/spending` (GetServiceSpending) y `GET /api/analytics/users/{userId}/debts` (GetDebtHistory). Todos requieren autenticación JWT. Solo invocan a MediatR.

#### Fase 7.7 — Program.cs y registro de dependencias

Completar `Program.cs` del proyecto `Analytics.Api`. Registrar `AnalyticsDbContext`, los repositorios, MediatR con los query handlers, MassTransit con todos los consumidores, autenticación JWT Bearer, Serilog y los endpoints. Crear la migración inicial.

#### Fase 7.8 — Tests del Analytics Service

Crear `Analytics.Application.Tests` con tests para los consumidores de eventos: verificar que `PaymentConfirmedIntegrationEventConsumer` actualiza correctamente los read models, y que `DebtSettledIntegrationEventConsumer` incrementa el contador de deudas saldadas. Usar NSubstitute para los repositorios.

### Fase 8 — API Gateway

Una vez que todos los servicios tienen sus endpoints funcionando, se configura el API Gateway con YARP. Define las rutas hacia cada microservicio, centraliza la autenticación JWT y aplica rate limiting.

#### Fase 8.1 — Estructura del proyecto ApiGateway

Crear el proyecto `ApiGateway` como una aplicación ASP.NET Core vacía de .NET 10. Añadirlo a la solución principal. Añadir el paquete NuGet `Yarp.ReverseProxy`. En este paso no se configura ninguna ruta, solo la estructura base y el `.csproj`.

#### Fase 8.2 — Configuración de rutas YARP

Configurar en `appsettings.json` las rutas y clusters de YARP que apuntan a cada microservicio: Groups Service, Identity Service, Subscriptions Service, Payments Service, Notifications Service y Analytics Service. Cada cluster define la dirección del destino leída desde variables de entorno para poder cambiarse entre entornos sin recompilar.

#### Fase 8.3 — Autenticación centralizada JWT

Configurar la validación del token JWT en el API Gateway mediante `AddAuthentication` con `JwtBearer`. Todos los endpoints protegidos requieren el token emitido por Identity Service. Las rutas públicas (`/api/auth/register` y `/api/auth/login`) se marcan explícitamente como `AllowAnonymous` en la configuración de YARP.

#### Fase 8.4 — Rate limiting y middlewares

Añadir rate limiting global con `AddRateLimiter` usando la política de ventana fija: máximo 100 peticiones por minuto por IP. Añadir middleware de logging de peticiones con Serilog. Añadir middleware de manejo de errores que devuelve respuestas Problem Details consistentes.

#### Fase 8.5 — Program.cs y tests de integración del Gateway

Completar `Program.cs` registrando YARP, la autenticación JWT, el rate limiter y los middlewares en el orden correcto. Crear el proyecto `ApiGateway.Tests` con tests de integración usando `WebApplicationFactory` que verifican el enrutamiento correcto hacia cada servicio y que los endpoints protegidos rechazan peticiones sin token.

### Fase 9 — Razor Class Library (UI compartida)

Se construyen los componentes Razor que se usarán tanto en la web como en la app móvil: el componente del semáforo de pagos, el panel del grupo, las tarjetas de suscripción y los gráficos de ahorro.

#### Fase 9.1 — Estructura del proyecto SharedUI

Crear el proyecto `SharedUI` como una Razor Class Library de .NET 10. Añadirlo a la solución principal. Configurar los paquetes NuGet necesarios. Crear la estructura de carpetas: `Components`, `Layouts` y `Services`. En este paso no se escribe ningún componente, solo la estructura base.

#### Fase 9.2 — Componente: PaymentStatusBadge (semáforo)

Crear el componente Razor `PaymentStatusBadge` que recibe un parámetro `PaymentStatus` (enum con valores `Green`, `Yellow` y `Red`) y renderiza el indicador visual correspondiente. Verde para pagado, amarillo para pendiente con menos de un día, rojo para moroso. El componente debe ser prominente y accesible, con soporte para aria-labels.

#### Fase 9.3 — Componente: GroupPanel

Crear el componente Razor `GroupPanel` que muestra el resumen de un grupo: nombre, número de miembros, suscripciones activas y el estado del semáforo de cada miembro. Acepta parámetros `GroupDetailsDto` y `IReadOnlyList<MemberDto>`. Incluye un `EventCallback` para las acciones de añadir y eliminar miembros.

#### Fase 9.4 — Componente: SubscriptionCard

Crear el componente Razor `SubscriptionCard` que muestra el detalle de una suscripción: nombre del servicio, coste total, cuota individual, próxima fecha de cobro y estado del ciclo. Acepta un parámetro `SubscriptionSummaryDto`. Incluye un botón para confirmar el pago visible solo para el administrador.

#### Fase 9.5 — Componente: SavingsChart

Crear el componente Razor `SavingsChart` que muestra un gráfico del ahorro anual del grupo. Acepta un parámetro `GroupSavingsDto`. Usa una librería de gráficos compatible con Blazor (por ejemplo, `Blazor.ApexCharts`) para renderizar el gráfico de barras con el gasto mensual acumulado frente al coste individual estimado sin el reparto.

### Fase 10 — Blazor Web App

Consume la Razor Class Library y el API Gateway. Implementa el dashboard principal, la gestión de grupos y el historial de pagos.

#### Fase 10.1 — Estructura del proyecto WebApp

Crear el proyecto `WebApp` como una Blazor Web App de .NET 10 con renderizado SSR e InteractiveServer. Añadirlo a la solución principal. Referenciar `SharedUI` y configurar los paquetes NuGet necesarios: cliente HTTP, autenticación con cookies y gestión de estado. Crear la estructura de carpetas: `Pages`, `Components`, `Services` y `Layout`.

#### Fase 10.2 — Autenticación y gestión de tokens

Crear el servicio `AuthService` que gestiona el ciclo de vida del token JWT: login, logout, refresco automático y almacenamiento en cookies HttpOnly. Configurar el esquema de autenticación con `CookieAuthenticationDefaults` en `Program.cs`. Crear el componente `LoginPage` con el formulario de email y contraseña que llama al endpoint `/api/auth/login` del Gateway.

#### Fase 10.3 — Dashboard principal

Crear la página `Dashboard` que muestra todos los grupos del usuario autenticado usando el componente `GroupPanel` de `SharedUI`. Para cada grupo muestra las suscripciones activas con `SubscriptionCard` y el estado del semáforo de cada miembro con `PaymentStatusBadge`. La página se actualiza en tiempo real mediante SignalR cuando cambia el estado de un pago.

#### Fase 10.4 — Gestión de grupos

Crear la página `GroupDetail` que muestra el detalle completo de un grupo. Incluye el formulario para añadir miembros por email, el botón para eliminar miembros (visible solo para el administrador) y la lista de suscripciones del grupo. Crear la página `CreateGroup` con el formulario de creación que llama a `POST /api/groups`.

#### Fase 10.5 — Historial de pagos y deudas

Crear la página `PaymentHistory` que muestra el historial de pagos de una suscripción usando `GET /api/payments/history/{subscriptionId}`. Crear la página `MyDebts` que muestra las deudas pendientes del usuario con `GET /api/payments/debts/pending/{userId}`, incluyendo el botón de pago online que inicia el flujo de Stripe y el botón de marcar como pagado manualmente.

#### Fase 10.6 — Gráficos de ahorro

Crear la página `Analytics` que muestra el componente `SavingsChart` de `SharedUI` con los datos del servicio Analytics. Incluye el selector de año y el desglose por servicio usando `ServiceSpendingDto`. La página solo es accesible para el administrador del grupo.

### Fase 11 — .NET MAUI Blazor Hybrid

Referencia la misma Razor Class Library que la web. Añade las notificaciones push nativas mediante Firebase y el almacenamiento seguro de tokens con MAUI Secure Storage.

#### Fase 11.1 — Estructura del proyecto MobileApp

Crear el proyecto `MobileApp` como una .NET MAUI Blazor Hybrid App de .NET 10. Añadirlo a la solución principal. Referenciar `SharedUI`. Configurar los paquetes NuGet necesarios: `Microsoft.Maui.Controls`, cliente HTTP con `HttpClient`, `Plugin.Firebase.CloudMessaging` y las herramientas de build para iOS y Android. Crear la estructura de carpetas: `Pages`, `Services` y `Platforms`.

#### Fase 11.2 — Autenticación con MAUI Secure Storage

Crear el servicio `MobileAuthService` que gestiona el ciclo de vida del token JWT en el contexto móvil. Almacenar el `AccessToken` y el `RefreshToken` usando `SecureStorage.SetAsync` de MAUI para que nunca queden expuestos en texto plano. Implementar el refresco automático del token antes de cada petición al Gateway. Crear la página `LoginPage` adaptada para móvil con el formulario de credenciales.

#### Fase 11.3 — Reutilización de componentes SharedUI

Verificar que todos los componentes de `SharedUI` (`PaymentStatusBadge`, `GroupPanel`, `SubscriptionCard` y `SavingsChart`) se renderizan correctamente dentro del shell de MAUI Blazor Hybrid. Crear las páginas `Dashboard`, `GroupDetail` y `MyDebts` en el proyecto `MobileApp` que referencian los mismos componentes que la web. Adaptar la navegación al modelo Shell de MAUI con flyout menu y tabs.

#### Fase 11.4 — Notificaciones push nativas con Firebase

Configurar Firebase Cloud Messaging en la plataforma iOS (añadir `GoogleService-Info.plist`) y Android (añadir `google-services.json`). Registrar el dispositivo en FCM al hacer login y almacenar el device token en el perfil del usuario llamando al endpoint de Identity Service. Implementar el handler de notificaciones en segundo plano (`FirebaseMessagingService`) que muestra la notificación nativa del sistema operativo al recibir un evento de pago pendiente o deuda saldada.

#### Fase 11.5 — Configuración de plataformas y publicación

Configurar los permisos necesarios en `AndroidManifest.xml` (INTERNET, RECEIVE_BOOT_COMPLETED, VIBRATE) y en `Info.plist` de iOS (push notifications, background modes). Configurar los esquemas de build en el `.csproj` para generar el APK de Android y el IPA de iOS. Verificar que la app arranca correctamente en el emulador de Android y en el simulador de iOS y que el flujo completo de login, dashboard y pago funciona end-to-end.

### Fase 12 — Docker Compose e infraestructura

Se completa el docker-compose.yml con todos los microservicios, se configuran las variables de entorno por servicio y se preparan los Dockerfiles de cada Api. Esta es la fase final antes de considerar el despliegue en Kubernetes.

#### Fase 12.1 — Dockerfiles de cada microservicio

Crear un `Dockerfile` multi-stage para cada uno de los seis microservicios (`Groups.Api`, `Identity.Api`, `Subscriptions.Api`, `Payments.Api`, `Notifications.Api`, `Analytics.Api`) y para el `ApiGateway`. Cada Dockerfile usa la imagen `mcr.microsoft.com/dotnet/sdk:10.0` para build y `mcr.microsoft.com/dotnet/aspnet:10.0` para runtime. El stage de build copia solo el `.csproj` y restaura los paquetes antes de copiar el resto del código para aprovechar la caché de capas de Docker.

#### Fase 12.2 — docker-compose.yml base

Crear el archivo `docker-compose.yml` en la raíz de la solución con los servicios de infraestructura: `rabbitmq` (imagen `rabbitmq:3-management`), `sqlserver` (imagen `mcr.microsoft.com/mssql/server:2022-latest`) y `seq` (imagen `datalust/seq`). Definir los volúmenes persistentes para cada base de datos y configurar las variables de entorno de cada contenedor (contraseñas, puertos). Añadir `healthcheck` a cada servicio de infraestructura.

#### Fase 12.3 — Servicios de aplicación en docker-compose.yml

Añadir al `docker-compose.yml` los contenedores de los seis microservicios y el API Gateway. Cada servicio referencia su `Dockerfile` con la directiva `build`. Configurar las variables de entorno de cada uno: cadena de conexión a su base de datos propia, URL de RabbitMQ, URL de Seq y las claves de JWT. Usar `depends_on` con condición `service_healthy` para garantizar que la infraestructura esté lista antes de arrancar los servicios.

#### Fase 12.4 — docker-compose.override.yml para desarrollo local

Crear `docker-compose.override.yml` para sobreescribir la configuración en desarrollo: exponer los puertos de cada microservicio en localhost para facilitar el debugging, montar los volúmenes de código fuente para hot-reload, reducir los tiempos de healthcheck y añadir el servicio `webApp` apuntando al proyecto `WebApp`. Este archivo nunca se usa en producción.

#### Fase 12.5 — Variables de entorno y documentación de arranque

Crear el archivo `.env.example` en la raíz con todas las variables de entorno necesarias para ejecutar el sistema: credenciales de SQL Server, cadena de conexión de RabbitMQ, clave secreta JWT, API keys de Stripe, SendGrid, Telegram y Firebase. Crear el archivo `README.md` con las instrucciones paso a paso para arrancar el entorno completo con `docker compose up --build` y verificar que todos los servicios responden correctamente.

---

## 10. Convenciones de nomenclatura

Los nombres de los agregados son sustantivos en singular y PascalCase. Los Domain Events usan el pasado más la palabra Event. Los Integration Events usan el pasado más IntegrationEvent. Los Commands usan el imperativo más Command. Las Queries usan Get más el nombre del resultado más Query. Los handlers usan el mismo nombre del comando o query más Handler. Las clases de errores de dominio son estáticas y se nombran con el nombre del agregado más Errors. Los mensajes de error al usuario están en español. Los identificadores de error son en inglés en formato punto, por ejemplo Group.MemberAlreadyExists.

---

## 11. Reglas de generación de código

Las propiedades de entidades y agregados siempre usan private set o init. Las colecciones internas nunca se exponen como List sino como IReadOnlyCollection. Siempre se incluye el constructor privado vacío para compatibilidad con EF Core. Nunca se usa DateTime.Now sino una abstracción IDateTimeProvider inyectada para que sea testeable. Los handlers de Commands y Queries son internal sealed class, nunca public. Los endpoints de la API no contienen lógica de negocio, solo invocan a MediatR. Toda validación de entrada se realiza con FluentValidation en la capa de Application. Nunca se usa .Result ni .Wait() en código asíncrono, siempre async/await. Toda la configuración va en appsettings.json y variables de entorno, nunca hardcodeada en el código. Los tests siguen el patrón Arrange, Act, Assert con comentarios explícitos separando cada sección. Los tests de dominio no usan mocks, son C# puro. Los tests de Application Layer usan NSubstitute para los repositorios y el UnitOfWork.
