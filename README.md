# The Atrium - Mindful Kitchen

[![CI](https://github.com/igorbmaciel94/eat-companion/actions/workflows/ci.yml/badge.svg)](https://github.com/igorbmaciel94/eat-companion/actions/workflows/ci.yml)
[![CodeQL](https://github.com/igorbmaciel94/eat-companion/actions/workflows/codeql.yml/badge.svg)](https://github.com/igorbmaciel94/eat-companion/actions/workflows/codeql.yml)

A full-stack meal planning companion that parses nutritionist PDF meal plans (in Portuguese) into structured data with per-meal options, and provides daily calorie/macro tracking, meal logging (complete/substitute/skip), grocery list generation, and adherence analytics.

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| **Frontend** | React 19, TypeScript, Vite 8, Tailwind CSS, Zustand, TanStack Query, Recharts |
| **Backend** | .NET 9, C#, Clean Architecture, Entity Framework Core, Serilog |
| **Database** | PostgreSQL 16 |
| **AI** | Anthropic Claude API (PDF parsing) |
| **Infrastructure** | Docker, docker-compose, Nginx |
| **Testing** | Vitest + Testing Library (frontend), xUnit (backend) |

## Project Structure

```
eat-companion/
├── backend/
│   ├── src/
│   │   ├── EatCompanion.Api/            # Controllers, middleware, startup
│   │   ├── EatCompanion.Application/    # Use cases, DTOs, interfaces
│   │   ├── EatCompanion.Domain/         # Entities, value objects
│   │   └── EatCompanion.Infrastructure/ # EF Core, Claude API, JWT
│   └── tests/
│       ├── EatCompanion.Domain.Tests/
│       └── EatCompanion.Infrastructure.Tests/
├── frontend/
│   └── src/
│       ├── api/          # HTTP clients (Axios)
│       ├── components/   # Reusable components
│       ├── features/     # Feature modules
│       ├── stores/       # Zustand stores
│       └── types/        # TypeScript types
├── docker-compose.yml
└── .github/workflows/    # CI/CD and security scanning
```

## Prerequisites

- **Docker** and **docker-compose** (recommended)
- Or individually: .NET 9 SDK, Node.js 20+, PostgreSQL 16
- **Anthropic** API key (for PDF import feature)

## Getting Started

### With Docker (recommended)

```bash
git clone https://github.com/igorbmaciel94/eat-companion.git
cd eat-companion

# Set the Anthropic API key
export ANTHROPIC_API_KEY="your-key-here"

# Start all services
docker-compose up --build
```

- Frontend: http://localhost:3000
- API: http://localhost:5001

### Local Development

**Backend:**

```bash
# Start PostgreSQL (or use the container)
docker-compose up postgres -d

cd backend
dotnet restore
dotnet run --project src/EatCompanion.Api
```

API available at http://localhost:5001

**Frontend:**

```bash
cd frontend
npm ci
npm run dev
```

App available at http://localhost:5173

## Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npm test
```

## Environment Variables

| Variable | Description | Required |
|----------|-----------|:--------:|
| `ANTHROPIC_API_KEY` | Anthropic API key for PDF parsing | Yes |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | Yes |
| `Jwt__Secret` | JWT signing secret (min 32 chars) | Yes |
| `Jwt__Issuer` | JWT token issuer | Yes |
| `Jwt__Audience` | JWT token audience | Yes |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET environment (`Development`, `Production`) | No |

## Security

The project uses the following security mechanisms in CI/CD:

- **CodeQL Analysis** - Static code analysis for C# and JavaScript/TypeScript
- **Dependency Review** - Vulnerable dependency checks on PRs
- **Security Audit** - NuGet and npm package auditing
- **Secret Scanning** - Committed secrets detection (configure on GitHub)

To enable full repository security, activate in GitHub Settings:
1. Code security > Secret scanning + Push protection
2. Code security > Dependabot alerts + Security updates
