# Scaffold + Domain + Auth Implementation Plan (40%)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build M0 (backend + frontend scaffold, docker-postgres, arch tests) + M1 (domain entities, EF Core, repositories) + M2 (auth register/login/refresh/logout, integration tests, frontend auth screens).

**Architecture:** Clean Architecture with four layers: Domain → Application → Infrastructure → Api. Use cases are plain handler classes (CQRS-lite, no MediatR). Auth uses JWT access token (body) + refresh token (httpOnly cookie, persisted hashed).

**Tech Stack:** .NET 10 Minimal APIs · EF Core 10 + Npgsql · BCrypt.Net-Next · FluentValidation · xUnit + FluentAssertions + NSubstitute + Testcontainers · NetArchTest · Vue 3 + TypeScript + Pinia + Vue Router + Tailwind + Axios · Vitest + @vue/test-utils · Playwright.

---

## File Map

### Backend — new files

```
backend/TwitterClone.sln
backend/src/TwitterClone.Domain/
  TwitterClone.Domain.csproj
  Entities/User.cs
  Entities/Tweet.cs
  Entities/Follow.cs
  Entities/Like.cs
  Entities/RefreshToken.cs
  Exceptions/DomainException.cs
backend/src/TwitterClone.Application/
  TwitterClone.Application.csproj
  Abstractions/Repositories/IUserRepository.cs
  Abstractions/Repositories/ITweetRepository.cs
  Abstractions/Repositories/IFollowRepository.cs
  Abstractions/Repositories/ILikeRepository.cs
  Abstractions/Auth/IPasswordHasher.cs
  Abstractions/Auth/IJwtService.cs
  Abstractions/Auth/IRefreshTokenRepository.cs
  Abstractions/IClock.cs
  Features/Auth/Commands/RegisterCommand.cs
  Features/Auth/Commands/RegisterHandler.cs
  Features/Auth/Commands/RegisterValidator.cs
  Features/Auth/Commands/LoginCommand.cs
  Features/Auth/Commands/LoginHandler.cs
  Features/Auth/Commands/RefreshCommand.cs
  Features/Auth/Commands/RefreshHandler.cs
  Features/Auth/Commands/LogoutCommand.cs
  Features/Auth/Commands/LogoutHandler.cs
  Features/Auth/DTOs/AuthResult.cs
  Common/Errors/AppError.cs
  DependencyInjection.cs
backend/src/TwitterClone.Infrastructure/
  TwitterClone.Infrastructure.csproj
  Persistence/AppDbContext.cs
  Persistence/Configurations/UserConfiguration.cs
  Persistence/Configurations/TweetConfiguration.cs
  Persistence/Configurations/FollowConfiguration.cs
  Persistence/Configurations/LikeConfiguration.cs
  Persistence/Configurations/RefreshTokenConfiguration.cs
  Persistence/Repositories/UserRepository.cs
  Persistence/Repositories/TweetRepository.cs
  Persistence/Repositories/FollowRepository.cs
  Persistence/Repositories/LikeRepository.cs
  Auth/BcryptPasswordHasher.cs
  Auth/JwtService.cs
  Auth/RefreshTokenRepository.cs
  SystemClock.cs
  DependencyInjection.cs
backend/src/TwitterClone.Api/
  TwitterClone.Api.csproj
  Program.cs
  Endpoints/AuthEndpoints.cs
  Middleware/ErrorHandlingMiddleware.cs
  DependencyInjection.cs
  appsettings.json
  appsettings.Development.json
backend/tests/TwitterClone.Domain.Tests/
  TwitterClone.Domain.Tests.csproj
  Entities/UserTests.cs
  Entities/TweetTests.cs
backend/tests/TwitterClone.Application.Tests/
  TwitterClone.Application.Tests.csproj
  Features/Auth/RegisterHandlerTests.cs
  Features/Auth/LoginHandlerTests.cs
backend/tests/TwitterClone.Integration.Tests/
  TwitterClone.Integration.Tests.csproj
  Fixtures/TestWebApplicationFactory.cs
  Auth/AuthEndpointsTests.cs
backend/tests/TwitterClone.Architecture.Tests/
  TwitterClone.Architecture.Tests.csproj
  LayerDependencyTests.cs
```

### Frontend — new files

```
frontend/
  package.json
  vite.config.ts
  tailwind.config.ts
  postcss.config.ts
  tsconfig.json
  tsconfig.app.json
  tsconfig.node.json
  eslint.config.ts
  .prettierrc
  index.html
  playwright.config.ts
  src/
    main.ts
    App.vue
    types/auth.ts
    types/api.ts
    api/client.ts
    api/auth.ts
    stores/useAuthStore.ts
    router/index.ts
    views/LoginView.vue
    views/RegisterView.vue
    views/HomeView.vue
    components/AppLayout.vue
  tests/
    auth/login.test.ts
    auth/register.test.ts
  e2e/
    auth.spec.ts
```

---

## Task 1 — Backend solution skeleton

**Files:**
- Create: `backend/TwitterClone.sln` (via CLI)
- Create: 4 src project `.csproj` files (via CLI)
- Create: 4 test project `.csproj` files (via CLI)

- [ ] **Step 1.1: Create solution and source projects**

```powershell
cd C:\Users\alex2\repos\twitter_clone_ai_challenge

dotnet new sln -n TwitterClone -o backend

dotnet new classlib -n TwitterClone.Domain       -o backend/src/TwitterClone.Domain       --framework net10.0
dotnet new classlib -n TwitterClone.Application  -o backend/src/TwitterClone.Application  --framework net10.0
dotnet new classlib -n TwitterClone.Infrastructure -o backend/src/TwitterClone.Infrastructure --framework net10.0
dotnet new webapi   -n TwitterClone.Api          -o backend/src/TwitterClone.Api          --framework net10.0 --use-minimal-apis
```

- [ ] **Step 1.2: Create test projects**

```powershell
dotnet new xunit -n TwitterClone.Domain.Tests        -o backend/tests/TwitterClone.Domain.Tests        --framework net10.0
dotnet new xunit -n TwitterClone.Application.Tests   -o backend/tests/TwitterClone.Application.Tests   --framework net10.0
dotnet new xunit -n TwitterClone.Integration.Tests   -o backend/tests/TwitterClone.Integration.Tests   --framework net10.0
dotnet new xunit -n TwitterClone.Architecture.Tests  -o backend/tests/TwitterClone.Architecture.Tests  --framework net10.0
```

- [ ] **Step 1.3: Add all projects to solution**

```powershell
dotnet sln backend/TwitterClone.sln add `
  backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj `
  backend/src/TwitterClone.Application/TwitterClone.Application.csproj `
  backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj `
  backend/src/TwitterClone.Api/TwitterClone.Api.csproj `
  backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj `
  backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj `
  backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj `
  backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj
```

- [ ] **Step 1.4: Wire Clean Architecture project references**

```powershell
# Application → Domain
dotnet add backend/src/TwitterClone.Application/TwitterClone.Application.csproj `
  reference backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj

# Infrastructure → Application + Domain
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj `
  reference backend/src/TwitterClone.Application/TwitterClone.Application.csproj
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj `
  reference backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj

# Api → Application + Infrastructure (DI only)
dotnet add backend/src/TwitterClone.Api/TwitterClone.Api.csproj `
  reference backend/src/TwitterClone.Application/TwitterClone.Application.csproj
dotnet add backend/src/TwitterClone.Api/TwitterClone.Api.csproj `
  reference backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj

# Test project references
dotnet add backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj `
  reference backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj

dotnet add backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj `
  reference backend/src/TwitterClone.Application/TwitterClone.Application.csproj
dotnet add backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj `
  reference backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj

dotnet add backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj `
  reference backend/src/TwitterClone.Api/TwitterClone.Api.csproj

dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj `
  reference backend/src/TwitterClone.Domain/TwitterClone.Domain.csproj
dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj `
  reference backend/src/TwitterClone.Application/TwitterClone.Application.csproj
dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj `
  reference backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj
dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj `
  reference backend/src/TwitterClone.Api/TwitterClone.Api.csproj
```

- [ ] **Step 1.5: Add NuGet packages**

```powershell
# Application
dotnet add backend/src/TwitterClone.Application/TwitterClone.Application.csproj package FluentValidation

# Infrastructure
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj package BCrypt.Net-Next
dotnet add backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer

