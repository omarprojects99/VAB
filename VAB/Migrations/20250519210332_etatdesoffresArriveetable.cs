using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class etatdesoffresArriveetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtatDesOffresArriveeId",
                table: "ArticleVentes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtatDesOffresArrivee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtatDesOffresArrivee", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVentes_EtatDesOffresArriveeId",
                table: "ArticleVentes",
                column: "EtatDesOffresArriveeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleVentes_EtatDesOffresArrivee_EtatDesOffresArriveeId",
                table: "ArticleVentes",
                column: "EtatDesOffresArriveeId",
                principalTable: "EtatDesOffresArrivee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleVentes_EtatDesOffresArrivee_EtatDesOffresArriveeId",
                table: "ArticleVentes");

            migrationBuilder.DropTable(
                name: "EtatDesOffresArrivee");

            migrationBuilder.DropIndex(
                name: "IX_ArticleVentes_EtatDesOffresArriveeId",
                table: "ArticleVentes");

            migrationBuilder.DropColumn(
                name: "EtatDesOffresArriveeId",
                table: "ArticleVentes");
        }
    }
}
