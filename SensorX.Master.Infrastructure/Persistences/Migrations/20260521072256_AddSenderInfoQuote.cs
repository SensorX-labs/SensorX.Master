using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderInfoQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Quotes",
                newName: "SenderId");

            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "Quotes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Quotes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderPhone",
                table: "Quotes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "SenderPhone",
                table: "Quotes");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Quotes",
                newName: "CreatedBy");
        }
    }
}
