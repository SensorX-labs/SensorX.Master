# SensorX.Master Project Memory

## Project Overview
- **Project Name:** SensorX.Master
- **Architecture:** Clean Architecture / DDD
- **Tech Stack:** .NET 9, EF Core, Npgsql, MediatR, MassTransit

## Recent Changes (2026-04-15)
- **Phone Value Object Integration:** Updated all `Phone` related fields to use the `Phone` value object instead of `string`.
    - `DeliveryInfo.ReceiverPhone` -> `Phone`
    - `CustomerInfo.RecipientPhone` -> `Phone`
    - Updated EF Core mappings in `OrderConfiguration`, `QuoteConfiguration`, and `RFQConfiguration` with `.HasConversion`.
    - Updated `OrderContextTests` and `QuoteAggregateTests` to use `Phone.Create()`.

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
