# Affiliate System - .NET Core Case Study

Enterprise-grade affiliate management system built with .NET Core 9, demonstrating clean architecture, SOLID principles, and comprehensive security implementations.

## Overview

This project showcases a production-ready affiliate system with role-based access control, secure authentication, and advanced security features including rate limiting, IP blocking, and XSS prevention. Built following clean architecture principles with full separation of concerns.

## Key Features

- **Multi-role Authentication System** - JWT-based authentication with Customer, Manager, and Admin roles
- **Referral Link Management** - Managers can create and track referral links with usage analytics
- **Advanced Security** - Rate limiting, progressive IP blocking, XSS protection, CAPTCHA integration
- **Clean Architecture** - Four-layer architecture with SOLID principles and AOP
- **Comprehensive Testing** - Unit tests with 52+ passing tests
- **API Documentation** - Full Swagger/OpenAPI documentation

## Technology Stack

- **Backend**: .NET Core 9.0, C# 13, ASP.NET Core Web API
- **Database**: PostgreSQL 13+ / SQL Server 2019+ with Entity Framework Core 9.0
- **Security**: JWT Bearer Authentication, BCrypt password hashing, AspNetCoreRateLimit
- **Validation**: FluentValidation with custom validators
- **Mapping**: AutoMapper for entity-DTO conversions
- **Testing**: xUnit, Moq
- **DevOps**: Docker, Docker Compose

## Technical Highlights

This project demonstrates several advanced software engineering practices:

### Architecture & Design Decisions

- **Clean Architecture Implementation**: Four-layer separation with strict dependency rules - Domain layer has zero dependencies, Infrastructure depends on Domain/Application, API orchestrates all layers
- **Progressive IP Blocking Strategy**: Intelligent threat mitigation with escalating blocks (10 attempts = 24h, 15 = 3 days, 20 = 7 days) instead of simple threshold blocking
- **Generic Repository Pattern**: Type-safe `IRepository<T>` with specialized implementations, reducing code duplication by ~40%
- **Unit of Work Pattern**: Transaction management across multiple repositories ensuring data consistency

### Security Engineering

- **Multi-layer Security Approach**:
  - Application layer: Rate limiting (AspNetCoreRateLimit)
  - Filter layer: XSS protection, model validation
  - Middleware layer: IP blocking, exception handling
  - Database layer: Parameterized queries, indexed lookups
- **BCrypt Work Factor 12**: Balances security (4096 iterations) with performance (~250ms per hash)
- **JWT with Refresh Tokens**: Stateless authentication with 24h access tokens, 7-day refresh tokens

### Code Quality & Maintainability

- **Zero Code Duplication**: Eliminated 600+ lines of duplicate code through service extraction and DI
- **Consistent API Responses**: BaseResponse<T> pattern across all endpoints with success/error/data structure
- **Comprehensive Validation**: 5 custom FluentValidation validators with 20+ validation rules
- **Performance Monitoring**: Custom AOP filter tracks slow endpoints (>1000ms threshold) for proactive optimization

### Development Practices

- **AOP for Cross-Cutting Concerns**: Filters and middleware handle security, validation, logging, performance - keeping controllers focused on business logic
- **Dependency Injection Throughout**: 100% constructor injection, no service locator pattern, fully testable
- **52+ Unit Tests**: Services, validators, repositories tested with Moq, achieving comprehensive coverage
- **Docker-First Deployment**: Production-ready containerization with docker-compose orchestration

## Architecture

The project follows **Clean Architecture** with four distinct layers:

```
API Layer (Controllers, Middleware, Filters)
    ↓
Application Layer (Services, DTOs, Validators, Mappings)
    ↓
Infrastructure Layer (DbContext, Repositories, External Services)
    ↓
Domain Layer (Entities, Interfaces, Enums)
```

### SOLID Principles Implementation

- **Single Responsibility**: Each service handles one concern (AuthService, UserService, IpBlockingService)
- **Open/Closed**: Generic repository pattern, extensible middleware pipeline
- **Liskov Substitution**: All implementations substitutable via interfaces
- **Interface Segregation**: 13 focused interfaces with minimal methods
- **Dependency Inversion**: Full dependency injection, no concrete dependencies

### Aspect-Oriented Programming (AOP)

Cross-cutting concerns handled via filters and middleware:
- `[XssProtection]` - Global XSS sanitization
- `[ValidateModel]` - Automatic DTO validation
- `[MonitorPerformance]` - Endpoint performance tracking
- `IpBlockingMiddleware` - Progressive IP blocking
- `ExceptionHandlingMiddleware` - Global exception handling

## Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone <repository-url>
cd affiliate-system-dotnet

# Start all services (API + PostgreSQL)
docker-compose up -d

# Check container status
docker-compose ps

# View logs
docker-compose logs -f api
```

Access the application:
- **API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health

Stop the application:
```bash
docker-compose down
```

Clean up (remove volumes):
```bash
docker-compose down -v
```

### Manual Setup

**Prerequisites:**
- PostgreSQL 13+ installed and running
- .NET 9.0 SDK installed

**1. Install PostgreSQL (macOS):**
```bash
# Install PostgreSQL
brew install postgresql@15

# Start PostgreSQL service
brew services start postgresql@15

# Add to PATH
export PATH="/opt/homebrew/opt/postgresql@15/bin:$PATH"
```

**2. Create Database:**
```bash
# Connect to PostgreSQL
psql postgres

