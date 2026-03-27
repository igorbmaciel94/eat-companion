# The Atrium - Mindful Kitchen

[![CI](https://github.com/igorbmaciel94/eat-companion/actions/workflows/ci.yml/badge.svg)](https://github.com/igorbmaciel94/eat-companion/actions/workflows/ci.yml)
[![CodeQL](https://github.com/igorbmaciel94/eat-companion/actions/workflows/codeql.yml/badge.svg)](https://github.com/igorbmaciel94/eat-companion/actions/workflows/codeql.yml)

Aplicacao full-stack para gestao de planos alimentares. Faz parsing de PDFs de nutricionistas (em portugues), transforma em planos alimentares estruturados com opcoes por refeicao, e oferece tracking diario de calorias/macros, log de refeicoes, geracao de lista de compras e analytics de aderencia.

## Tech Stack

| Camada | Tecnologias |
|--------|-------------|
| **Frontend** | React 19, TypeScript, Vite 8, Tailwind CSS, Zustand, TanStack Query, Recharts |
| **Backend** | .NET 9, C#, Clean Architecture, Entity Framework Core, Serilog |
| **Banco de Dados** | PostgreSQL 16 |
| **IA** | Anthropic Claude API (parsing de PDFs) |
| **Infra** | Docker, docker-compose, Nginx |
| **Testes** | Vitest + Testing Library (frontend), xUnit (backend) |

## Estrutura do Projeto

```
eat-companion/
├── backend/
│   ├── src/
│   │   ├── EatCompanion.Api/            # Controllers, middleware, startup
│   │   ├── EatCompanion.Application/    # Use cases, DTOs, interfaces
│   │   ├── EatCompanion.Domain/         # Entidades, value objects
│   │   └── EatCompanion.Infrastructure/ # EF Core, Claude API, JWT
│   └── tests/
│       ├── EatCompanion.Domain.Tests/
│       └── EatCompanion.Infrastructure.Tests/
├── frontend/
│   └── src/
│       ├── api/          # Clients HTTP (Axios)
│       ├── components/   # Componentes reutilizaveis
│       ├── features/     # Feature modules
│       ├── stores/       # Zustand stores
│       └── types/        # TypeScript types
├── docker-compose.yml
└── .github/workflows/    # CI/CD e security scanning
```

## Pre-requisitos

- **Docker** e **docker-compose** (recomendado)
- Ou individualmente: .NET 9 SDK, Node.js 20+, PostgreSQL 16
- Chave de API da **Anthropic** (para importacao de PDFs)

## Como Rodar

### Com Docker (recomendado)

```bash
git clone https://github.com/igorbmaciel94/eat-companion.git
cd eat-companion

# Configurar a API key da Anthropic
export ANTHROPIC_API_KEY="sua-chave-aqui"

# Subir todos os servicos
docker-compose up --build
```

- Frontend: http://localhost:3000
- API: http://localhost:5001

### Desenvolvimento Local

**Backend:**

```bash
# Iniciar PostgreSQL (ou usar o container)
docker-compose up postgres -d

cd backend
dotnet restore
dotnet run --project src/EatCompanion.Api
```

API disponivel em http://localhost:5001

**Frontend:**

```bash
cd frontend
npm ci
npm run dev
```

App disponivel em http://localhost:5173

## Testes

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npm test
```

## Variaveis de Ambiente

| Variavel | Descricao | Obrigatoria |
|----------|-----------|:-----------:|
| `ANTHROPIC_API_KEY` | Chave da API Anthropic para parsing de PDFs | Sim |
| `ConnectionStrings__DefaultConnection` | Connection string do PostgreSQL | Sim |
| `Jwt__Secret` | Chave secreta para assinatura JWT (min 32 chars) | Sim |
| `Jwt__Issuer` | Issuer do token JWT | Sim |
| `Jwt__Audience` | Audience do token JWT | Sim |
| `ASPNETCORE_ENVIRONMENT` | Ambiente ASP.NET (`Development`, `Production`) | Nao |

## Seguranca

O projeto utiliza os seguintes mecanismos de seguranca no CI/CD:

- **CodeQL Analysis** - Analise estatica de codigo para C# e JavaScript/TypeScript
- **Dependency Review** - Verificacao de dependencias vulneraveis em PRs
- **Security Audit** - Auditoria de pacotes NuGet e npm
- **Secret Scanning** - Deteccao de segredos commitados (configurar no GitHub)

Para configurar a seguranca completa no repositorio, ative nas Settings do GitHub:
1. Code security > Secret scanning + Push protection
2. Code security > Dependabot alerts + Security updates
