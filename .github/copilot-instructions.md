# GitHub Copilot Instructions — SharedSubscriptions

Coloca este archivo en `.github/copilot-instructions.md` en la raíz del repositorio.
Copilot lo leerá automáticamente en cada sesión de Visual Studio / VS Code.

---

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

### Fase 1 — SharedKernel

Es lo primero que se construye porque todos los servicios dependen de él. Contiene las clases base abstractas que todo el sistema comparte: la clase base de AggregateRoot con soporte para domain events, la interfaz y clase base de Entity, la clase base de ValueObject, las interfaces IDomainEvent e IIntegrationEvent, la clase Result y Result<T> con el tipo Error, la interfaz IRepository<T> genérica, la interfaz IUnitOfWork, la interfaz IDateTimeProvider y la clase base IntegrationEvent. Hasta que estas clases no existen, no se puede escribir ningún agregado en ningún servicio.

### Fase 2 — Groups Service (completo)

Es el segundo porque es el Bounded Context central del negocio. Sin grupo no existe ninguna otra entidad del sistema. Se construye en este orden interno: primero el Domain (agregado Group, agregado Invitation, value objects GroupName y GroupId, domain events, errores de dominio e interfaz IGroupRepository), luego el Application (commands CreateGroup y AddMember, queries GetGroupDetails y GetGroupsByUser, validadores y DTOs), luego el Infrastructure (GroupsDbContext, configuraciones de EF Core, implementación de GroupRepository, publicación de integration events con MassTransit) y finalmente el Api (endpoints Minimal API, registro de dependencias en Program.cs).

### Fase 3 — Identity Service (completo)

Es el tercero porque se necesitan usuarios reales para probar el flujo completo de grupos. Se construye en el mismo orden interno: Domain, Application, Infrastructure y Api. Usa ASP.NET Core Identity como base. Emite UserRegisteredIntegrationEvent y UserDeletedIntegrationEvent. Groups Service debe estar preparado para consumir UserDeletedIntegrationEvent y eliminar al miembro de sus grupos.

### Fase 4 — Subscriptions Service (completo)

Es el cuarto porque depende de que existan grupos (Groups Service) y usuarios autenticados (Identity Service). Gestiona el agregado Subscription con sus value objects Money y BillingSchedule. Consume GroupCreatedIntegrationEvent y MemberRemovedIntegrationEvent. Emite BillingDueSoonIntegrationEvent que es el disparador de todo el flujo de pagos.

### Fase 5 — Payments Service (completo)

Es el quinto porque depende de que existan suscripciones (Subscriptions Service). Contiene la lógica de negocio más compleja: el cálculo prorrateado, la generación de deudas y la integración con Stripe. Consume BillingDueSoonIntegrationEvent y MemberAddedToGroupIntegrationEvent. Emite PaymentConfirmedIntegrationEvent y DebtSettledIntegrationEvent.

### Fase 6 — Notifications Service (completo)

Es el sexto porque solo reacciona a eventos de los demás servicios. No tiene lógica de negocio propia. Consume BillingDueSoonIntegrationEvent, PaymentConfirmedIntegrationEvent y MemberAddedToGroupIntegrationEvent. Integra con Telegram.Bot, la API de WhatsApp Business, SendGrid y Firebase Cloud Messaging.

### Fase 7 — Analytics Service (completo)

Es el séptimo porque consume eventos de todos los demás servicios para construir sus proyecciones. Es un servicio de solo lectura con CQRS puro (solo Queries, sin Commands). Mantiene read models optimizados para los gráficos del dashboard: ahorro anual por grupo, gasto por servicio y evolución histórica de deudas.

### Fase 8 — API Gateway

Una vez que todos los servicios tienen sus endpoints funcionando, se configura el API Gateway con YARP. Define las rutas hacia cada microservicio, centraliza la autenticación JWT y aplica rate limiting.

### Fase 9 — Razor Class Library (UI compartida)

Se construyen los componentes Razor que se usarán tanto en la web como en la app móvil: el componente del semáforo de pagos, el panel del grupo, las tarjetas de suscripción y los gráficos de ahorro.

### Fase 10 — Blazor Web App

Consume la Razor Class Library y el API Gateway. Implementa el dashboard principal, la gestión de grupos y el historial de pagos.

### Fase 11 — .NET MAUI Blazor Hybrid

Referencia la misma Razor Class Library que la web. Añade las notificaciones push nativas mediante Firebase y el almacenamiento seguro de tokens con MAUI Secure Storage.

### Fase 12 — Docker Compose e infraestructura

Se completa el docker-compose.yml con todos los microservicios, se configuran las variables de entorno por servicio y se preparan los Dockerfiles de cada Api. Esta es la fase final antes de considerar el despliegue en Kubernetes.

---

## 10. Convenciones de nomenclatura

Los nombres de los agregados son sustantivos en singular y PascalCase. Los Domain Events usan el pasado más la palabra Event. Los Integration Events usan el pasado más IntegrationEvent. Los Commands usan el imperativo más Command. Las Queries usan Get más el nombre del resultado más Query. Los handlers usan el mismo nombre del comando o query más Handler. Las clases de errores de dominio son estáticas y se nombran con el nombre del agregado más Errors. Los mensajes de error al usuario están en español. Los identificadores de error son en inglés en formato punto, por ejemplo Group.MemberAlreadyExists.

---

## 11. Reglas de generación de código

Las propiedades de entidades y agregados siempre usan private set o init. Las colecciones internas nunca se exponen como List sino como IReadOnlyCollection. Siempre se incluye el constructor privado vacío para compatibilidad con EF Core. Nunca se usa DateTime.Now sino una abstracción IDateTimeProvider inyectada para que sea testeable. Los handlers de Commands y Queries son internal sealed class, nunca public. Los endpoints de la API no contienen lógica de negocio, solo invocan a MediatR. Toda validación de entrada se realiza con FluentValidation en la capa de Application. Nunca se usa .Result ni .Wait() en código asíncrono, siempre async/await. Toda la configuración va en appsettings.json y variables de entorno, nunca hardcodeada en el código. Los tests siguen el patrón Arrange, Act, Assert con comentarios explícitos separando cada sección. Los tests de dominio no usan mocks, son C# puro. Los tests de Application Layer usan NSubstitute para los repositorios y el UnitOfWork.
