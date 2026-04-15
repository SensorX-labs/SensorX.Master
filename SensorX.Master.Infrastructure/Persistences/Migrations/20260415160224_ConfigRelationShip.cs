using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class ConfigRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_DestinationWarehouseId",
                table: "TransferOrders",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_SupplyRequestId",
                table: "TransferOrders",
                column: "SupplyRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyRequests_WarehouseId",
                table: "SupplyRequests",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_RFQId",
                table: "Quotes",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_QuoteId",
                table: "Orders",
                column: "QuoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Quotes_QuoteId",
                table: "Orders",
                column: "QuoteId",
                principalTable: "Quotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_RFQs_RFQId",
                table: "Quotes",
                column: "RFQId",
                principalTable: "RFQs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyRequests_Warehouses_WarehouseId",
                table: "SupplyRequests",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_SupplyRequests_SupplyRequestId",
                table: "TransferOrders",
                column: "SupplyRequestId",
                principalTable: "SupplyRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferOrders_Warehouses_DestinationWarehouseId",
                table: "TransferOrders",
                column: "DestinationWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Quotes_QuoteId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_RFQs_RFQId",
                table: "Quotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyRequests_Warehouses_WarehouseId",
                table: "SupplyRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_SupplyRequests_SupplyRequestId",
                table: "TransferOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferOrders_Warehouses_DestinationWarehouseId",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_DestinationWarehouseId",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransferOrders_SupplyRequestId",
                table: "TransferOrders");

            migrationBuilder.DropIndex(
                name: "IX_SupplyRequests_WarehouseId",
                table: "SupplyRequests");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_RFQId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_QuoteId",
                table: "Orders");
        }
    }
}
