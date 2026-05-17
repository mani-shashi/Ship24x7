# Ship24X7 Platform

Enterprise-grade shipping and logistics management platform built on microservices architecture with ASP.NET Core 10.0.

---

## Running the App Manually

### Prerequisites

- .NET 10.0 SDK
- Docker Desktop (for SQL Server + RabbitMQ only)
- [direnv](https://direnv.net/) — auto-loads `.env` into your shell

### 1. Install direnv (once)

```bash
brew install direnv
echo 'eval "$(direnv hook zsh)"' >> ~/.zshrc
source ~/.zshrc
```

### 2. Configure environment

```bash
cp .env.example .env
# Edit .env with your values
direnv allow .
```

### 3. Start infrastructure (SQL Server + RabbitMQ)

```bash
docker compose up -d
```

### 4. Run services (each in a separate terminal tab)

```bash
# Gateway — http://localhost:8000
dotnet run --project src/Gateway/Ship24X7.Gateway/Ship24X7.Gateway.csproj

# Auth — http://localhost:9001
dotnet run --project src/Services/Auth/Ship24X7.Auth.API/Ship24X7.Auth.API.csproj

# Shipment — http://localhost:9002
dotnet run --project src/Services/Shipment/Ship24X7.Shipment.API/Ship24X7.Shipment.API.csproj

# Tracking — http://localhost:9003
dotnet run --project src/Services/Tracking/Ship24X7.Tracking.API/Ship24X7.Tracking.API.csproj

# Notification — http://localhost:9004
dotnet run --project src/Services/Notification/Ship24X7.Notification.API/Ship24X7.Notification.API.csproj

# Payment — http://localhost:9005
dotnet run --project src/Services/Payment/Ship24X7.Payment.API/Ship24X7.Payment.API.csproj
```

### 5. Access

| Service | URL |
|---|---|
| API Gateway | http://localhost:8000 |
| Auth Swagger | http://localhost:9001/swagger |
| Shipment Swagger | http://localhost:9002/swagger |
| Tracking Swagger | http://localhost:9003/swagger |
| Notification Swagger | http://localhost:9004/swagger |
| Payment Swagger | http://localhost:9005/swagger |
| RabbitMQ UI | http://localhost:15672 (ship24x7 / Ship24X7@RabbitPass) |

### Stop infrastructure

```bash
docker compose down        # stop
docker compose down -v     # stop + wipe all data
```

---

## Architecture Overview

The platform consists of:
- **5 Microservices**: Auth, Shipment, Tracking, Notification, Payment
- **API Gateway**: Ocelot-based gateway for unified entry point
- **Message Broker**: RabbitMQ for asynchronous communication
- **Database**: SQL Server with database-per-service pattern
- **Shared Library**: Common domain events, value objects, and middleware

## Project Structure

```
Ship24X7/
├── src/
│   ├── Services/
│   │   ├── Auth/                    # Authentication & Authorization Service (Port 9001)
│   │   ├── Shipment/                # Shipment Management Service (Port 9002)
│   │   ├── Tracking/                # Tracking & Delivery Service (Port 9003)
│   │   ├── Notification/            # Notification Service (Port 9004)
│   │   └── Payment/                 # Payment Service (Port 9005)
│   ├── Gateway/
│   │   └── Ship24X7.Gateway         # Ocelot API Gateway (Port 8000)
│   └── Shared/
│       └── Ship24X7.Shared          # Shared Infrastructure Library
├── .env                             # Local config — never commit
├── .env.example                     # Template — safe to commit
└── Ship24X7.sln
```

## Service Ports

| Service | Port | Database |
|---|---|---|
| API Gateway | 8000 | — |
| Auth Service | 9001 | Ship24X7_Auth |
| Shipment Service | 9002 | Ship24X7_Shipments |
| Tracking Service | 9003 | Ship24X7_Tracking |
| Notification Service | 9004 | Ship24X7_Notifications |
| Payment Service | 9005 | Ship24X7_Payment |

## Configuration

All config lives in `.env` using .NET's `Double__Underscore` format which maps directly to `Section:Key` in the config system. direnv loads it automatically when you `cd` into the project — no manual `export` needed.

Key variables:

| Variable | Used by |
|---|---|
| `Jwt__SecretKey` | All services |
| `ConnectionStrings__AuthDb` | Auth |
| `ConnectionStrings__RabbitMQ` | Shipment, Notification |
| `Email__SmtpUsername/Password` | Auth, Notification |
| `Razorpay__KeyId/KeySecret` | Payment |
| `Twilio__AccountSid/AuthToken` | Notification |
| `OAuth__Google__ClientId/Secret` | Auth |

## Shared Infrastructure

The `Ship24X7.Shared` library provides:

- **BaseEntity**: Audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CorrelationId)
- **CorrelationIdMiddleware**: Propagates unique CorrelationId across all service calls via `X-Correlation-Id` header
- **SerilogConfiguration**: Pre-configured structured logging — console + daily rolling file (30-day retention, 100 MB per file)

## Health Checks

| Service | URL |
|---|---|
| API Gateway | http://localhost:8000/health |
| Auth | http://localhost:9001/health |
| Shipment | http://localhost:9002/health |
| Tracking | http://localhost:9003/health |
| Notification | http://localhost:9004/health |
| Payment | http://localhost:9005/health |

## Database Migrations

Each service manages its own database schema via EF Core migrations:

```bash
# Example — Auth service
dotnet ef migrations add InitialCreate \
  --project src/Services/Auth/Ship24X7.Auth.Infrastructure \
  --startup-project src/Services/Auth/Ship24X7.Auth.API

dotnet ef database update \
  --project src/Services/Auth/Ship24X7.Auth.Infrastructure \
  --startup-project src/Services/Auth/Ship24X7.Auth.API
```

## Security

- **JWT**: 15-minute access tokens, 7-day refresh tokens with rotation
- **Secrets**: All in `.env`, never committed (enforced via `.gitignore`)
- **CORS**: Restricted to `Cors__AllowedOrigins`
- **Email verification**: Token link routed through Gateway → redirects to frontend
- **Password policy**: Min 8 chars, uppercase + lowercase + digit + special char

## License

Proprietary — All rights reserved
