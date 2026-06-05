# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Bus Ticket Booking & Management System** – A full-stack web application for passengers to book bus tickets and for operators (BusAdmins) and system administrators (SysAdmins) to manage fleets, trips, and analytics.

- **Backend:** ASP.NET Core 10 Web API (C#)
- **Frontend:** React 19 + Vite + Bun
- **Database:** PostgreSQL 18
- **ORM:** Entity Framework Core 10
- **State Management:** MobX
- **Styling:** Tailwind CSS + Shadcn UI
- **Authentication:** JWT + Google OAuth

## Directory Structure

```
server/                     # ASP.NET Core backend
├── Controllers/            # API routes (organized by Admin/, BusAdmin/, Auth/, Users/)
├── Services/               # Business logic (organized similarly: Admin/, BusAdmin/, Users/)
├── Models/                 # Domain entities (User, Bus, Trip, Ticket, etc.)
├── Dtos/                   # Request/Response DTOs
├── Data/                   # EF Core DbContext and migrations
├── Enums/                  # UserRole, TripStatus, BookingStatus, etc.
├── Configurations/         # DI configuration and options classes
├── Extensions/             # Extension methods
└── Utils/                  # Utility functions

web/                        # React frontend
├── src/
│   ├── pages/             # Route components (Main/, admin/, busadmin/)
│   ├── components/        # Reusable UI components (Shadcn)
│   ├── stores/            # MobX state stores
│   ├── api/               # Auto-generated API client (from Swagger)
│   └── utils/             # Helper functions
├── vite.config.ts         # Vite build config (@ alias for src/)
├── tsconfig.json          # TypeScript config
└── package.json           # Bun/npm dependencies
```

## Development Setup

### Prerequisites

- .NET SDK 10.0 or higher
- Bun 1.0+ (or Node.js as fallback)
- Docker + Docker Compose (for PostgreSQL)
- PowerShell or bash shell

### Initial Setup

1. **Clone & Environment:**
   ```powershell
   git clone <repo>
   cd pbl3
   cp .env.example .env  # or create manually
   ```

2. **Configure `.env` file:**
   ```ini
   POSTGRES_PASSWORD=your_db_password
   DATABASE_URL="Host=localhost;Database=pbl3;Username=pbl3;Password=your_db_password;Port=5432"
   JWT_KEY=your_super_secret_jwt_key_min_32_chars
   JWT_ISSUER=your_jwt_issuer
   JWT_AUDIENCE=your_jwt_audience
   GOOGLE_CLIENT_ID=your_client_id.apps.googleusercontent.com
   ```

3. **Start PostgreSQL:**
   ```powershell
   docker-compose up -d postgres
   ```

## Common Commands

### Backend (`server/`)

| Command | Purpose |
|---------|---------|
| `dotnet watch run` | Run with hot-reload (main development command) |
| `dotnet run` | Run without hot-reload |
| `dotnet ef database update` | Apply pending EF Core migrations |
| `dotnet ef migrations add <name>` | Create a new migration |
| `dotnet ef database drop` | Drop database (dev only) |
| `dotnet run -- --migrate` | Migrate and exit (CI/CD) |
| `dotnet run -- --seed` | Seed dummy data and exit |

**API Docs:** http://localhost:5026/swagger (when running)

### Frontend (`web/`)

| Command | Purpose |
|---------|---------|
| `bun dev` | Start Vite dev server (hot-reload) |
| `bun build` | Build for production |
| `bun run lint` | Run ESLint |
| `bun run generate-api` | Regenerate TypeScript API client from Swagger spec |
| `bun run format` | Format code with Prettier |

**Dev Server:** http://localhost:5173 (when running)

**Note:** `bun run generate-api` requires the backend running at http://localhost:5026. Runs `@hey-api/openapi-ts` to auto-generate type-safe API bindings.

## Architecture Patterns

### Backend Layer Organization

**Controllers** → **Services** → **EF DbContext**

- **Controllers:** Validate auth/input, call services, return DTOs
- **Services:** Business logic, database queries, external integrations (payments, Google Auth)
- **Models:** EF entity definitions (User, Bus, Trip, Ticket, Booking, Company, Station, etc.)
- **Dtos:** Request/response contracts (separate from models to version APIs independently)

### Authentication & Authorization

- **JWT Bearer Tokens:** Issued on login/Google signup; validated on every request
- **Roles:** `Passenger`, `BusAdmin`, `SysAdmin` (stored in JWT claims)
- **Policies:** `UserOnly` (any authenticated user), `BusAdmin` (BusAdmin+SysAdmin), `AdminOnly` (SysAdmin)
- **Endpoint Protection:** Controllers use `[Authorize(Policy = "...")]` attributes

Example:
```csharp
[HttpGet("admin-only")]
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> AdminEndpoint() { ... }
```

### Database Enums

Enums are mapped to PostgreSQL enum types in `Program.cs` (lines 115–128). When adding a new enum:
1. Define in `Enums/` folder
2. Add `dataSourceBuilder.MapEnum<YourEnum>()` in Program.cs
3. Create and apply EF migration

### Services Hierarchy

Key services by domain:

**Admin Services** (require `AdminOnly` or `BusAdmin` policy):
- `IRevenueAnalyticsService` – Dashboard metrics
- `ISystemAdminManagementService` – User/company CRUD
- `ICompanyProfileUpdateRequestService` – Review upgrade requests
- `IBusAdminUpgradeResponseService` – Process BusAdmin appeals
- `ITransactionManagementService` – Payment tracking
- `ITripMonitoringService` – System-wide trip oversight

**BusAdmin Services:**
- `IBusAdminBusesService` – Manage buses (CRUD + seat layout)
- `ITripDetailService` – Create/update/manage trips
- `IBusAdminProfileService` – Company profile & document uploads

**User Services:**
- `IBookingService` – Seat reservation & ticket creation
- `IPaymentService` – MoMo payment gateway integration
- `IRefundManagementService` – Cancellations & refunds
- `IReviewManagementService` – Ratings/reviews
- `ITripSearchService` – Route search & filtering
- `ILocationSearchService` – Station/city autocomplete

**Shared Services:**
- `IJwtTokenService` – Token generation/validation
- `ICurrentUserContext` – Extract user from request (via HttpContext)

### Frontend State Management (MobX)

Stores are in `web/src/stores/` (singleton pattern):
```typescript
// stores/user.state.ts
class UserStore {
  user: User | null = null;
  setUser(user: User) { this.user = user; }
  logout() { this.user = null; }
}

export const userStore = new UserStore();
```

Components subscribe with `observer()`:
```typescript
const MyComponent = observer(() => {
  return <div>{userStore.user?.name}</div>;
});
```

### API Client Generation

- Frontend uses auto-generated TypeScript client via `@hey-api/openapi-ts`
- Source: Backend Swagger spec at `/swagger/v1/swagger.json`
- **Do not hand-edit** `web/src/api/` – regenerate with `bun run generate-api`
- All API types are type-safe and validated against Swagger spec

## Database Migrations

EF Core migrations live in `server/Migrations/`. When schema changes:

1. **Modify** `Models/*.cs` (domain entities)
2. **Create migration:**
   ```powershell
   cd server
   dotnet ef migrations add DescriptiveNameHere
   ```
3. **Review** the generated `.cs` file in `Migrations/`
4. **Apply:**
   ```powershell
   dotnet ef database update
   ```

**Important:** Migrations are applied automatically in production via `docker-compose run --rm migrate`.

## Deployment

### Docker Compose

The stack is fully containerized:

```powershell
# Build images locally
docker-compose build

# Or pull pre-built from GitHub Container Registry (default)
docker-compose up -d

# Seed database (optional)
docker-compose run --rm seed
```

Services:
- **postgres:** Database (port 5432 via host networking)
- **server:** ASP.NET API (port 60010, built from `server/Dockerfile`)
- **migrate/seed:** One-off tools (profiles: ["tools"])

### Frontend Deployment

Build static files and deploy to Vercel, Netlify, or any static host:
```powershell
cd web
bun build
# Upload /dist folder
```

## Testing

- **Backend:** No centralized test suite visible; use `dotnet test` if tests exist
- **Frontend:** ESLint is configured; run `bun run lint` before committing

## Common Development Tasks

### Add a New API Endpoint

1. Create/modify a controller in `server/Controllers/` (or subdirectory)
2. Inject services into controller constructor
3. Implement endpoint method with `[HttpGet/Post/etc]` attribute
4. Return `Ok(dto)` or `BadRequest()` etc.
5. Regenerate frontend client: `cd web && bun run generate-api`
6. Use auto-generated client in React components

### Add a New Database Entity

1. Create model in `server/Models/`
2. Add `DbSet<YourModel>` to `ApplicationDbContext` in `server/Data/ApplicationDbContext.cs`
3. If entity uses enums, map them in `Program.cs`
4. Create and apply migration
5. Implement service in `server/Services/`
6. Create controller if needed

### Update Frontend Component

1. Edit `.tsx` file in `web/src/pages/` or `web/src/components/`
2. Use MobX stores for state: `import { userStore } from '@/stores/user.state'`
3. Use API client: `import { UserService } from '@/api/services/UserService'` (auto-generated)
4. Wrap with `observer()` if reading observable state
5. Test in `bun dev` at http://localhost:5173

### Regenerate API Client After Backend Changes

```powershell
# Ensure backend is running
dotnet watch run  # in server/

# In another terminal
cd web
bun run generate-api
```

## Important Notes

- **JWT Validation:** Tokens are validated on every request; no sessions
- **CORS:** Hardcoded to `http://localhost:5173` in dev (see `Program.cs` line 63)
- **API Auth Header:** Frontend automatically includes JWT via auto-generated client
- **Payment Gateway:** MoMo integration configured via `.env` (MOMO_* variables)
- **Database Connection:** Uses Npgsql; enums mapped to PostgreSQL native enums for efficiency
- **Seed Data:** Available via `dotnet run -- --seed` for local development

## Useful Links

- **Backend Swagger:** http://localhost:5026/swagger
- **Frontend Dev:** http://localhost:5173
- **Entity Framework Docs:** https://docs.microsoft.com/en-us/ef/core/
- **MobX Docs:** https://mobx.js.org/
- **Shadcn UI:** https://ui.shadcn.com/
- **Vite Docs:** https://vite.dev/
