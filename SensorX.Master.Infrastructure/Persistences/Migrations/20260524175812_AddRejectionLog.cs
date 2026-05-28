using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedByStaffIds",
                table: "RFQs");

            migrationBuilder.AddColumn<string>(
                name: "RejectedLogs",
                table: "RFQs",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedLogs",
                table: "RFQs");

            migrationBuilder.AddColumn<string>(
                name: "RejectedByStaffIds",
                table: "RFQs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
