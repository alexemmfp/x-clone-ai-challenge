# Twitter Clone — The Flock Challenge

Clon funcional de Twitter/X. Backend .NET 10 Clean Architecture + Frontend Vue 3 + TypeScript + PostgreSQL.

---

## Prerrequisitos

| Herramienta | Versión | Verificar |
|---|---|---|
| .NET SDK | **10.0.x** | `dotnet --version` |
| Node.js | **20.x** | `node --version` |
| Docker Engine o Docker Desktop | **25+** | `docker --version` |
| Docker Compose | **v2** (plugin integrado) | `docker compose version` |
| PowerShell | **7+** | `pwsh --version` |
| Git | cualquiera | `git --version` |

---

## Inicio rápido (Docker — un comando)

> Requiere Docker Engine (Linux/macOS) o Docker Desktop (Windows/macOS) con el plugin `docker compose` v2. Levanta Postgres + API + frontend juntos.

```bash
git clone <repo-url> twitter_clone
cd twitter_clone
docker compose up -d --build
```

- Frontend: **http://localhost** (puerto 80)
- API: **http://localhost:8080** — health: `GET /health`
- Postgres: **localhost:5433**

La API corre las migraciones y el seed automáticamente al primer inicio. Credenciales: `alice@example.com` / `Seed1234!`.

---

## Inicio rápido (desarrollo local)

Todos los comandos son copy-pasteable. Ejecutar desde la raíz del repo salvo indicación.

### 1 — Clonar

```bash
git clone <repo-url> twitter_clone
cd twitter_clone
```

### 2 — Levantar PostgreSQL

```bash
docker compose up -d postgres
```

Postgres escucha en **localhost:5433** (puerto host) → 5432 dentro del container.
Credenciales por defecto: usuario `twitter`, password `change-me-local-dev`, base `twitterclone`.

### 3 — Levantar el backend

> Las migraciones y el seed corren automáticamente al iniciar. No se necesita `dotnet ef database update`. `appsettings.Development.json` ya está commiteado con el connection string correcto (puerto 5433).

```powershell
# PowerShell
cd backend/src/TwitterClone.Api
dotnet run
```

```bash
# bash
cd backend/src/TwitterClone.Api && dotnet run
```

El backend escucha en **http://localhost:5089**. Health check: `GET http://localhost:5089/health`.

El backend corre las migraciones y el seed automáticamente al iniciar. No se necesita ningún paso adicional.

### 5 — Configurar el entorno del frontend

El frontend lee `VITE_API_BASE_URL` de un archivo `.env.development`. Este archivo está en `.gitignore` — hay que crearlo manualmente con cualquier editor de texto:

**`frontend/.env.development`**
```
VITE_API_BASE_URL=http://localhost:5089
```

> **Importante:** No usar `echo > archivo` en PowerShell — genera UTF-16 y Vite no lo lee. Usar un editor de texto o `New-Item` + `Set-Content -Encoding utf8`.

### 6 — Levantar el frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend escucha en **http://localhost:5173**.

Abrir **http://localhost:5173** en el navegador. Se puede registrar una cuenta nueva o usar los usuarios del seed.

---

## Seed de datos

Diez cuentas demo están pre-cargadas. Password para todas: **`Seed1234!`**

| Username | Email |
|---|---|
| alice | alice@example.com |
| bob | bob@example.com |
| carol | carol@example.com |
| dave | dave@example.com |
| eve | eve@example.com |
| frank | frank@example.com |
| grace | grace@example.com |
| henry | henry@example.com |
| iris | iris@example.com |
| jack | jack@example.com |

El seed genera automáticamente tweets (con y sin imágenes), follows cruzados y likes entre usuarios. Se ejecuta solo al primer inicio (idempotente).

---

## Variables de entorno

