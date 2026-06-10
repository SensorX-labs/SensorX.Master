using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorX.Master.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class AddAIHyperparameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIHyperparameters",
                schema: "read",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    K = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.5),
                    IdleWeight = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.10000000000000001),
                    LearningRate = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.01)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIHyperparameters", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "read",
                table: "AIHyperparameters",
                columns: new[] { "Id", "IdleWeight", "K", "LearningRate" },
                values: new object[] { 1, 0.10000000000000001, 1.5, 0.01 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIHyperparameters",
                schema: "read");
        }
    }
}
