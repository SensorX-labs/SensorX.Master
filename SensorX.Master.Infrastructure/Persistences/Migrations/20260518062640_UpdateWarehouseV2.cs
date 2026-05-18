using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarehouseV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "WarehouseInventoryProjections");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Warehouses");
        }
    }
}