| Variable | Descripción | Valor ejemplo |
|---|---|---|
| `POSTGRES_USER` | Usuario de PostgreSQL | `twitter` |
| `POSTGRES_PASSWORD` | Password de PostgreSQL | `change-me-local-dev` |
| `POSTGRES_DB` | Nombre de la base de datos | `twitterclone` |
| `POSTGRES_PORT` | Puerto interno del container | `5432` |
| `ConnectionStrings__Default` | Connection string del backend (Npgsql) | `Host=localhost;Port=5432;Database=twitterclone;Username=twitter;Password=change-me-local-dev` |
| `Jwt__Issuer` | Issuer del JWT | `twitterclone` |
| `Jwt__Audience` | Audience del JWT | `twitterclone-web` |
| `Jwt__SigningKey` | Clave secreta para firmar tokens (mínimo 32 chars) | `replace-with-32+char-random-secret` |
| `Jwt__AccessTokenMinutes` | Duración del access token en minutos | `15` |
| `Jwt__RefreshTokenDays` | Duración del refresh token en días | `7` |
| `Bcrypt__WorkFactor` | Work factor para hashing de passwords | `11` |
| `Cors__AllowedOrigins` | Origen permitido por CORS | `http://localhost:5173` |
| `VITE_API_BASE_URL` | URL base de la API (solo frontend) | `http://localhost:8080` |

> **`.env` es opcional.** El `docker-compose.yml` tiene defaults para todas las variables — el stack levanta sin ningún archivo `.env`. Copiar `.env.example` a `.env` solo si se quieren sobreescribir valores (por ejemplo, `Jwt__SigningKey` en producción).  
> **Dev local:** el backend usa `appsettings.Development.json` (ya commiteado). Solo crear `frontend/.env.development` con `VITE_API_BASE_URL=http://localhost:5089`.

---

## Correr los tests

```powershell
# Suite completa (build + tests + lint + typecheck)
pwsh scripts/check.ps1

# Solo backend
pwsh scripts/check.ps1 -Backend

# Solo frontend
pwsh scripts/check.ps1 -Frontend
```

Los logs detallados se escriben en `.logs/`. La consola muestra solo un resumen.

**Coverage** (objetivo ≥85%) se enforce en Linux/CI via `scripts/check.sh` (GitHub Actions). En Windows, Smart App Control puede bloquear la instrumentación de DLLs — correr `docker compose run --rm backend dotnet test /p:CollectCoverage=true /p:Threshold=85` para medirlo en un container Linux.

**80 tests de backend**: 10 domain · 50 application · 3 architecture · 17 integration.

---

## Features implementadas

- **Auth** — registro, login, logout, JWT access token + silent refresh via httpOnly cookie
- **Timeline** — feed cronológico de tweets de usuarios seguidos + propios
- **Timeline en tiempo real** — SignalR hub pushea nuevos tweets a clientes conectados instantáneamente
- **Tweets** — crear (280 chars), eliminar propios, reply threads (parentId), imágenes adjuntas
- **Follow / Unfollow** — seguir o dejar de seguir cualquier usuario
- **Likes** — like y unlike en tweets con contadores en vivo
- **Perfil** — ver perfil de cualquier usuario, lista de followers/following, editar bio y avatar propio
- **Búsqueda de usuarios** — barra de búsqueda en sidebar con dropdown de resultados
- **Retweets** — retweet y un-retweet con contador
- **@menciones** — autocomplete al escribir @, renderiza como links a perfiles
- **Notificaciones** — push en tiempo real para follows, likes y menciones via SignalR
- **Docker** — `docker compose up -d --build` levanta todo el stack (Postgres + API + web)

---

## Por qué este stack

### .NET 10 + ASP.NET Core

Es el stack que uso profesionalmente a diario. Elegirlo no fue conveniencia sino una decisión deliberada: en un challenge donde el proceso y la calidad de código pesan tanto como el resultado, no quize realizarlo en un lenguaje o framework desconocido que me cueste verificarlo. Con .NET pude focalizarme en diseñar bien sin pelearme con la sintaxis.

