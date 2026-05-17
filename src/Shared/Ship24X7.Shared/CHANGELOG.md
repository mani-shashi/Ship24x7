# Ship24X7.Shared Changelog

All notable changes to this package will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2025-05-17

### Added

Initial packaged release of `Ship24X7.Shared` as a versioned NuGet package. Previously distributed as a `<ProjectReference>` across all services; services now consume it via `<PackageReference Include="Ship24X7.Shared" Version="1.0.0" />`.

**Included components:**

- `BaseEntity` — abstract base class providing audit fields (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`, `CorrelationId`)
- `BaseDomainEvent` — abstract base class providing common event fields for domain events
- `IDomainEvent` — interface contract for domain events
- `CorrelationIdMiddleware` — ASP.NET Core middleware that propagates `X-Correlation-Id` headers across service boundaries
- `GlobalExceptionMiddleware` — ASP.NET Core middleware that catches unhandled exceptions and returns structured error responses
- `ValidationBehaviour<TRequest, TResponse>` — MediatR pipeline behaviour that runs FluentValidation validators before handlers
- `SerilogConfiguration` — shared Serilog setup with file sink, environment enricher, and thread enricher
- `Email` — value object encapsulating a validated email address
- `Money` — value object encapsulating an amount and currency code
- `PhoneNumber` — value object encapsulating a validated phone number

### Migration Guide

Replace any `<ProjectReference>` to `Ship24X7.Shared.csproj` with:

```xml
<PackageReference Include="Ship24X7.Shared" Version="1.0.0" />
```

Ensure `NuGet.Config` at the solution root includes the `local` feed pointing to `./nuget-packages/` so the package can be resolved during `dotnet restore`.

### Breaking Changes

None — this is the initial packaged release. All public APIs are identical to the previous `<ProjectReference>` version.
