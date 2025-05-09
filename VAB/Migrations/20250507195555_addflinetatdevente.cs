using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class addflinetatdevente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
