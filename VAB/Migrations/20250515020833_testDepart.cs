using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class testDepart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtatDesVentesDepartId",
                table: "Vols",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtatDesVentesDepart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateEdition = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EquipageId = table.Column<int>(type: "int", nullable: false),
                    AgentSaisieId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalEncaisse = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EcartPlus = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EcartMoins = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesVentesDepart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtatDesVentesDepart_AspNetUsers_AgentSaisieId",
                        column: x => x.AgentSaisieId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtatDesVentesDepart_Equipages_EquipageId",
                        column: x => x.EquipageId,
                        principalTable: "Equipages",
                        principalColumn: "EquipageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleVenteDepart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesDepartId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotation = table.Column<int>(type: "int", nullable: false),
                    QuantiteComplementaire = table.Column<int>(type: "int", nullable: false),
                    QuantiteCasse = table.Column<int>(type: "int", nullable: false),
                    QuantiteOffert = table.Column<int>(type: "int", nullable: false),
                    QuantiteVendu = table.Column<int>(type: "int", nullable: false),
                    PrixUnitaireHT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleVenteDepart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleVenteDepart_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleVenteDepart_EtatDesVentesDepart_EtatDesVentesDepartId",
                        column: x => x.EtatDesVentesDepartId,
                        principalTable: "EtatDesVentesDepart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeuilleEtatDesVentesDepart",
                columns: table => new
                {
                    FeuilleDeLignesId = table.Column<int>(type: "int", nullable: false),
                    EtatDesVentesDepartId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeuilleEtatDesVentesDepart", x => new { x.FeuilleDeLignesId, x.EtatDesVentesDepartId });
                    table.ForeignKey(
                        name: "FK_FeuilleEtatDesVentesDepart_EtatDesVentesDepart_EtatDesVentesDepartId",
                        column: x => x.EtatDesVentesDepartId,
                        principalTable: "EtatDesVentesDepart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeuilleEtatDesVentesDepart_FeuilleDeLignes_FeuilleDeLignesId",
                        column: x => x.FeuilleDeLignesId,
                        principalTable: "FeuilleDeLignes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vols_EtatDesVentesDepartId",
                table: "Vols",
                column: "EtatDesVentesDepartId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVenteDepart_ArticleId",
                table: "ArticleVenteDepart",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVenteDepart_EtatDesVentesDepartId",
                table: "ArticleVenteDepart",
                column: "EtatDesVentesDepartId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesDepart_AgentSaisieId",
                table: "EtatDesVentesDepart",
                column: "AgentSaisieId");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesDepart_EquipageId",
                table: "EtatDesVentesDepart",
                column: "EquipageId");

            migrationBuilder.CreateIndex(
                name: "IX_FeuilleEtatDesVentesDepart_EtatDesVentesDepartId",
                table: "FeuilleEtatDesVentesDepart",
                column: "EtatDesVentesDepartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_EtatDesVentesDepart_EtatDesVentesDepartId",
                table: "Vols",
                column: "EtatDesVentesDepartId",
                principalTable: "EtatDesVentesDepart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vols_EtatDesVentesDepart_EtatDesVentesDepartId",
                table: "Vols");

            migrationBuilder.DropTable(
                name: "ArticleVenteDepart");

            migrationBuilder.DropTable(
                name: "FeuilleEtatDesVentesDepart");

            migrationBuilder.DropTable(
                name: "EtatDesVentesDepart");

            migrationBuilder.DropIndex(
                name: "IX_Vols_EtatDesVentesDepartId",
                table: "Vols");

            migrationBuilder.DropColumn(
                name: "EtatDesVentesDepartId",
                table: "Vols");
        }
    }
}
