using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class addcreatedbyinetatoffrearrivee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleOffres_EtatDesOffresDepart_EtatDesOffresDepartId",
                table: "ArticleOffres");

            migrationBuilder.DropIndex(
                name: "IX_ArticleOffres_EtatDesOffresDepartId",
                table: "ArticleOffres");

            migrationBuilder.DropColumn(
                name: "EtatDesOffresDepartId",
                table: "ArticleOffres");

            migrationBuilder.AddColumn<string>(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "e2de4e7b-7c7c-4eac-80dc-87ce590d4559");

            migrationBuilder.CreateIndex(
                name: "IX_EtatDesOffresArrivee_AgentSaisieId",
                table: "EtatDesOffresArrivee",
                column: "AgentSaisieId");

            migrationBuilder.AddForeignKey(
                name: "FK_EtatDesOffresArrivee_AspNetUsers_AgentSaisieId",
                table: "EtatDesOffresArrivee",
                column: "AgentSaisieId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EtatDesOffresArrivee_AspNetUsers_AgentSaisieId",
                table: "EtatDesOffresArrivee");

            migrationBuilder.DropIndex(
                name: "IX_EtatDesOffresArrivee_AgentSaisieId",
                table: "EtatDesOffresArrivee");

            migrationBuilder.DropColumn(
                name: "AgentSaisieId",
                table: "EtatDesOffresArrivee");

            migrationBuilder.AddColumn<int>(
                name: "EtatDesOffresDepartId",
                table: "ArticleOffres",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffres_EtatDesOffresDepartId",
                table: "ArticleOffres",
                column: "EtatDesOffresDepartId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleOffres_EtatDesOffresDepart_EtatDesOffresDepartId",
                table: "ArticleOffres",
                column: "EtatDesOffresDepartId",
                principalTable: "EtatDesOffresDepart",
                principalColumn: "Id");
        }
    }
}