# Api
dotnet add backend/src/TwitterClone.Api/TwitterClone.Api.csproj package FluentValidation.AspNetCore

# Domain.Tests
dotnet add backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj package FluentAssertions
dotnet add backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj package coverlet.collector

# Application.Tests
dotnet add backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj package FluentAssertions
dotnet add backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj package NSubstitute
dotnet add backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj package coverlet.collector

# Integration.Tests
dotnet add backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj package FluentAssertions
dotnet add backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj package Testcontainers.PostgreSql
dotnet add backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj package coverlet.collector

# Architecture.Tests
dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj package NetArchTest.Rules
dotnet add backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj package FluentAssertions
```

- [ ] **Step 1.6: Delete auto-generated boilerplate**

```powershell
# classlib generates Class1.cs; webapi generates WeatherForecast
Remove-Item backend/src/TwitterClone.Domain/Class1.cs -ErrorAction SilentlyContinue
Remove-Item backend/src/TwitterClone.Application/Class1.cs -ErrorAction SilentlyContinue
Remove-Item backend/src/TwitterClone.Infrastructure/Class1.cs -ErrorAction SilentlyContinue
Remove-Item backend/src/TwitterClone.Api/WeatherForecast.cs -ErrorAction SilentlyContinue
# Also remove the UnitTest1.cs stubs
Get-ChildItem backend/tests -Recurse -Filter "UnitTest1.cs" | Remove-Item
```

- [ ] **Step 1.7: Write minimal Program.cs (just boots, no features yet)**

Create `backend/src/TwitterClone.Api/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Needed for WebApplicationFactory in Integration.Tests
public partial class Program { }
```

- [ ] **Step 1.8: Verify solution builds**

```powershell
dotnet build backend/TwitterClone.sln -warnaserror
```

Expected: `Build succeeded. 0 Warning(s). 0 Error(s).`

- [ ] **Step 1.9: Commit**

```bash
git add backend/
git commit -m "build: scaffold backend clean-architecture solution"
```

---

## Task 2 — Frontend skeleton

**Files:**
- Create: all `frontend/` files listed in file map

- [ ] **Step 2.1: Scaffold Vite + Vue + TypeScript**

```powershell
cd C:\Users\alex2\repos\twitter_clone_ai_challenge
npm create vite@latest frontend -- --template vue-ts
cd frontend
npm install
```

- [ ] **Step 2.2: Install dependencies**

```powershell
# State, routing, HTTP
npm install vue-router@4 pinia axios

# Tailwind
npm install -D tailwindcss@4 postcss autoprefixer @tailwindcss/vite

# Testing
npm install -D vitest @vue/test-utils @vitejs/plugin-vue jsdom
npm install -D @playwright/test
npm install -D eslint @typescript-eslint/eslint-plugin @typescript-eslint/parser eslint-plugin-vue
npm install -D prettier eslint-config-prettier

# Types
npm install -D @types/node
```

- [ ] **Step 2.3: Write `vite.config.ts`**

```typescript
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: process.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: [],
  },
})
```

- [ ] **Step 2.4: Write `tailwind.config.ts`**

```typescript
import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{vue,ts}'],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#1DA1F2',
          dark: '#0d8bd9',
        },
      },
    },
  },
  plugins: [],
} satisfies Config
```

- [ ] **Step 2.5: Write `src/main.ts`**

```typescript
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './style.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
```

- [ ] **Step 2.6: Write `src/style.css` (Tailwind entry)**

```css
@import "tailwindcss";
```

- [ ] **Step 2.7: Write `src/App.vue`**

```vue
<script setup lang="ts">
import AppLayout from '@/components/AppLayout.vue'
</script>

<template>
  <AppLayout />
</template>
```

- [ ] **Step 2.8: Write `src/components/AppLayout.vue`**

```vue
<script setup lang="ts">
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <RouterView />
  </div>
</template>
```

- [ ] **Step 2.9: Write `src/types/auth.ts`**

```typescript
export interface AuthResult {
  accessToken: string
  user: UserProfile
}

export interface UserProfile {
  id: string
  username: string
  email: string
  bio: string | null
  avatarUrl: string | null
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}
```

- [ ] **Step 2.10: Write `src/types/api.ts`**

```typescript
export interface ProblemDetails {
  type?: string
  title: string
  status: number
  detail?: string
  errors?: Record<string, string[]>
}

export interface PagedResult<T> {
  items: T[]
  nextCursor: string | null
}
```

- [ ] **Step 2.11: Write `src/router/index.ts`**

```typescript
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/useAuthStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/login', name: 'login', component: () => import('@/views/LoginView.vue') },
    { path: '/register', name: 'register', component: () => import('@/views/RegisterView.vue') },
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue'),
      meta: { requiresAuth: true },
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login' }
  }
})

export default router
```

- [ ] **Step 2.12: Write placeholder views**

`src/views/HomeView.vue`:
```vue
<script setup lang="ts">
import { useAuthStore } from '@/stores/useAuthStore'
const auth = useAuthStore()
</script>
<template>
  <div class="p-4">
    <h1 class="text-xl font-bold">Home</h1>
    <p class="text-gray-500">Logged in as {{ auth.user?.username }}</p>
  </div>
</template>
```

`src/views/LoginView.vue` and `src/views/RegisterView.vue` — placeholder only (full implementation in Task 11):

```vue
<!-- LoginView.vue -->
<template><div class="p-4"><h1>Login</h1></div></template>
```

```vue
<!-- RegisterView.vue -->
<template><div class="p-4"><h1>Register</h1></div></template>
```

- [ ] **Step 2.13: Write `src/stores/useAuthStore.ts` (stub — full in Task 11)**

```typescript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserProfile } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<UserProfile | null>(null)

  const isAuthenticated = computed(() => accessToken.value !== null)

  function setAuth(token: string, profile: UserProfile) {
    accessToken.value = token
    user.value = profile
  }

  function clearAuth() {
    accessToken.value = null
    user.value = null
  }

  return { accessToken, user, isAuthenticated, setAuth, clearAuth }
})
```

- [ ] **Step 2.14: Write `src/api/client.ts` (stub — interceptor added in Task 11)**

```typescript
import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
  withCredentials: true,
})
```

- [ ] **Step 2.15: Write `src/api/auth.ts` (stub — full in Task 11)**

```typescript
import { apiClient } from './client'
import type { AuthResult, LoginRequest, RegisterRequest } from '@/types/auth'

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<AuthResult>('/api/auth/login', data).then((r) => r.data),

  register: (data: RegisterRequest) =>
    apiClient.post<AuthResult>('/api/auth/register', data).then((r) => r.data),

  refresh: () =>
    apiClient.post<{ accessToken: string }>('/api/auth/refresh').then((r) => r.data),

  logout: () => apiClient.post('/api/auth/logout'),
}
```

- [ ] **Step 2.16: Write `package.json` scripts section (add missing scripts)**

Edit `frontend/package.json` to ensure these scripts exist:
```json
{
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc --noEmit && vite build",
    "preview": "vite preview",
    "type-check": "vue-tsc --noEmit",
    "lint": "eslint . --ext .vue,.ts",
    "format": "prettier --write .",
    "test:unit": "vitest run",
    "test:unit:watch": "vitest",
    "test:e2e": "playwright test"
  }
}
```

- [ ] **Step 2.17: Write `playwright.config.ts`**

```typescript
import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://localhost:5173',
  },
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
})
```

- [ ] **Step 2.18: Write `.prettierrc`**

```json
{
  "semi": false,
  "singleQuote": true,
  "printWidth": 100,
  "trailingComma": "es5"
}
```

- [ ] **Step 2.19: Write `.env` from example**

```powershell
Copy-Item .env.example .env
```

- [ ] **Step 2.20: Verify frontend type-checks**

```powershell
cd frontend
npm run type-check
```

Expected: no errors.

- [ ] **Step 2.21: Commit**

```bash
cd ..
git add frontend/
git commit -m "build: scaffold vue frontend"
```

---

## Task 3 — Docker Postgres + backend healthcheck

**Files:**
- Modify: `backend/src/TwitterClone.Api/Program.cs`
- Modify: `backend/src/TwitterClone.Api/appsettings.json`

- [ ] **Step 3.1: Start Postgres via docker-compose**

```powershell
docker-compose up -d postgres
docker-compose ps
```

Expected: `twitterclone-db` shows `healthy`.

- [ ] **Step 3.2: Add Postgres connection to `appsettings.Development.json`**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=twitterclone;Username=twitter;Password=change-me-local-dev"
  }
}
```

