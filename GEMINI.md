# SensorX.Master Project Memory

## Project Overview
- **Project Name:** SensorX.Master
- **Architecture:** Clean Architecture / DDD
- **Tech Stack:** .NET 9, EF Core, Npgsql, MediatR, MassTransit

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
