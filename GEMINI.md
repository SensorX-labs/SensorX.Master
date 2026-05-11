<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **SensorX.Master** (1415 symbols, 2967 relationships, 13 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/SensorX.Master/context` | Codebase overview, check index freshness |
| `gitnexus://repo/SensorX.Master/clusters` | All functional areas |
| `gitnexus://repo/SensorX.Master/processes` | All execution flows |
| `gitnexus://repo/SensorX.Master/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->

# SensorX.Master Project Memory

## Project Overview
- **Project Name:** SensorX.Master
- **Architecture:** Clean Architecture / DDD
- **Tech Stack:** .NET 9, EF Core, Npgsql, MediatR, MassTransit

## Recent Changes (2026-04-26)
- **Bogus Removal:** Completely removed the `Bogus` library and `BogusSeeder.cs` from the project as requested.
- **Program.cs Cleanup:**
    - Fixed `UseExceptionHandler()` crash by providing a delegate and adding `AddProblemDetails()`.
    - Removed redundant duplicate `app.Run()` and middleware calls.
    - Cleaned up the migration retry loop to exclude seeding logic.

## Recent Changes (2026-04-21)
- **Infrastructure Standards Synchronization:** Established standard query patterns across `SensorX.Master`, `SensorX.Data`, and `SensorX.Warehouse`.
    - Added `IQueryBuilder<T>`: Exposes `IQueryable` sources (Tracking and No-Tracking) without executing them.
    - Added `IQueryExecutor`: Handles materialization (`ToListAsync`, `FirstOrDefaultAsync`, etc.) and aggregation (`CountAsync`, `AnyAsync`), abstracting EF Core away from the Application layer.
    - Standardized directory structure: `Application\Common\Interfaces\` for interfaces and `Infrastructure\Persistences\` for implementations.
    - Updated `DI.cs` in all services to register these infrastructure components.
- **Bug Fix:** Fixed a bug in `Result.cs` where `Result.Success` would throw an `InvalidOperationException` due to an incorrect check in the constructor. Messages are now allowed (and default to "Success") for successful results.

## Recent Changes (2026-04-15)
- **Phone Value Object Integration:** Updated all `Phone` related fields to use the `Phone` value object instead of `string`.
    - `DeliveryInfo.ReceiverPhone` -> `Phone`
    - `CustomerInfo.RecipientPhone` -> `Phone`
    - Updated EF Core mappings in `OrderConfiguration`, `QuoteConfiguration`, and `RFQConfiguration` with `.HasConversion`.
    - Updated `OrderContextTests` and `QuoteAggregateTests` to use `Phone.Create()`.

## Recent Changes (2026-04-23)
- **Pagination System Synchronization:** Synchronized pagination patterns with `SensorX.Data`.
    - Added `OffsetPagination` and `KeysetPagination` in `Application\Common\QueryExtensions`.
    - Updated `GetPageListQuote` and `GetPageListRFQ` to use `OffsetPagination`.
    - Removed old `Common\Pagination` directory.

## Pagination System
### 1. Offset Pagination (`OffsetPagination` folder)
- **Use case**: Standard web tables with total page counts.
- **Base Query**: `OffsetPagedQuery` (contains `PageNumber`, `PageSize`).
- **Result Wrapper**: `OffsetPagedResult<T>` (contains `TotalCount`, `TotalPages`, etc.).
- **Extension**: `ApplyOffsetPagination(request)`.

### 2. Keyset Pagination (`KeysetPagination` folder)
- **Use case**: High-performance infinite scroll or large datasets.
- **Base Query**: `KeysetPagedQuery` (contains cursors).
- **Result Wrapper**: `KeysetPagedResult<T>` (contains cursors for next/previous).
- **Extension**: `ApplyKeysetPagination(request, createdAtSelector, idSelector)`.

## Infrastructure Standards
### Query Patterns
- **IQueryBuilder<T>**: Only provides `IQueryable` sources.
- **IQueryExecutor**: Executes `IQueryable` expressions (ToListAsync, CountAsync, etc.).
- **Usage Strategy**: Building queries in Application handlers using `IQueryBuilder` and executing them via `IQueryExecutor` to keep Domain/Application layers clean from ORM-specific logic.

## Domain Model
### Value Objects
- **Phone:** Validates Vietnamese phone formats using Regex.
- **Email:** Standard email validation.
- **Code:** Business code validation.
- **Money:** Currencies and amounts.
- **Quantity:** Strictly positive quantity.
- **Percent:** Range [0, 100].

### Aggregates
- **RFQ:** Request for Quotation.
- **Quote:** Sales quotation.
- **Order:** Purchase order.
- **Invoice:** Billing information.
- **Warehouse:** Inventory storage.
- **SupplyRequest:** Internal supply request.
- **TransferOrder:** Stock transfer between warehouses.
