using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Role",
                table: "Notifications",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Role_IsRead",
                table: "Notifications",
                columns: new[] { "Role", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_Role",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Role_IsRead",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });
        }
    }
}
