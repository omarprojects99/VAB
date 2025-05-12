using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class donnegenerale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vols_FeuilleDeLignes_FeuilleDeLignesId",
                table: "Vols");

            migrationBuilder.AddColumn<string>(
                name: "Aircraft",
                table: "Vols",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EtatDesVentesArriveeId",
                table: "Vols",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EtatDesVentesArriveeId1",
                table: "Vols",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vols_EtatDesVentesArriveeId",
                table: "Vols",
                column: "EtatDesVentesArriveeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vols_EtatDesVentesArriveeId1",
                table: "Vols",
                column: "EtatDesVentesArriveeId1",
                unique: true,
                filter: "[EtatDesVentesArriveeId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                table: "Vols",
                column: "EtatDesVentesArriveeId",
                principalTable: "EtatDesVentesArrivee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_EtatDesVentesArrivee_EtatDesVentesArriveeId1",
                table: "Vols",
                column: "EtatDesVentesArriveeId1",
                principalTable: "EtatDesVentesArrivee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_FeuilleDeLignes_FeuilleDeLignesId",
                table: "Vols",
                column: "FeuilleDeLignesId",
                principalTable: "FeuilleDeLignes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vols_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                table: "Vols");

            migrationBuilder.DropForeignKey(
                name: "FK_Vols_EtatDesVentesArrivee_EtatDesVentesArriveeId1",
                table: "Vols");

            migrationBuilder.DropForeignKey(
                name: "FK_Vols_FeuilleDeLignes_FeuilleDeLignesId",
                table: "Vols");

            migrationBuilder.DropIndex(
                name: "IX_Vols_EtatDesVentesArriveeId",
                table: "Vols");

            migrationBuilder.DropIndex(
                name: "IX_Vols_EtatDesVentesArriveeId1",
                table: "Vols");

            migrationBuilder.DropColumn(
                name: "Aircraft",
                table: "Vols");

            migrationBuilder.DropColumn(
                name: "EtatDesVentesArriveeId",
                table: "Vols");

            migrationBuilder.DropColumn(
                name: "EtatDesVentesArriveeId1",
                table: "Vols");

            migrationBuilder.AddForeignKey(
                name: "FK_Vols_FeuilleDeLignes_FeuilleDeLignesId",
                table: "Vols",
                column: "FeuilleDeLignesId",
                principalTable: "FeuilleDeLignes",
                principalColumn: "Id");
        }
    }
}