Concretamente para este proyecto:
- **Minimal APIs** permitieron endpoints declarativos sin el boilerplate de controllers, manteniendo cada handler en menos de 20 líneas.
- **EF Core 10 + Npgsql** cubre el grafo de follows (tabla `Follows` con PK compuesta) y el timeline (query con `IN + UNION ALL`) con LINQ legible y migraciones versionadas.
- **BCrypt nativo** para passwords y JWT issuance desde `System.IdentityModel.Tokens.Jwt` sin depender de proveedores de identidad externos.
- **`TreatWarningsAsErrors`** en el `.csproj` convierte warnings de compilador en errores, haciendo imposible commitear código con nullability o casting implícito.
- **xUnit + FluentAssertions + Testcontainers** son el estándar del ecosistema para testing serio: 80 tests, cobertura ≥85%, con Postgres real en los integration tests (no mocks de DB que ocultan bugs de SQL).

### Vue 3 + TypeScript + Pinia

Tuve experiencia previa con Vue.
- Vue's **Composition API** encaja bien para este dominio: cada feature tiene su composable aislado (`useMentionAutocomplete`, stores de Pinia) que se puede testear independientemente del componente que lo usa.
- **Pinia** es más ergonómico que Redux/Zustand para este tamaño: el auth store, el tweet store y el notifications store son archivos de ~50 líneas sin reducers ni action creators.
- El **interceptor de Axios** maneja el silent refresh en 401 en un solo lugar; todos los llamados a la API se benefician sin código adicional en cada componente.
- **vue-tsc** corre el type-checker de TypeScript sobre los templates, no solo sobre el script, atrapando errores que Vite solo detectaría en runtime.

### Clean Architecture

La elegí por una razón práctica y directa: el challenge exige cobertura de tests ≥85%, y Clean Architecture es la estructura que hace ese umbral alcanzable de forma sostenible.

Su separación de capas permite testear los use cases — crear tweet, follow, like, auth — de forma completamente aislada, sin levantar Postgres ni HTTP. Los 50 tests de `Application.Tests` corren con repos mockeados en milisegundos. Clean Architecture no sube el coverage por sí sola (los tests hay que escribirlos igual), pero elimina la principal razón por la que ese número es difícil de alcanzar: que testear lógica de negocio requiera infraestructura real. Con las interfaces definidas en Application y las implementaciones en Infrastructure, cada use case es una función pura desde el punto de vista del test.

El beneficio adicional es que los límites entre capas son verificables automáticamente: `TwitterClone.Architecture.Tests` usa NetArchTest para confirmar que Domain no referencia ningún otro proyecto, que Application no toca Infrastructure, etc. No es una convención que alguien puede romper sin darse cuenta — es un test que falla en CI.

### PostgreSQL

Es la base de datos que uso en mi trabajo diario y con la que me siento más cómodo, lo que en un challenge de 72 horas tiene peso real.

Además encaja bien con el problema: usuarios, tweets, follows y likes son datos que se relacionan entre sí de forma clara y estructurada, y PostgreSQL maneja eso de forma nativa. La tabla de follows tiene una clave primaria compuesta por `(FollowerId, FolloweeId)` — dos columnas que juntas identifican de forma única quién sigue a quién, sin necesidad de lógica extra en el código. El timeline es una consulta SQL directa que trae los tweets de los usuarios que seguís, ordenados por fecha. Por último, uso el mismo PostgreSQL tanto en producción como en los tests de integración, así lo que se prueba es exactamente lo que corre en producción.

---

## Arquitectura

Clean Architecture: `Domain → Application → Infrastructure + Api`. Las dependencias apuntan hacia adentro; Domain no tiene dependencias externas. Los límites de capas se enforcan en test-time con NetArchTest. Ver [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) para la justificación completa, responsabilidades por capa y trade-offs.

---

## Decisiones técnicas

