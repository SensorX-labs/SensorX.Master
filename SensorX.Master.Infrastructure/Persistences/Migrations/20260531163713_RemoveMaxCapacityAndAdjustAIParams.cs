using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxCapacityAndAdjustAIParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "SaleStaffSnapshots");

            migrationBuilder.AlterColumn<double>(
                name: "K",
                schema: "read",
                table: "AIHyperparameters",
                type: "double precision",
                nullable: false,
                defaultValue: 3.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.5);

            migrationBuilder.AlterColumn<double>(
                name: "IdleWeight",
                schema: "read",
                table: "AIHyperparameters",
                type: "double precision",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 0.10000000000000001);

            migrationBuilder.UpdateData(
                schema: "read",
                table: "AIHyperparameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IdleWeight", "K" },
                values: new object[] { 1.0, 3.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "SaleStaffSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "K",
                schema: "read",
                table: "AIHyperparameters",
                type: "double precision",
                nullable: false,
                defaultValue: 1.5,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 3.0);

            migrationBuilder.AlterColumn<double>(
                name: "IdleWeight",
                schema: "read",
                table: "AIHyperparameters",
                type: "double precision",
                nullable: false,
                defaultValue: 0.10000000000000001,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldDefaultValue: 1.0);

            migrationBuilder.UpdateData(
                schema: "read",
                table: "AIHyperparameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IdleWeight", "K" },
                values: new object[] { 0.10000000000000001, 1.5 });
        }
    }
}