- [ ] **Step 3.3: Verify backend boots and healthcheck responds**

```powershell
cd backend/src/TwitterClone.Api
dotnet run &
Start-Sleep -Seconds 3
Invoke-RestMethod http://localhost:8080/health
```

Expected: `{ "status": "healthy" }`.

```powershell
Stop-Process -Name "TwitterClone.Api" -ErrorAction SilentlyContinue
```

- [ ] **Step 3.4: Commit**

```bash
git add backend/src/TwitterClone.Api/appsettings.Development.json
git commit -m "build: postgres via docker-compose"
```

---

## Task 4 — Architecture tests (NetArchTest)

**Files:**
- Create: `backend/tests/TwitterClone.Architecture.Tests/LayerDependencyTests.cs`

- [ ] **Step 4.1: Write the architecture tests**

```csharp
using NetArchTest.Rules;
using FluentAssertions;

namespace TwitterClone.Architecture.Tests;

public class LayerDependencyTests
{
    private const string DomainNs = "TwitterClone.Domain";
    private const string ApplicationNs = "TwitterClone.Application";
    private const string InfrastructureNs = "TwitterClone.Infrastructure";
    private const string ApiNs = "TwitterClone.Api";

    private static readonly Types AllTypes = Types.InAssemblies(
    [
        typeof(TwitterClone.Domain.Entities.User).Assembly,
        typeof(TwitterClone.Application.DependencyInjection).Assembly,
        typeof(TwitterClone.Infrastructure.DependencyInjection).Assembly,
        typeof(Program).Assembly,
    ]);

    [Fact]
    public void Domain_Should_Not_Reference_Any_Other_Project()
    {
        var result = Types.InAssembly(typeof(TwitterClone.Domain.Entities.User).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.FailingTypeNames?.Aggregate((a, b) => $"{a}, {b}"));
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(TwitterClone.Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.FailingTypeNames?.Aggregate((a, b) => $"{a}, {b}"));
    }

    [Fact]
    public void Infrastructure_Should_Not_Reference_Api()
    {
        var result = Types.InAssembly(typeof(TwitterClone.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(result.FailingTypeNames?.Aggregate((a, b) => $"{a}, {b}"));
    }
}
```

Note: This test references `TwitterClone.Application.DependencyInjection` and `TwitterClone.Infrastructure.DependencyInjection` — create those stubs now.

- [ ] **Step 4.2: Create `Application/DependencyInjection.cs` stub**

```csharp
namespace TwitterClone.Application;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
```

- [ ] **Step 4.3: Create `Infrastructure/DependencyInjection.cs` stub**

```csharp
namespace TwitterClone.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        return services;
    }
}
```

- [ ] **Step 4.4: Run architecture tests**

```powershell
dotnet test backend/tests/TwitterClone.Architecture.Tests/TwitterClone.Architecture.Tests.csproj -v normal
```

Expected: 3 tests pass.

- [ ] **Step 4.5: Commit**

```bash
git add backend/
git commit -m "test: clean-architecture boundary rules"
```

---

## Task 5 — Domain entities + domain tests

**Files:**
- Create: `backend/src/TwitterClone.Domain/Entities/User.cs`
- Create: `backend/src/TwitterClone.Domain/Entities/Tweet.cs`
- Create: `backend/src/TwitterClone.Domain/Entities/Follow.cs`
- Create: `backend/src/TwitterClone.Domain/Entities/Like.cs`
- Create: `backend/src/TwitterClone.Domain/Entities/RefreshToken.cs`
- Create: `backend/src/TwitterClone.Domain/Exceptions/DomainException.cs`
- Create: `backend/tests/TwitterClone.Domain.Tests/Entities/UserTests.cs`
- Create: `backend/tests/TwitterClone.Domain.Tests/Entities/TweetTests.cs`

- [ ] **Step 5.1: Write domain exception**

```csharp
// backend/src/TwitterClone.Domain/Exceptions/DomainException.cs
namespace TwitterClone.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

- [ ] **Step 5.2: Write failing domain entity tests**

```csharp
// backend/tests/TwitterClone.Domain.Tests/Entities/UserTests.cs
using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ValidArgs_ReturnsUser()
    {
        var user = User.Create("alice", "alice@example.com", "hashedpw");
        user.Username.Should().Be("alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashedpw");
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyUsername_ThrowsDomainException(string username)
    {
        var act = () => User.Create(username, "a@b.com", "hash");
        act.Should().Throw<DomainException>().WithMessage("*username*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_InvalidEmail_ThrowsDomainException(string email)
    {
        var act = () => User.Create("alice", email, "hash");
        act.Should().Throw<DomainException>().WithMessage("*email*");
    }
}
```

```csharp
// backend/tests/TwitterClone.Domain.Tests/Entities/TweetTests.cs
using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests.Entities;

public class TweetTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();

    [Fact]
    public void Create_ValidText_ReturnsTweet()
    {
        var tweet = Tweet.Create(AuthorId, "Hello world");
        tweet.Text.Should().Be("Hello world");
        tweet.AuthorId.Should().Be(AuthorId);
        tweet.ParentId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyText_ThrowsDomainException(string text)
    {
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().Throw<DomainException>().WithMessage("*text*");
    }

    [Fact]
    public void Create_TextOver280Chars_ThrowsDomainException()
    {
        var text = new string('x', 281);
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().Throw<DomainException>().WithMessage("*280*");
    }

    [Fact]
    public void Create_TextExactly280Chars_Succeeds()
    {
        var text = new string('x', 280);
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().NotThrow();
    }
}
```

- [ ] **Step 5.3: Run tests — expect failure (entities don't exist yet)**

```powershell
dotnet test backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj
```

Expected: build errors / test failures.

- [ ] **Step 5.4: Write `User` entity**

```csharp
// backend/src/TwitterClone.Domain/Entities/User.cs
namespace TwitterClone.Domain.Entities;

using TwitterClone.Domain.Exceptions;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("username cannot be empty");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("email is invalid");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("passwordHash cannot be empty");

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateProfile(string? bio, string? avatarUrl)
    {
        Bio = bio;
        AvatarUrl = avatarUrl;
    }
}
```

- [ ] **Step 5.5: Write `Tweet` entity**

```csharp
// backend/src/TwitterClone.Domain/Entities/Tweet.cs
namespace TwitterClone.Domain.Entities;

using TwitterClone.Domain.Exceptions;

public class Tweet
{
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; } = default!;
    public Guid? ParentId { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tweet() { }

    public static Tweet Create(Guid authorId, string text, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("tweet text cannot be empty");
        if (text.Length > 280)
            throw new DomainException("tweet text cannot exceed 280 characters");

        return new Tweet
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Text = text.Trim(),
            ParentId = parentId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
```

- [ ] **Step 5.6: Write `Follow`, `Like`, `RefreshToken` entities**

```csharp
// backend/src/TwitterClone.Domain/Entities/Follow.cs
namespace TwitterClone.Domain.Entities;

public class Follow
{
    public Guid FollowerId { get; private set; }
    public Guid FolloweeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Follow() { }

    public static Follow Create(Guid followerId, Guid followeeId) =>
        new() { FollowerId = followerId, FolloweeId = followeeId, CreatedAt = DateTime.UtcNow };
}
```

```csharp
// backend/src/TwitterClone.Domain/Entities/Like.cs
namespace TwitterClone.Domain.Entities;

public class Like
{
    public Guid UserId { get; private set; }
    public Guid TweetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Like() { }

    public static Like Create(Guid userId, Guid tweetId) =>
        new() { UserId = userId, TweetId = tweetId, CreatedAt = DateTime.UtcNow };
}
```

```csharp
// backend/src/TwitterClone.Domain/Entities/RefreshToken.cs
namespace TwitterClone.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
        };

    public void Revoke() => IsRevoked = true;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired;
}
```

- [ ] **Step 5.7: Run domain tests — expect green**

```powershell
dotnet test backend/tests/TwitterClone.Domain.Tests/TwitterClone.Domain.Tests.csproj -v normal
```

Expected: 7 tests pass.

- [ ] **Step 5.8: Commit**

```bash
git add backend/
git commit -m "feat(domain): core entities and rules"
```

---

## Task 6 — EF Core DbContext + configurations + initial migration

**Files:**
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/AppDbContext.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Configurations/*.cs` (5 files)
- Modify: `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs`

- [ ] **Step 6.1: Write `AppDbContext.cs`**

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/AppDbContext.cs
namespace TwitterClone.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Domain.Entities;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tweet> Tweets => Set<Tweet>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 6.2: Write entity configurations**

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Configurations/UserConfiguration.cs
namespace TwitterClone.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(160);
        builder.Property(u => u.AvatarUrl).HasMaxLength(512);
    }
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Configurations/TweetConfiguration.cs
namespace TwitterClone.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