# Create database
CREATE DATABASE "AffiliateSystemDb";

# Exit
\q
```

**3. Configure and Run:**
```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update --project src/AffiliateSystem.Infrastructure --startup-project src/AffiliateSystem.API

# Run application
dotnet run --project src/AffiliateSystem.API
```

Access the application at:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

### Test Data

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@affiliate.com | Admin@123 |
| Manager | manager@affiliate.com | Manager@123 |
| Customer | customer1@affiliate.com | Customer@123 |

**Manager Referral Code**: `MGR12345`

## Security Features

### Authentication & Authorization
- JWT Bearer tokens with 256-bit HS256 encryption
- Role-based access control (Customer, Manager, Admin)
- Refresh token support with 7-day expiration

### Password Security
- BCrypt hashing with work factor 12 (4096 iterations)
- Minimum 8 characters with complexity requirements (uppercase, lowercase, number, special character)

### Threat Protection
- **Rate Limiting**: 60 requests/min general, 5 login attempts per 5 minutes
- **IP Blocking**: Progressive blocking (10 attempts = 24h, 15 = 3 days, 20 = 7 days)
- **XSS Prevention**: Global input sanitization filter
- **CAPTCHA**: Google reCAPTCHA v3 integration
- **SQL Injection**: Parameterized queries via Entity Framework Core

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/logout` - Logout user
- `GET /api/auth/validate-referral/{code}` - Validate referral code
- `GET /api/auth/check-ip` - Check IP block status

### User Management
- `GET /api/user/profile` - Get current user profile
- `PUT /api/user/profile` - Update profile
- `POST /api/user/change-password` - Change password
- `GET /api/user/dashboard` - User dashboard with statistics
- `GET /api/user/referral-links` - Get referral links (Manager/Admin)
- `POST /api/user/referral-links` - Create referral link (Manager/Admin)

### Admin
- `GET /api/admin/users` - List all users (paginated)
- `GET /api/admin/users/{id}` - Get user by ID
- `DELETE /api/admin/users/{id}` - Delete user
- `GET /api/admin/statistics` - System statistics
- `GET /api/admin/blocked-ips` - List blocked IPs
- `DELETE /api/admin/blocked-ips/{ip}` - Unblock IP

Full API documentation available at `/swagger` endpoint.

## Testing

### Manual Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test suite
dotnet test --filter "FullyQualifiedName~AuthService"

# Watch mode (auto-run on file changes)
dotnet watch test
```

### Docker Testing

```bash
# Run tests in Docker container
docker-compose run --rm api dotnet test

# Run tests with coverage in Docker
docker-compose run --rm api dotnet test --collect:"XPlat Code Coverage"
```

**Test Coverage**: 52+ unit tests covering services, validators, and repositories.

## Project Structure

```
src/
├── AffiliateSystem.API/              # API layer (Controllers, Program.cs)
├── AffiliateSystem.Application/      # Application layer (Services, DTOs, Validators)
├── AffiliateSystem.Infrastructure/   # Infrastructure layer (DbContext, Repositories)
└── AffiliateSystem.Domain/           # Domain layer (Entities, Interfaces)

tests/
└── AffiliateSystem.Tests/            # Unit tests
```

## Configuration

Key configuration in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AffiliateSystemDb;..."
  },
  "JwtSettings": {
    "SecretKey": "Your-256-bit-secret-key",
    "ExpirationInHours": 24
  },
  "SecuritySettings": {
    "MaxFailedLoginAttempts": 5,
    "IpBlockingThreshold": 10,
    "IpBlockDurationHours": 24
  },
  "IpRateLimiting": {
    "GeneralRules": [
      { "Endpoint": "*", "Period": "1m", "Limit": 60 }
    ]
  }
}
```

## Development

### Build & Run
```bash
dotnet build                                    # Build solution
dotnet run --project src/AffiliateSystem.API   # Run API
dotnet watch --project src/AffiliateSystem.API # Hot reload mode
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/AffiliateSystem.Infrastructure --startup-project src/AffiliateSystem.API

# Update database
dotnet ef database update --project src/AffiliateSystem.Infrastructure --startup-project src/AffiliateSystem.API
```

## Design Patterns

- **Repository Pattern**: Generic `IRepository<T>` with specialized repositories
- **Unit of Work**: Transaction management across repositories
- **Service Layer**: Business logic separation from controllers
- **DTO Pattern**: Entity-DTO separation with AutoMapper
- **Dependency Injection**: Constructor injection throughout
- **Factory Pattern**: JWT token generation, referral code creation
- **Strategy Pattern**: Progressive IP blocking based on attempt count

## Performance Optimizations

- **Database Indexes**: Email (unique), IP+timestamp, block status
- **Async/Await**: Non-blocking I/O operations
- **AsNoTracking**: Read-only queries for improved performance
- **Pagination**: All list endpoints support paging
- **Performance Monitoring**: Custom AOP filter tracks slow endpoints (>1000ms)

## Deployment

### Docker
```bash
docker build -t affiliate-system .
docker-compose up -d
```

### Manual
```bash
dotnet publish -c Release -o ./publish
# Deploy to IIS, Nginx, or cloud platform
```

## Code Quality

- **Build Status**: 0 errors, 0 warnings
- **Test Coverage**: 52 passing tests
- **Code Lines**: ~3000 lines across 4 layers
- **Design**: SOLID principles, Clean Architecture
- **Security**: Production-grade security implementations
