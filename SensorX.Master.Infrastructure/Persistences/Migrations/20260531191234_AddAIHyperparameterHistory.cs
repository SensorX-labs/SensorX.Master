using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddAIHyperparameterHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SaleStaffSnapshots",
                newName: "SaleStaffSnapshots",
                newSchema: "read");

            migrationBuilder.RenameTable(
                name: "CustomerSnapshots",
                newName: "CustomerSnapshots",
                newSchema: "read");

            migrationBuilder.CreateTable(
                name: "AIHyperparameterHistories",
                schema: "read",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RFQId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    PredictedScore = table.Column<double>(type: "double precision", nullable: false),
                    KBefore = table.Column<double>(type: "double precision", nullable: false),
                    KAfter = table.Column<double>(type: "double precision", nullable: false),
                    DeltaK = table.Column<double>(type: "double precision", nullable: false),
                    IdleWeightBefore = table.Column<double>(type: "double precision", nullable: false),
                    IdleWeightAfter = table.Column<double>(type: "double precision", nullable: false),
                    DeltaIdleWeight = table.Column<double>(type: "double precision", nullable: false),
                    Loss = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIHyperparameterHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIHyperparameterHistories_RFQId",
                schema: "read",
                table: "AIHyperparameterHistories",
                column: "RFQId");

            migrationBuilder.CreateIndex(
                name: "IX_AIHyperparameterHistories_StaffId",
                schema: "read",
                table: "AIHyperparameterHistories",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AIHyperparameterHistories_Timestamp",
                schema: "read",
                table: "AIHyperparameterHistories",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIHyperparameterHistories",
                schema: "read");

            migrationBuilder.RenameTable(
                name: "SaleStaffSnapshots",
                schema: "read",
                newName: "SaleStaffSnapshots");

            migrationBuilder.RenameTable(
                name: "CustomerSnapshots",
                schema: "read",
                newName: "CustomerSnapshots");
        }
    }
}