public class TweetConfiguration : IEntityTypeConfiguration<Tweet>
{
    public void Configure(EntityTypeBuilder<Tweet> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Text).HasMaxLength(280).IsRequired();
        builder.Property(t => t.ImageUrl).HasMaxLength(512);
        builder.HasIndex(t => new { t.AuthorId, t.CreatedAt });
    }
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Configurations/FollowConfiguration.cs
namespace TwitterClone.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FolloweeId });
    }
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Configurations/LikeConfiguration.cs
namespace TwitterClone.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(l => new { l.UserId, l.TweetId });
    }
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs
namespace TwitterClone.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TokenHash).IsRequired();
        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.HasIndex(r => r.UserId);
    }
}
```

- [ ] **Step 6.3: Register DbContext in Infrastructure DI**

```csharp
// backend/src/TwitterClone.Infrastructure/DependencyInjection.cs
namespace TwitterClone.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(connectionString));

        return services;
    }
}
```

- [ ] **Step 6.4: Wire DI in Program.cs**

```csharp
// backend/src/TwitterClone.Api/Program.cs
using TwitterClone.Application;
using TwitterClone.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
```

- [ ] **Step 6.5: Run initial migration**

Ensure docker-compose Postgres is running (`docker-compose up -d postgres`), then:

```powershell
dotnet ef migrations add InitialCreate `
  -p backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj `
  -s backend/src/TwitterClone.Api/TwitterClone.Api.csproj `
  -o Persistence/Migrations

dotnet ef database update `
  -p backend/src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj `
  -s backend/src/TwitterClone.Api/TwitterClone.Api.csproj
```

Expected: `Done. Applied migration 'InitialCreate'.`

- [ ] **Step 6.6: Build solution**

```powershell
dotnet build backend/TwitterClone.sln -warnaserror
```

Expected: 0 errors, 0 warnings.

- [ ] **Step 6.7: Commit**

```bash
git add backend/
git commit -m "feat(infra): ef core context and initial migration"
```

---

## Task 7 — Repository interfaces + EF implementations

**Files:**
- Create: `backend/src/TwitterClone.Application/Abstractions/Repositories/*.cs` (4 files)
- Create: `backend/src/TwitterClone.Application/Abstractions/IClock.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Persistence/Repositories/*.cs` (4 files)
- Create: `backend/src/TwitterClone.Infrastructure/SystemClock.cs`

- [ ] **Step 7.1: Write repository interfaces**

```csharp
// backend/src/TwitterClone.Application/Abstractions/Repositories/IUserRepository.cs
namespace TwitterClone.Application.Abstractions.Repositories;

using TwitterClone.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

```csharp
// backend/src/TwitterClone.Application/Abstractions/Repositories/ITweetRepository.cs
namespace TwitterClone.Application.Abstractions.Repositories;

using TwitterClone.Domain.Entities;

