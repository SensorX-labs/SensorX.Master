using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class initPaymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_ApiEndpointUrl",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ApiEndpointUrl",
                table: "Warehouses");

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Warehouses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "Warehouses",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "Payment_History",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Gateway = table.Column<string>(type: "text", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubAccount = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TransferType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TransferAmount = table.Column<decimal>(type: "numeric(18,0)", nullable: false),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    Accumulated = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment_History", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseInventoryProjections",
                columns: table => new
                {
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhysicalQuantity = table.Column<int>(type: "integer", nullable: false),
                    AllocatedQuantity = table.Column<int>(type: "integer", nullable: false),
                    WarehouseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrandZone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RackCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastSyncAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseInventoryProjections", x => new { x.WarehouseId, x.ProductId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment_History");

            migrationBuilder.DropTable(
                name: "WarehouseInventoryProjections");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Warehouses");

            migrationBuilder.AddColumn<string>(
                name: "ApiEndpointUrl",
                table: "Warehouses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_ApiEndpointUrl",
                table: "Warehouses",
                column: "ApiEndpointUrl",
                unique: true);
        }
    }
}
