using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAIAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSnapshots");

            migrationBuilder.EnsureSchema(
                name: "read");

            migrationBuilder.AddColumn<int>(
                name: "CurrentWorkload",
                table: "SaleStaffSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAssignedAt",
                table: "SaleStaffSnapshots",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "SaleStaffSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectedByStaffIds",
                table: "RFQs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "StaffContextPerformances",
                schema: "read",
                columns: table => new
                {
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailureCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalMarginAccumulated = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffContextPerformances", x => new { x.StaffId, x.CategoryId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffContextPerformances",
                schema: "read");

            migrationBuilder.DropColumn(
                name: "CurrentWorkload",
                table: "SaleStaffSnapshots");

            migrationBuilder.DropColumn(
                name: "LastAssignedAt",
                table: "SaleStaffSnapshots");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "SaleStaffSnapshots");

            migrationBuilder.DropColumn(
                name: "RejectedByStaffIds",
                table: "RFQs");

            migrationBuilder.CreateTable(
                name: "ProductSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSnapshots", x => x.Id);
                });
        }
    }
}