public interface ITweetRepository
{
    Task<Tweet?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Tweet tweet, CancellationToken ct = default);
    Task RemoveAsync(Tweet tweet, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

```csharp
// backend/src/TwitterClone.Application/Abstractions/Repositories/IFollowRepository.cs
namespace TwitterClone.Application.Abstractions.Repositories;

using TwitterClone.Domain.Entities;

public interface IFollowRepository
{
    Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);
    Task AddAsync(Follow follow, CancellationToken ct = default);
    Task RemoveAsync(Follow follow, CancellationToken ct = default);
    Task<List<Guid>> GetFolloweeIdsAsync(Guid followerId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

```csharp
// backend/src/TwitterClone.Application/Abstractions/Repositories/ILikeRepository.cs
namespace TwitterClone.Application.Abstractions.Repositories;

using TwitterClone.Domain.Entities;

public interface ILikeRepository
{
    Task<Like?> GetAsync(Guid userId, Guid tweetId, CancellationToken ct = default);
    Task AddAsync(Like like, CancellationToken ct = default);
    Task RemoveAsync(Like like, CancellationToken ct = default);
    Task<int> CountByTweetAsync(Guid tweetId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 7.2: Write `IClock` interface**

```csharp
// backend/src/TwitterClone.Application/Abstractions/IClock.cs
namespace TwitterClone.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
```

- [ ] **Step 7.3: Write EF repository implementations**

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Repositories/UserRepository.cs
namespace TwitterClone.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Domain.Entities;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Repositories/TweetRepository.cs
namespace TwitterClone.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Domain.Entities;

public class TweetRepository(AppDbContext db) : ITweetRepository
{
    public Task<Tweet?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Tweets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Tweet tweet, CancellationToken ct) =>
        await db.Tweets.AddAsync(tweet, ct);

    public Task RemoveAsync(Tweet tweet, CancellationToken ct)
    {
        db.Tweets.Remove(tweet);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Repositories/FollowRepository.cs
namespace TwitterClone.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Domain.Entities;

public class FollowRepository(AppDbContext db) : IFollowRepository
{
    public Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken ct) =>
        db.Follows.FirstOrDefaultAsync(
            f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);

    public async Task AddAsync(Follow follow, CancellationToken ct) =>
        await db.Follows.AddAsync(follow, ct);

    public Task RemoveAsync(Follow follow, CancellationToken ct)
    {
        db.Follows.Remove(follow);
        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetFolloweeIdsAsync(Guid followerId, CancellationToken ct) =>
        db.Follows
            .Where(f => f.FollowerId == followerId)
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
```

```csharp
// backend/src/TwitterClone.Infrastructure/Persistence/Repositories/LikeRepository.cs
namespace TwitterClone.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Domain.Entities;

public class LikeRepository(AppDbContext db) : ILikeRepository
{
    public Task<Like?> GetAsync(Guid userId, Guid tweetId, CancellationToken ct) =>
        db.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.TweetId == tweetId, ct);

    public async Task AddAsync(Like like, CancellationToken ct) =>
        await db.Likes.AddAsync(like, ct);

    public Task RemoveAsync(Like like, CancellationToken ct)
    {
        db.Likes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<int> CountByTweetAsync(Guid tweetId, CancellationToken ct) =>
        db.Likes.CountAsync(l => l.TweetId == tweetId, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
```

- [ ] **Step 7.4: Write `SystemClock`**

```csharp
// backend/src/TwitterClone.Infrastructure/SystemClock.cs
namespace TwitterClone.Infrastructure;

using TwitterClone.Application.Abstractions;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
```

- [ ] **Step 7.5: Register repos in Infrastructure DI**

Update `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs` — add after `services.AddDbContext`:

```csharp
using TwitterClone.Infrastructure.Persistence.Repositories;
// ... existing usings

services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ITweetRepository, TweetRepository>();
services.AddScoped<IFollowRepository, FollowRepository>();
services.AddScoped<ILikeRepository, LikeRepository>();
services.AddScoped<IClock, SystemClock>();
```

Full updated file:

```csharp
namespace TwitterClone.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Infrastructure.Persistence;
using TwitterClone.Infrastructure.Persistence.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITweetRepository, TweetRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IClock, SystemClock>();

        return services;
    }
}
```

- [ ] **Step 7.6: Build**

```powershell
dotnet build backend/TwitterClone.sln -warnaserror
```

Expected: 0 errors.

- [ ] **Step 7.7: Commit**

```bash
git add backend/
git commit -m "feat(infra): repositories"
```

---

## Task 8 — Auth abstractions + Application use cases

**Files:**
- Create: `backend/src/TwitterClone.Application/Abstractions/Auth/IPasswordHasher.cs`
- Create: `backend/src/TwitterClone.Application/Abstractions/Auth/IJwtService.cs`
- Create: `backend/src/TwitterClone.Application/Abstractions/Auth/IRefreshTokenRepository.cs`
- Create: `backend/src/TwitterClone.Application/Features/Auth/DTOs/AuthResult.cs`
- Create: `backend/src/TwitterClone.Application/Common/Errors/AppError.cs`
- Create: `backend/src/TwitterClone.Application/Features/Auth/Commands/Register*.cs` (3 files)
- Create: `backend/src/TwitterClone.Application/Features/Auth/Commands/Login*.cs` (2 files)
- Create: `backend/src/TwitterClone.Application/Features/Auth/Commands/Refresh*.cs` (2 files)
- Create: `backend/src/TwitterClone.Application/Features/Auth/Commands/Logout*.cs` (2 files)

- [ ] **Step 8.1: Write auth abstractions**

```csharp
// backend/src/TwitterClone.Application/Abstractions/Auth/IPasswordHasher.cs
namespace TwitterClone.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

```csharp
// backend/src/TwitterClone.Application/Abstractions/Auth/IJwtService.cs
namespace TwitterClone.Application.Abstractions.Auth;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string username, string email);
    string GenerateRefreshToken();
}
```

```csharp
// backend/src/TwitterClone.Application/Abstractions/Auth/IRefreshTokenRepository.cs
namespace TwitterClone.Application.Abstractions.Auth;

using TwitterClone.Domain.Entities;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 8.2: Write `AppError` + `AuthResult`**

```csharp
// backend/src/TwitterClone.Application/Common/Errors/AppError.cs
namespace TwitterClone.Application.Common.Errors;

public record AppError(string Code, string Message)
{
    public static readonly AppError EmailAlreadyExists = new("EMAIL_EXISTS", "Email is already registered.");
    public static readonly AppError UsernameAlreadyExists = new("USERNAME_EXISTS", "Username is already taken.");
    public static readonly AppError InvalidCredentials = new("INVALID_CREDENTIALS", "Email or password is incorrect.");
    public static readonly AppError InvalidRefreshToken = new("INVALID_REFRESH_TOKEN", "Refresh token is invalid or expired.");
}
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/DTOs/AuthResult.cs
namespace TwitterClone.Application.Features.Auth.DTOs;

public record AuthResult(
    string AccessToken,
    Guid UserId,
    string Username,
    string Email,
    string? Bio,
    string? AvatarUrl);
```

- [ ] **Step 8.3: Write Register use case**

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/RegisterCommand.cs
namespace TwitterClone.Application.Features.Auth.Commands;

public record RegisterCommand(string Username, string Email, string Password);
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/RegisterValidator.cs
namespace TwitterClone.Application.Features.Auth.Commands;

using FluentValidation;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/RegisterHandler.cs
namespace TwitterClone.Application.Features.Auth.Commands;

using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Application.Common.Errors;
using TwitterClone.Application.Features.Auth.DTOs;
using TwitterClone.Domain.Entities;

public class RegisterHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IClock clock)
{
    public async Task<(AuthResult? Result, string? RefreshTokenRaw, AppError? Error)> HandleAsync(
        RegisterCommand cmd, CancellationToken ct = default)
    {
        if (await users.ExistsByEmailAsync(cmd.Email, ct))
            return (null, null, AppError.EmailAlreadyExists);

        if (await users.ExistsByUsernameAsync(cmd.Username, ct))
            return (null, null, AppError.UsernameAlreadyExists);

        var user = User.Create(cmd.Username, cmd.Email, hasher.Hash(cmd.Password));
        await users.AddAsync(user, ct);

        var rawRefresh = jwt.GenerateRefreshToken();
        var tokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawRefresh)));

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenHash,
            clock.UtcNow.AddDays(7));

        await refreshTokens.AddAsync(refreshToken, ct);
        await users.SaveChangesAsync(ct);

        var access = jwt.GenerateAccessToken(user.Id, user.Username, user.Email);
        var result = new AuthResult(access, user.Id, user.Username, user.Email, user.Bio, user.AvatarUrl);

        return (result, rawRefresh, null);
    }
}
```

- [ ] **Step 8.4: Write Login use case**

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/LoginCommand.cs
namespace TwitterClone.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password);
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/LoginHandler.cs
namespace TwitterClone.Application.Features.Auth.Commands;

using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Application.Common.Errors;
using TwitterClone.Application.Features.Auth.DTOs;
using TwitterClone.Domain.Entities;

public class LoginHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IClock clock)
{
    public async Task<(AuthResult? Result, string? RefreshTokenRaw, AppError? Error)> HandleAsync(
        LoginCommand cmd, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(cmd.Email, ct);
        if (user is null || !hasher.Verify(cmd.Password, user.PasswordHash))
            return (null, null, AppError.InvalidCredentials);

        var rawRefresh = jwt.GenerateRefreshToken();
        var tokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawRefresh)));

        var refreshToken = RefreshToken.Create(user.Id, tokenHash, clock.UtcNow.AddDays(7));
        await refreshTokens.AddAsync(refreshToken, ct);
        await users.SaveChangesAsync(ct);

        var access = jwt.GenerateAccessToken(user.Id, user.Username, user.Email);
        var result = new AuthResult(access, user.Id, user.Username, user.Email, user.Bio, user.AvatarUrl);

        return (result, rawRefresh, null);
    }
}
```

- [ ] **Step 8.5: Write Refresh use case**

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/RefreshCommand.cs
namespace TwitterClone.Application.Features.Auth.Commands;

public record RefreshCommand(string RefreshToken);
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/RefreshHandler.cs
namespace TwitterClone.Application.Features.Auth.Commands;

using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Application.Common.Errors;
using TwitterClone.Domain.Entities;

public class RefreshHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    IJwtService jwt,
    IClock clock)
{
    public async Task<(string? AccessToken, string? NewRefreshToken, AppError? Error)> HandleAsync(
        RefreshCommand cmd, CancellationToken ct = default)
    {
        var tokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(cmd.RefreshToken)));

        var stored = await refreshTokens.GetByHashAsync(tokenHash, ct);
        if (stored is null || !stored.IsValid)
            return (null, null, AppError.InvalidRefreshToken);

        var user = await users.GetByIdAsync(stored.UserId, ct);
        if (user is null)
            return (null, null, AppError.InvalidRefreshToken);

        stored.Revoke();

        var rawRefresh = jwt.GenerateRefreshToken();
        var newHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawRefresh)));

        var newToken = RefreshToken.Create(user.Id, newHash, clock.UtcNow.AddDays(7));
        await refreshTokens.AddAsync(newToken, ct);
        await refreshTokens.SaveChangesAsync(ct);

        var access = jwt.GenerateAccessToken(user.Id, user.Username, user.Email);
        return (access, rawRefresh, null);
    }
}
```

- [ ] **Step 8.6: Write Logout use case**

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/LogoutCommand.cs
namespace TwitterClone.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken);
```

```csharp
// backend/src/TwitterClone.Application/Features/Auth/Commands/LogoutHandler.cs
namespace TwitterClone.Application.Features.Auth.Commands;

using TwitterClone.Application.Abstractions.Auth;

public class LogoutHandler(IRefreshTokenRepository refreshTokens)
{
    public async Task HandleAsync(LogoutCommand cmd, CancellationToken ct = default)
    {
        var tokenHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(cmd.RefreshToken)));

        var stored = await refreshTokens.GetByHashAsync(tokenHash, ct);
        if (stored is not null)
        {
            stored.Revoke();
            await refreshTokens.SaveChangesAsync(ct);
        }
    }
}
```

- [ ] **Step 8.7: Register handlers in Application DI**

```csharp
// backend/src/TwitterClone.Application/DependencyInjection.cs
namespace TwitterClone.Application;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Features.Auth.Commands;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
        return services;
    }
}
```

- [ ] **Step 8.8: Build**

```powershell
dotnet build backend/TwitterClone.sln -warnaserror
```

Expected: 0 errors.

- [ ] **Step 8.9: Commit**

```bash
git add backend/
git commit -m "feat(auth): register, login, refresh, logout"
```

---

## Task 9 — Infrastructure auth implementations + Auth endpoints

**Files:**
- Create: `backend/src/TwitterClone.Infrastructure/Auth/BcryptPasswordHasher.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Auth/JwtService.cs`
- Create: `backend/src/TwitterClone.Infrastructure/Auth/RefreshTokenRepository.cs`
- Create: `backend/src/TwitterClone.Api/Endpoints/AuthEndpoints.cs`
- Create: `backend/src/TwitterClone.Api/Middleware/ErrorHandlingMiddleware.cs`
- Modify: `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs`
- Modify: `backend/src/TwitterClone.Api/Program.cs`

- [ ] **Step 9.1: Write `BcryptPasswordHasher`**

```csharp
// backend/src/TwitterClone.Infrastructure/Auth/BcryptPasswordHasher.cs
namespace TwitterClone.Infrastructure.Auth;

