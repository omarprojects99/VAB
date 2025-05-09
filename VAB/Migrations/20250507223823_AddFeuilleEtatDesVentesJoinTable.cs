using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class AddFeuilleEtatDesVentesJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeuilleDeLignes_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                table: "FeuilleDeLignes");

            migrationBuilder.DropIndex(
                name: "IX_FeuilleDeLignes_EtatDesVentesArriveeId",
                table: "FeuilleDeLignes");

            migrationBuilder.DropColumn(
                name: "EtatDesVentesArriveeId",
                table: "FeuilleDeLignes");

            migrationBuilder.CreateTable(
                name: "FeuilleEtatDesVentesArrivee",
                columns: table => new
                {
                    FeuilleDeLignesId = table.Column<int>(type: "int", nullable: false),
                    EtatDesVentesArriveeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeuilleEtatDesVentesArrivee", x => new { x.FeuilleDeLignesId, x.EtatDesVentesArriveeId });
                    table.ForeignKey(
                        name: "FK_FeuilleEtatDesVentesArrivee_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                        column: x => x.EtatDesVentesArriveeId,
                        principalTable: "EtatDesVentesArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeuilleEtatDesVentesArrivee_FeuilleDeLignes_FeuilleDeLignesId",
                        column: x => x.FeuilleDeLignesId,
                        principalTable: "FeuilleDeLignes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeuilleEtatDesVentesArrivee_EtatDesVentesArriveeId",
                table: "FeuilleEtatDesVentesArrivee",
                column: "EtatDesVentesArriveeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeuilleEtatDesVentesArrivee");

            migrationBuilder.AddColumn<int>(
                name: "EtatDesVentesArriveeId",
                table: "FeuilleDeLignes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeuilleDeLignes_EtatDesVentesArriveeId",
                table: "FeuilleDeLignes",
                column: "EtatDesVentesArriveeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeuilleDeLignes_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                table: "FeuilleDeLignes",
                column: "EtatDesVentesArriveeId",
                principalTable: "EtatDesVentesArrivee",
                principalColumn: "Id");
        }
    }
}
