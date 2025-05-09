using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentSaisieIdToEtatDesVentesArrivee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentSaisieId",
                table: "EtatDesVentesArrivee",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesVentesArrivee_AgentSaisieId",
                table: "EtatDesVentesArrivee",
                column: "AgentSaisieId");

            migrationBuilder.AddForeignKey(
                name: "FK_EtatDesVentesArrivee_AspNetUsers_AgentSaisieId",
                table: "EtatDesVentesArrivee",
                column: "AgentSaisieId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EtatDesVentesArrivee_AspNetUsers_AgentSaisieId",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropIndex(
                name: "IX_EtatDesVentesArrivee_AgentSaisieId",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropColumn(
                name: "AgentSaisieId",
                table: "EtatDesVentesArrivee");
        }
    }
}