using TwitterClone.Application.Abstractions.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
```

- [ ] **Step 9.2: Write `JwtService`**

```csharp
// backend/src/TwitterClone.Infrastructure/Auth/JwtService.cs
namespace TwitterClone.Infrastructure.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TwitterClone.Application.Abstractions.Auth;

public class JwtService(IConfiguration config) : IJwtService
{
    public string GenerateAccessToken(Guid userId, string username, string email)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]
                ?? throw new InvalidOperationException("Jwt:SigningKey missing")));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("username", username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var minutes = config.GetValue<int>("Jwt:AccessTokenMinutes", 15);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
```

- [ ] **Step 9.3: Write `RefreshTokenRepository`**

```csharp
// backend/src/TwitterClone.Infrastructure/Auth/RefreshTokenRepository.cs
namespace TwitterClone.Infrastructure.Auth;

using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Domain.Entities;
using TwitterClone.Infrastructure.Persistence;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
```

- [ ] **Step 9.4: Write `ErrorHandlingMiddleware`**

```csharp
// backend/src/TwitterClone.Api/Middleware/ErrorHandlingMiddleware.cs
namespace TwitterClone.Api.Middleware;

using System.Net;
using System.Text.Json;
using FluentValidation;
using TwitterClone.Application.Common.Errors;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ctx.Response.ContentType = "application/problem+json";
            var problem = new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Validation failed",
                status = 400,
                errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()),
            };
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/problem+json";
            var problem = new { title = "An unexpected error occurred.", status = 500 };
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
```

- [ ] **Step 9.5: Write `AuthEndpoints`**

```csharp
// backend/src/TwitterClone.Api/Endpoints/AuthEndpoints.cs
namespace TwitterClone.Api.Endpoints;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Application.Features.Auth.Commands;

public static class AuthEndpoints
{
    private const string RefreshTokenCookie = "refreshToken";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            [FromBody] RegisterCommand cmd,
            RegisterHandler handler,
            IValidator<RegisterCommand> validator,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            await validator.ValidateAndThrowAsync(cmd, ct);
            var (result, rawRefresh, error) = await handler.HandleAsync(cmd, ct);
            if (error is not null)
                return Results.Problem(error.Message, statusCode: 409, title: error.Code);
            SetRefreshCookie(ctx, rawRefresh!);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (
            [FromBody] LoginCommand cmd,
            LoginHandler handler,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var (result, rawRefresh, error) = await handler.HandleAsync(cmd, ct);
            if (error is not null)
                return Results.Problem(error.Message, statusCode: 401, title: error.Code);
            SetRefreshCookie(ctx, rawRefresh!);
            return Results.Ok(result);
        });

        group.MapPost("/refresh", async (
            RefreshHandler handler,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var rawRefresh = ctx.Request.Cookies[RefreshTokenCookie];
            if (string.IsNullOrEmpty(rawRefresh))
                return Results.Unauthorized();

            var (access, newRefresh, error) = await handler.HandleAsync(new RefreshCommand(rawRefresh), ct);
            if (error is not null)
                return Results.Unauthorized();

            SetRefreshCookie(ctx, newRefresh!);
            return Results.Ok(new { accessToken = access });
        });

        group.MapPost("/logout", async (
            LogoutHandler handler,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var rawRefresh = ctx.Request.Cookies[RefreshTokenCookie];
            if (!string.IsNullOrEmpty(rawRefresh))
                await handler.HandleAsync(new LogoutCommand(rawRefresh), ct);

            ctx.Response.Cookies.Delete(RefreshTokenCookie);
            return Results.NoContent();
        });

        return app;
    }

    private static void SetRefreshCookie(HttpContext ctx, string token)
    {
        ctx.Response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
        });
    }
}
```

- [ ] **Step 9.6: Update Infrastructure DI to register auth services**

Replace `backend/src/TwitterClone.Infrastructure/DependencyInjection.cs` with:

```csharp
namespace TwitterClone.Infrastructure;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Infrastructure.Auth;
using TwitterClone.Infrastructure.Persistence;
using TwitterClone.Infrastructure.Persistence.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITweetRepository, TweetRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey missing");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                };
            });

        services.AddAuthorization();

        return services;
    }
}
```

- [ ] **Step 9.7: Update `Program.cs` with middleware + CORS + auth + endpoints**

```csharp
// backend/src/TwitterClone.Api/Program.cs
using TwitterClone.Api.Endpoints;
using TwitterClone.Api.Middleware;
using TwitterClone.Application;
using TwitterClone.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapAuthEndpoints();

app.Run();

public partial class Program { }
```

- [ ] **Step 9.8: Update `appsettings.json` with JWT/CORS config structure**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "twitterclone",
    "Audience": "twitterclone-web",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

- [ ] **Step 9.9: Build**

```powershell
dotnet build backend/TwitterClone.sln -warnaserror
```

Expected: 0 errors.

- [ ] **Step 9.10: Commit**

```bash
git add backend/
git commit -m "feat(auth): protected routes"
```

---

## Task 10 — Auth Application tests (unit, mocked)

**Files:**
- Create: `backend/tests/TwitterClone.Application.Tests/Features/Auth/RegisterHandlerTests.cs`
- Create: `backend/tests/TwitterClone.Application.Tests/Features/Auth/LoginHandlerTests.cs`

- [ ] **Step 10.1: Write RegisterHandler tests**

```csharp
// backend/tests/TwitterClone.Application.Tests/Features/Auth/RegisterHandlerTests.cs
using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Application.Common.Errors;
using TwitterClone.Application.Features.Auth.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Features.Auth;

public class RegisterHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RegisterHandler _sut;

    public RegisterHandlerTests()
    {
        _clock.UtcNow.Returns(DateTime.UtcNow);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed-pw");
        _jwt.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("access-token");
        _jwt.GenerateRefreshToken().Returns(Convert.ToBase64String(new byte[64]));
        _sut = new RegisterHandler(_users, _hasher, _jwt, _refreshTokens, _clock);
    }

    [Fact]
    public async Task HandleAsync_NewUser_ReturnsAuthResult()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var (result, rawRefresh, error) = await _sut.HandleAsync(
            new RegisterCommand("alice", "alice@example.com", "password123"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.Username.Should().Be("alice");
        error.Should().BeNull();
        rawRefresh.Should().NotBeNull();
        await _users.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsEmailExistsError()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var (result, _, error) = await _sut.HandleAsync(
            new RegisterCommand("alice", "alice@example.com", "password123"));

        result.Should().BeNull();
        error.Should().Be(AppError.EmailAlreadyExists);
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateUsername_ReturnsUsernameExistsError()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _users.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var (result, _, error) = await _sut.HandleAsync(
            new RegisterCommand("alice", "other@example.com", "password123"));

        result.Should().BeNull();
        error.Should().Be(AppError.UsernameAlreadyExists);
    }
}
```

- [ ] **Step 10.2: Write LoginHandler tests**

```csharp
// backend/tests/TwitterClone.Application.Tests/Features/Auth/LoginHandlerTests.cs
using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Abstractions;
using TwitterClone.Application.Abstractions.Auth;
using TwitterClone.Application.Abstractions.Repositories;
using TwitterClone.Application.Common.Errors;
using TwitterClone.Application.Features.Auth.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Features.Auth;

