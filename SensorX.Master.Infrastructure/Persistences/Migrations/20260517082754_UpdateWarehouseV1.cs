using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWarehouseV1 : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
