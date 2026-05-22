using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentModelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankTransactionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferContent",
                table: "Payments");

            migrationBuilder.AddColumn<List<string>>(
                name: "PaymentQRURls",
                table: "Payments",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Payment_History",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Payment_History_OrderId",
                table: "Payment_History",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_History_PaymentId_ReferenceCode",
                table: "Payment_History",
                columns: new[] { "PaymentId", "ReferenceCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_History_Payments_PaymentId",
                table: "Payment_History",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_History_Payments_PaymentId",
                table: "Payment_History");

            migrationBuilder.DropIndex(
                name: "IX_Payment_History_OrderId",
                table: "Payment_History");

            migrationBuilder.DropIndex(
                name: "IX_Payment_History_PaymentId_ReferenceCode",
                table: "Payment_History");

            migrationBuilder.DropColumn(
                name: "PaymentQRURls",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Payment_History");

            migrationBuilder.AddColumn<string>(
                name: "BankTransactionId",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TransactionDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "TransferContent",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