public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly LoginHandler _sut;

    private static readonly User FakeUser =
        User.Create("alice", "alice@example.com", "hashed-pw");

    public LoginHandlerTests()
    {
        _clock.UtcNow.Returns(DateTime.UtcNow);
        _jwt.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("access-token");
        _jwt.GenerateRefreshToken().Returns(Convert.ToBase64String(new byte[64]));
        _sut = new LoginHandler(_users, _hasher, _jwt, _refreshTokens, _clock);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAuthResult()
    {
        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>())
            .Returns(FakeUser);
        _hasher.Verify("password123", "hashed-pw").Returns(true);

        var (result, rawRefresh, error) = await _sut.HandleAsync(
            new LoginCommand("alice@example.com", "password123"));

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        error.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        _users.GetByEmailAsync("alice@example.com", Arg.Any<CancellationToken>())
            .Returns(FakeUser);
        _hasher.Verify("wrong", "hashed-pw").Returns(false);

        var (result, _, error) = await _sut.HandleAsync(
            new LoginCommand("alice@example.com", "wrong"));

        result.Should().BeNull();
        error.Should().Be(AppError.InvalidCredentials);
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ReturnsInvalidCredentials()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var (result, _, error) = await _sut.HandleAsync(
            new LoginCommand("nobody@example.com", "password123"));

        result.Should().BeNull();
        error.Should().Be(AppError.InvalidCredentials);
    }
}
```

- [ ] **Step 10.3: Run Application unit tests**

```powershell
dotnet test backend/tests/TwitterClone.Application.Tests/TwitterClone.Application.Tests.csproj -v normal
```

Expected: 6 tests pass.

- [ ] **Step 10.4: Commit**

```bash
git add backend/
git commit -m "test(auth): integration + e2e auth flow"
```

---

## Task 11 — Integration tests (WebApplicationFactory + Testcontainers)

**Files:**
- Create: `backend/tests/TwitterClone.Integration.Tests/Fixtures/TestWebApplicationFactory.cs`
- Create: `backend/tests/TwitterClone.Integration.Tests/Auth/AuthEndpointsTests.cs`

- [ ] **Step 11.1: Write `TestWebApplicationFactory`**

```csharp
// backend/tests/TwitterClone.Integration.Tests/Fixtures/TestWebApplicationFactory.cs
namespace TwitterClone.Integration.Tests.Fixtures;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using TwitterClone.Infrastructure.Persistence;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("test_db")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with test container connection
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));
        });

        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "integration-test-secret-key-32-chars!!",
                ["Jwt:Issuer"] = "twitterclone",
                ["Jwt:Audience"] = "twitterclone-web",
                ["Jwt:AccessTokenMinutes"] = "15",
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
}
```

- [ ] **Step 11.2: Write auth integration tests**

```csharp
// backend/tests/TwitterClone.Integration.Tests/Auth/AuthEndpointsTests.cs
namespace TwitterClone.Integration.Tests.Auth;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Features.Auth.Commands;
using TwitterClone.Application.Features.Auth.DTOs;
using TwitterClone.Infrastructure.Persistence;
using TwitterClone.Integration.Tests.Fixtures;

public class AuthEndpointsTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ValidRequest_Returns200WithAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("alice", "alice@example.com", "password123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResult>();
        body!.AccessToken.Should().NotBeEmpty();
        body.Username.Should().Be("alice");
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("alice", "alice@example.com", "password123"));

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("alice2", "alice@example.com", "password123"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("bob", "bob@example.com", "password123"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand("bob@example.com", "password123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResult>();
        body!.AccessToken.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("carol", "carol@example.com", "password123"));

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand("carol@example.com", "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ValidCookie_Returns200NewAccessToken()
    {
        // Register to get cookie
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("dave", "dave@example.com", "password123"));
        regResponse.EnsureSuccessStatusCode();

        // Refresh using the cookie (HttpClient stores cookies automatically with UseCookies)
        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await refreshResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body!["accessToken"].Should().NotBeEmpty();
    }

    [Fact]
    public async Task Logout_RevokesToken_RefreshRetuns401()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("eve", "eve@example.com", "password123"));

        await _client.PostAsync("/api/auth/logout", null);

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

- [ ] **Step 11.3: Run integration tests (requires Docker)**

```powershell
dotnet test backend/tests/TwitterClone.Integration.Tests/TwitterClone.Integration.Tests.csproj -v normal
```

Expected: 5 tests pass (Testcontainers spins up Postgres, runs migrations, runs tests).

- [ ] **Step 11.4: Run all backend tests**

```powershell
dotnet test backend/TwitterClone.sln -v normal
```

Expected: all tests pass (domain + application + integration + architecture).

- [ ] **Step 11.5: Commit**

```bash
git add backend/
git commit -m "test(auth): integration + e2e auth flow"
```

---

## Task 12 — Frontend auth screens + interceptor + guards

**Files:**
- Modify: `frontend/src/views/LoginView.vue`
- Modify: `frontend/src/views/RegisterView.vue`
- Modify: `frontend/src/stores/useAuthStore.ts`
- Modify: `frontend/src/api/client.ts`

- [ ] **Step 12.1: Full `useAuthStore`**

```typescript
// frontend/src/stores/useAuthStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import type { UserProfile, LoginRequest, RegisterRequest } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<UserProfile | null>(null)
  const router = useRouter()

  const isAuthenticated = computed(() => accessToken.value !== null)

  function setAuth(token: string, profile: UserProfile) {
    accessToken.value = token
    user.value = profile
  }

  function clearAuth() {
    accessToken.value = null
    user.value = null
  }

  async function login(data: LoginRequest) {
    const result = await authApi.login(data)
    setAuth(result.accessToken, result.user)
    await router.push('/')
  }

  async function register(data: RegisterRequest) {
    const result = await authApi.register(data)
    setAuth(result.accessToken, result.user)
    await router.push('/')
  }

  async function logout() {
    await authApi.logout()
    clearAuth()
    await router.push('/login')
  }

  async function tryRefresh(): Promise<boolean> {
    try {
      const { accessToken: newToken } = await authApi.refresh()
      accessToken.value = newToken
      return true
    } catch {
      clearAuth()
      return false
    }
  }

  return { accessToken, user, isAuthenticated, setAuth, clearAuth, login, register, logout, tryRefresh }
})
```

- [ ] **Step 12.2: Axios client with refresh interceptor**

```typescript
// frontend/src/api/client.ts
import axios from 'axios'
import { useAuthStore } from '@/stores/useAuthStore'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080',
  withCredentials: true,
})

apiClient.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`
  }
  return config
})

let isRefreshing = false
let refreshQueue: Array<(token: string) => void> = []

apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config
    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }
    original._retry = true

    if (isRefreshing) {
      return new Promise((resolve) => {
        refreshQueue.push((token) => {
          original.headers.Authorization = `Bearer ${token}`
          resolve(apiClient(original))
        })
      })
    }

    isRefreshing = true
    const auth = useAuthStore()
    const ok = await auth.tryRefresh()
    isRefreshing = false

    if (ok) {
      refreshQueue.forEach((cb) => cb(auth.accessToken!))
      refreshQueue = []
      original.headers.Authorization = `Bearer ${auth.accessToken}`
      return apiClient(original)
    }

    refreshQueue = []
    return Promise.reject(error)
  },
)
```

- [ ] **Step 12.3: Update `authApi` to map backend response to `AuthResult`**

```typescript
// frontend/src/api/auth.ts
import { apiClient } from './client'
import type { AuthResult, LoginRequest, RegisterRequest } from '@/types/auth'

interface BackendAuthResult {
  accessToken: string
  userId: string
  username: string
  email: string
  bio: string | null
  avatarUrl: string | null
}

function mapResult(r: BackendAuthResult): AuthResult {
  return {
    accessToken: r.accessToken,
    user: {
      id: r.userId,
      username: r.username,
      email: r.email,
      bio: r.bio,
      avatarUrl: r.avatarUrl,
    },
  }
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<BackendAuthResult>('/api/auth/login', data).then((r) => mapResult(r.data)),

  register: (data: RegisterRequest) =>
    apiClient.post<BackendAuthResult>('/api/auth/register', data).then((r) => mapResult(r.data)),

  refresh: () =>
    apiClient.post<{ accessToken: string }>('/api/auth/refresh').then((r) => r.data),

  logout: () => apiClient.post('/api/auth/logout'),
}
```

- [ ] **Step 12.4: Write `LoginView.vue`**

```vue
<!-- frontend/src/views/LoginView.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import type { ProblemDetails } from '@/types/api'
import axios from 'axios'