- **Auth propio** — JWT access token (Bearer, 15 min) + refresh token en httpOnly cookie (7 días). Sin proveedores de terceros. Passwords hasheados con BCrypt (work factor 11).
- **SHA256 para hashing del refresh token** — BCrypt es no-determinista (salt random por llamada), por lo que los hashes almacenados no se pueden buscar por valor. Los refresh tokens son strings aleatorios de alta entropía; el determinismo de SHA256 permite lookups exactos en la DB sin el overhead de BCrypt.
- **Clean Architecture** — use cases en `Application`, reglas de dominio en `Domain`, EF Core + repos en `Infrastructure`. Enforced por NetArchTest.
- **EF Core + Testcontainers** — los integration tests levantan un container real de PostgreSQL para cubrir SQL y migraciones reales, no mocks.
- **Vue 3 + Pinia + Axios interceptor** — el interceptor de Axios auto-refresca el access token en 401 antes de reintentar el request original; el usuario nunca ve un redirect a login por expiración de token.
- **SignalR real-time** — `TimelineHub` broadcast eventos `TweetCreated` a todos los clientes conectados. La interfaz (`ITimelineNotifier`) está definida en Application; la implementación con SignalR está en Infrastructure (regla de dependencia preservada).
- **Tailwind mobile-first** — todas las vistas son responsive; el layout colapsa a columna única en pantallas pequeñas.
- **Desarrollo asistido por AI** — Claude Code (Sonnet 4.6) escribió ~85% del código en modo pair-programming: el humano escribió las restricciones en CLAUDE.md, revisó los diffs y tomó decisiones arquitectónicas; la AI implementó handlers, tests, componentes frontend y DevOps. Todos los commits incluyen `Co-Authored-By: Claude Sonnet 4.6`.

---

## Timeline y grafo de follows

**Grafo de follows — tabla `Follows`**

```
Follows { FollowerId (FK→Users), FolloweeId (FK→Users), CreatedAt }
PK: (FollowerId, FolloweeId)
```

Un arco dirigido `(A → B)` significa "A sigue a B". Los follows mutuos son dos filas separadas. Los contadores de followers/following se calculan con queries `COUNT` sobre `FolloweeId` / `FollowerId` respectivamente; sin columnas de contador denormalizadas para mantener la consistencia simple.

**Query del timeline**

El home timeline muestra tweets de los usuarios que sigue el viewer, más los propios, ordenados por `CreatedAt DESC` con paginación por cursor (`createdAt + id` como cursor para evitar duplicados entre páginas):

```sql
SELECT t.* FROM Tweets t
WHERE t.AuthorId IN (
    SELECT FolloweeId FROM Follows WHERE FollowerId = @viewerId
    UNION ALL SELECT @viewerId
)
AND t.ParentId IS NULL          -- excluye replies del feed principal
ORDER BY t.CreatedAt DESC, t.Id DESC
LIMIT @pageSize
```

Los retweets se almacenan como filas separadas en `Retweets { RetweeterId, TweetId }`. El handler del timeline obtiene tweets y retweets de usuarios seguidos, los combina y ordena en memoria (aceptable a esta escala; un sistema en producción denormalizaría en una tabla de feed).

**Trade-offs**

- Modelo pull (query en lectura) en lugar de fan-out en escritura. Simple y correcto a esta escala; no escala a millones de followers sin caché o un servicio de feed dedicado.
- Sin índice full-text — la búsqueda de usuarios usa `ILIKE '%term%'` que requiere sequential scan. Aceptable para demo; en producción se usaría `pg_trgm` o un motor de búsqueda dedicado.

---

## Estructura del proyecto

```
backend/
  src/
    TwitterClone.Domain/         # entidades, value objects — sin dependencias
    TwitterClone.Application/    # use cases, DTOs, interfaces
    TwitterClone.Infrastructure/ # EF Core, repos, JWT, BCrypt
    TwitterClone.Api/            # endpoints minimal API, DI wiring
  tests/
    TwitterClone.Domain.Tests/
    TwitterClone.Application.Tests/
    TwitterClone.Integration.Tests/    # WebApplicationFactory + Testcontainers
    TwitterClone.Architecture.Tests/   # NetArchTest boundary rules
frontend/
  src/{components,views,stores,router,api,composables,types}
  tests/      # Vitest unit/integration
  e2e/        # Playwright
docs/   scripts/   docker-compose.yml   .env.example
```

---

## Más información

- Spec: [`docs/SPEC.md`](docs/SPEC.md)
- Roadmap: [`docs/ROADMAP.md`](docs/ROADMAP.md)
- Arquitectura en detalle: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- Convenciones backend: [`backend/AGENTS.md`](backend/AGENTS.md)
- Convenciones frontend: [`frontend/AGENTS.md`](frontend/AGENTS.md)
