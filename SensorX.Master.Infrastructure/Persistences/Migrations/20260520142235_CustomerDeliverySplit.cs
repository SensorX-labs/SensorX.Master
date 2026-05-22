using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class CustomerDeliverySplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerShippingAddress",
                table: "RFQs");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "RFQs");

            migrationBuilder.DropColumn(
                name: "CustomerShippingAddress",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerShippingAddress",
                table: "RFQs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "RFQs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerShippingAddress",
                table: "Quotes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Quotes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