const auth = useAuthStore()
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleSubmit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login({ email: email.value, password: password.value })
  } catch (e) {
    if (axios.isAxiosError(e)) {
      const problem = e.response?.data as ProblemDetails
      error.value = problem?.detail ?? 'Login failed'
    } else {
      error.value = 'Unexpected error'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-gray-50 px-4">
    <div class="w-full max-w-sm space-y-6 rounded-xl bg-white p-8 shadow">
      <h1 class="text-2xl font-bold text-gray-900">Sign in to The Flock</h1>

      <form class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-sm font-medium text-gray-700" for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            autocomplete="email"
            class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand focus:outline-none focus:ring-1 focus:ring-brand"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700" for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand focus:outline-none focus:ring-1 focus:ring-brand"
          />
        </div>

        <p v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</p>

        <button
          type="submit"
          :disabled="loading"
          class="w-full rounded-lg bg-brand py-2 text-sm font-semibold text-white hover:bg-brand-dark disabled:opacity-50"
        >
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-500">
        No account?
        <RouterLink to="/register" class="font-medium text-brand hover:underline">Register</RouterLink>
      </p>
    </div>
  </div>
</template>
```

- [ ] **Step 12.5: Write `RegisterView.vue`**

```vue
<!-- frontend/src/views/RegisterView.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/useAuthStore'
import type { ProblemDetails } from '@/types/api'
import axios from 'axios'

const auth = useAuthStore()
const username = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleSubmit() {
  error.value = ''
  loading.value = true
  try {
    await auth.register({ username: username.value, email: email.value, password: password.value })
  } catch (e) {
    if (axios.isAxiosError(e)) {
      const problem = e.response?.data as ProblemDetails
      error.value = problem?.detail ?? 'Registration failed'
    } else {
      error.value = 'Unexpected error'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-screen items-center justify-center bg-gray-50 px-4">
    <div class="w-full max-w-sm space-y-6 rounded-xl bg-white p-8 shadow">
      <h1 class="text-2xl font-bold text-gray-900">Create your account</h1>

      <form class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-sm font-medium text-gray-700" for="username">Username</label>
          <input
            id="username"
            v-model="username"
            type="text"
            required
            maxlength="50"
            autocomplete="username"
            class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand focus:outline-none focus:ring-1 focus:ring-brand"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700" for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            autocomplete="email"
            class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand focus:outline-none focus:ring-1 focus:ring-brand"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700" for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            minlength="8"
            autocomplete="new-password"
            class="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-brand focus:outline-none focus:ring-1 focus:ring-brand"
          />
        </div>

        <p v-if="error" class="text-sm text-red-600" role="alert">{{ error }}</p>

        <button
          type="submit"
          :disabled="loading"
          class="w-full rounded-lg bg-brand py-2 text-sm font-semibold text-white hover:bg-brand-dark disabled:opacity-50"
        >
          {{ loading ? 'Creating account…' : 'Create account' }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-500">
        Already have an account?
        <RouterLink to="/login" class="font-medium text-brand hover:underline">Sign in</RouterLink>
      </p>
    </div>
  </div>
</template>
```

- [ ] **Step 12.6: Type-check frontend**

```powershell
cd frontend
npm run type-check
```

Expected: no errors.

- [ ] **Step 12.7: Commit**

```bash
cd ..
git add frontend/
git commit -m "feat(web): auth screens and session"
```

---

## Task 13 — Frontend auth tests (Vitest)

**Files:**
- Create: `frontend/tests/auth/login.test.ts`
- Create: `frontend/tests/auth/register.test.ts`
- Create: `frontend/e2e/auth.spec.ts`

- [ ] **Step 13.1: Write login component test**

```typescript
// frontend/tests/auth/login.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import { useAuthStore } from '@/stores/useAuthStore'

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div>Home</div>' } },
    { path: '/login', component: LoginView },
    { path: '/register', component: { template: '<div>Register</div>' } },
  ],
})

describe('LoginView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('calls auth.login with form values on submit', async () => {
    const wrapper = mount(LoginView, {
      global: { plugins: [router, createPinia()] },
    })

    const auth = useAuthStore()
    auth.login = vi.fn().mockResolvedValue(undefined)

    await wrapper.find('input[type="email"]').setValue('alice@example.com')
    await wrapper.find('input[type="password"]').setValue('password123')
    await wrapper.find('form').trigger('submit.prevent')

    expect(auth.login).toHaveBeenCalledWith({
      email: 'alice@example.com',
      password: 'password123',
    })
  })

  it('shows error message on login failure', async () => {
    const wrapper = mount(LoginView, {
      global: { plugins: [router, createPinia()] },
    })

    const auth = useAuthStore()
    auth.login = vi.fn().mockRejectedValue(
      Object.assign(new Error(), {
        isAxiosError: true,
        response: { data: { detail: 'Invalid credentials' } },
      }),
    )

    await wrapper.find('input[type="email"]').setValue('x@x.com')
    await wrapper.find('input[type="password"]').setValue('wrong')
    await wrapper.find('form').trigger('submit.prevent')
    await new Promise((r) => setTimeout(r, 0))

    expect(wrapper.find('[role="alert"]').text()).toContain('Invalid credentials')
  })
})
```

- [ ] **Step 13.2: Write register component test**

```typescript
// frontend/tests/auth/register.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'
import RegisterView from '@/views/RegisterView.vue'
import { useAuthStore } from '@/stores/useAuthStore'

const router = createRouter({
  history: createMemoryHistory(),
  routes: [
    { path: '/', component: { template: '<div>Home</div>' } },
    { path: '/login', component: { template: '<div>Login</div>' } },
    { path: '/register', component: RegisterView },
  ],
})

describe('RegisterView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('calls auth.register with form values on submit', async () => {
    const wrapper = mount(RegisterView, {
      global: { plugins: [router, createPinia()] },
    })

    const auth = useAuthStore()
    auth.register = vi.fn().mockResolvedValue(undefined)

    await wrapper.find('input[id="username"]').setValue('alice')
    await wrapper.find('input[id="email"]').setValue('alice@example.com')
    await wrapper.find('input[id="password"]').setValue('password123')
    await wrapper.find('form').trigger('submit.prevent')

    expect(auth.register).toHaveBeenCalledWith({
      username: 'alice',
      email: 'alice@example.com',
      password: 'password123',
    })
  })
})
```

- [ ] **Step 13.3: Write Playwright E2E auth test**

```typescript
// frontend/e2e/auth.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Auth flow', () => {
  test('register, then login redirects to home', async ({ page }) => {
    const ts = Date.now()
    const email = `user${ts}@example.com`
    const username = `user${ts}`

    // Register
    await page.goto('/register')
    await page.fill('#username', username)
    await page.fill('#email', email)
    await page.fill('#password', 'password123')
    await page.click('button[type="submit"]')
    await expect(page).toHaveURL('/', { timeout: 5000 })

    // Home shows username
    await expect(page.locator('text=' + username)).toBeVisible()
  })

  test('login with wrong password shows error', async ({ page }) => {
    await page.goto('/login')
    await page.fill('#email', 'nobody@example.com')
    await page.fill('#password', 'wrongpassword')
    await page.click('button[type="submit"]')
    await expect(page.locator('[role="alert"]')).toBeVisible()
  })

  test('unauthenticated user redirected to login', async ({ page }) => {
    await page.goto('/')
    await expect(page).toHaveURL('/login')
  })
})
```

- [ ] **Step 13.4: Run Vitest unit tests**

```powershell
cd frontend
npm run test:unit
```

Expected: 3 tests pass.

- [ ] **Step 13.5: Commit**

```bash
cd ..
git add frontend/
git commit -m "test(web): auth flows"
```

---

## Final verification

- [ ] **Run full backend gate**

```powershell
pwsh scripts/check.ps1 -Backend
```

Expected: build green, all tests pass, coverage ≥ 85% (check `.logs/`).

- [ ] **Run full frontend gate**

```powershell
pwsh scripts/check.ps1 -Frontend
```

Expected: type-check clean, lint clean, Vitest green.

---

## Self-review checklist

### Spec coverage
| Requirement | Covered by task |
|---|---|
| Register (email + password) | Task 8, 11, 12 |
| Login / Logout | Task 8, 11, 12 |
| Route protection | Task 9, 12 router guard |
| JWT access token + httpOnly refresh cookie | Task 9, 11 |
| BCrypt hashing | Task 9 |
| Backend coverage ≥ 85% | Tasks 10, 11 |
| At least one E2E auth flow test | Task 13 |
| Domain entities (User, Tweet, Follow, Like) | Task 5 |
| EF migrations (postgres) | Task 6 |
| Architecture boundary enforcement | Task 4 |
| Frontend login/register screens | Task 12 |
| Axios refresh interceptor | Task 12 |
| Frontend auth tests (login flow) | Task 13 |

### What's NOT covered (M3–M6, ~60% remaining)
- Tweet create/delete, timeline, infinite scroll
- Follow/unfollow, like/unlike, like counter
- Profile page + follower/following lists
- User search
- Seed data
- README Runbook
- Docker full-stack (bonus)
- SignalR real-time (bonus)
