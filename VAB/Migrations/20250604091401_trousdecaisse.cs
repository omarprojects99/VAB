using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class trousdecaisse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrouDeCaisses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesArriveeId = table.Column<int>(type: "int", nullable: false),
                    PncResponsableId = table.Column<int>(type: "int", nullable: false),
                    DateDetection = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontantCalcule = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantEncaisse = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantManquant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstApplique = table.Column<bool>(type: "bit", nullable: false),
                    DateApplication = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DetectePar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Commentaires = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrouDeCaisses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrouDeCaisses_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                        column: x => x.EtatDesVentesArriveeId,
                        principalTable: "EtatDesVentesArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrouDeCaisses_Pncs_PncResponsableId",
                        column: x => x.PncResponsableId,
                        principalTable: "Pncs",
                        principalColumn: "PncId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrouDeCaisses_EtatDesVentesArriveeId",
                table: "TrouDeCaisses",
                column: "EtatDesVentesArriveeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrouDeCaisses_PncResponsableId",
                table: "TrouDeCaisses",
                column: "PncResponsableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrouDeCaisses");
        }
    }
}
