using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class testcommission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PncCommissionCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesArriveeId = table.Column<int>(type: "int", nullable: false),
                    DateCalcul = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalEncaisse = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionPnc1Individual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionCollective = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionPnc1FromCollective = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionTotalPnc1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NombrePncsTotal = table.Column<int>(type: "int", nullable: false),
                    CommissionParPnc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PncCommissionCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PncCommissionCalculations_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                        column: x => x.EtatDesVentesArriveeId,
                        principalTable: "EtatDesVentesArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PncCommissionDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PncCommissionCalculationId = table.Column<int>(type: "int", nullable: false),
                    PncId = table.Column<int>(type: "int", nullable: false),
                    IsPnc1 = table.Column<bool>(type: "bit", nullable: false),
                    CommissionIndividuelle = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionCollective = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionTotale = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PncCommissionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PncCommissionDetails_PncCommissionCalculations_PncCommissionCalculationId",
                        column: x => x.PncCommissionCalculationId,
                        principalTable: "PncCommissionCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PncCommissionDetails_Pncs_PncId",
                        column: x => x.PncId,
                        principalTable: "Pncs",
                        principalColumn: "PncId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PncCommissionCalculations_EtatDesVentesArriveeId",
                table: "PncCommissionCalculations",
                column: "EtatDesVentesArriveeId");

            migrationBuilder.CreateIndex(
                name: "IX_PncCommissionDetails_PncCommissionCalculationId",
                table: "PncCommissionDetails",
                column: "PncCommissionCalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_PncCommissionDetails_PncId",
                table: "PncCommissionDetails",
                column: "PncId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PncCommissionDetails");

            migrationBuilder.DropTable(
                name: "PncCommissionCalculations");
        }
    }
}
