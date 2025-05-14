using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAB.Migrations
{
    /// <inheritdoc />
    public partial class articltesttwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EcartMoins",
                table: "EtatDesVentesArrivee",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EcartPlus",
                table: "EtatDesVentesArrivee",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEncaisse",
                table: "EtatDesVentesArrivee",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArticleVentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtatDesVentesArriveeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ArticleVentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleVentes_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArticleVentes_EtatDesVentesArrivee_EtatDesVentesArriveeId",
                        column: x => x.EtatDesVentesArriveeId,
                        principalTable: "EtatDesVentesArrivee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVentes_ArticleId",
                table: "ArticleVentes",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleVentes_EtatDesVentesArriveeId",
                table: "ArticleVentes",
                column: "EtatDesVentesArriveeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleVentes");

            migrationBuilder.DropColumn(
                name: "EcartMoins",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropColumn(
                name: "EcartPlus",
                table: "EtatDesVentesArrivee");

            migrationBuilder.DropColumn(
                name: "TotalEncaisse",
                table: "EtatDesVentesArrivee");
        }
    }
}
