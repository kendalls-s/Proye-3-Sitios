# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository overview

This repo contains independent ASP.NET Core (.NET 8) minimal-API microservices for a personnel/hiring system ("Sitios" project). Each service is named `SRV_Core<N>_<Nombre>` after the user story ("HU") it implements, and each is a fully separate solution — there is no shared class library between them. Endpoints, entities, repositories and services are duplicated per-service on purpose (e.g. `BitacoraRepository` exists identically in each project) so that every microservice can run independently without depending on another running service.

Current services:
- `SRV_Core1_ListadoPuestos` — `GET /puestos-activos` — all active puestos (`disponible = 1`), code and name only.
- `SRV_Core2_OferentesAptos` — `GET /oferentes-aptos/{codigoPuesto}` — oferentes that meet 100% of a puesto's requirements.
- `SRV_Core7_ListadoOferentes` — `GET /listado-oferentes/{codigoPuesto}` — full listing of oferentes for a puesto.
- `SRV_Core8_DetalleOferente` — `GET /detalle-oferente/{identificacion}` — detail of a single oferente.

`Colecciones Postman/` has one Postman collection per service for manual API testing.

## Commands

Each service has its own `.sln`/`.csproj`; run commands from inside the service folder (e.g. `SRV_Core2_OferentesAptos/`).

```
dotnet build                 # build a single service
dotnet run                   # run a single service (uses Properties/launchSettings.json)
dotnet build SRV_Core3_x.sln # build via the .sln instead of the .csproj
```

There are no automated tests in this repo currently; verification is done manually via the Postman collections in `Colecciones Postman/`.

Default local ports (from `Properties/launchSettings.json`, `ASPNETCORE_ENVIRONMENT=Development`):
- Core1 ListadoPuestos: `http://localhost:5200`
- Core2 OferentesAptos: `http://localhost:5201`
- Core7 ListadoOferentes: `http://localhost:5207`
- Core8 DetalleOferente: `http://localhost:5208`

## Architecture (per service)

Every service follows the same layering, namespaced as `Core<N>.<Feature>`:

- `Program.cs` — composition root: registers CORS policy `"ClientDev"` (allow any origin/header/method — dev-only), DI bindings, and maps the feature's endpoints extension method.
- `<Feature>Endpoints.cs` — a static class with a `Map<Feature>Endpoints(this IEndpointRouteBuilder)` extension defining a single `MapGroup` with one `GET` route. Route handlers validate the route parameter (when there is one — Core1's `/puestos-activos` takes none since it lists everything), delegate to the service layer, map "not found" results to 404, and wrap unexpected exceptions in a `bitacora`-logged 500 (`Results.Problem`). Successful (200) responses are wrapped in an envelope: `{ success: true, statusCode: 200, message: "<mensaje>", data: <payload> }` — inline anonymous objects in the endpoint, not a shared type (no shared code between services). 400/404/500 responses are NOT wrapped — they keep using `Results.BadRequest`/`Results.NotFound`/`Results.Problem` as-is.
- `Services/I<Feature>Service.cs` + `<Feature>Service.cs` — business logic. Talks to the repository layer and to `IBitacoraRepository`, and returns tuples/nullable DTOs that the endpoint translates into HTTP results (e.g. `(bool puestoExiste, IEnumerable<...> data)` to distinguish "puesto not found" from "no results").
- `Repository/DbConnectionFactory.cs` — `IDbConnectionFactory.CreateConnection()` returns a `MySqlConnection` built from `IConfiguration.GetConnectionString("DefaultConnection")`. Registered as a singleton.
- `Repository/<Feature>Repository.cs` — Dapper queries against MySQL views/tables (e.g. `vw_oferentes_aptos_puesto`) using raw parameterized SQL (`QueryAsync`, `QueryFirstOrDefaultAsync`, `ExecuteAsync`). Not registered behind an interface (concrete class in DI).
- `Repository/BitacoraRepository.cs` (`IBitacoraRepository`) — writes every query and technical error to a `bitacora` table in the same MySQL database, keeping the audit trail self-contained per service. Failures to write the bitácora are swallowed (logged to console) and never bubble up — audit logging must never break the main request flow.
- `Entities/*.cs` — plain DTOs returned directly by Dapper and serialized as the API response.

When adding a new `SRV_Core<N>_*` microservice, mirror this exact structure (Program.cs wiring, endpoints/services/repository split, and the same `BitacoraRepository` pattern) rather than trying to share code with existing services.

## Data access notes

- All services connect to the same shared MySQL database (connection string `DefaultConnection` in each `appsettings.json`) but only read/write the tables/views relevant to their own feature.
- SQL is raw Dapper (no EF Core, no migrations) — column names in `SELECT` are explicitly aliased to match C# entity property names (e.g. `id_oferente AS IdOferente`).
- `appsettings.json` in each service currently commits a live `DefaultConnection` connection string (host/user/password). Treat this as sensitive; don't propagate real credentials into new files or logs.
