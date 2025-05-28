using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class correct3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleOffreDepart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesOffresDepartId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    QuantiteDotation = table.Column<int>(type: "int", nullable: false),
                    QuantiteComplementaire = table.Column<int>(type: "int", nullable: false),
                    QuantiteCasse = table.Column<int>(type: "int", nullable: false),
                    QuantiteOffert = table.Column<int>(type: "int", nullable: false),
                    RetourConsig = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleOffreDepart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleOffreDepart_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleOffreDepart_EtatDesOffresDepart_EtatDesOffresDepartId",
                        column: x => x.EtatDesOffresDepartId,
                        principalTable: "EtatDesOffresDepart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffreDepart_ArticleId",
                table: "ArticleOffreDepart",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleOffreDepart_EtatDesOffresDepartId",
                table: "ArticleOffreDepart",
                column: "EtatDesOffresDepartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleOffreDepart");
        }
    }
}
