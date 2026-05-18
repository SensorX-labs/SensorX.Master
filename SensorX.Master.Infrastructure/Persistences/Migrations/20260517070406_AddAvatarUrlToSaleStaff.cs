using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlToSaleStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "SaleStaffSnapshots",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "SaleStaffSnapshots");
        }
    }
}
